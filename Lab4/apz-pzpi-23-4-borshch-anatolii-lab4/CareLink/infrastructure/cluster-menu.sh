#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
cd "$SCRIPT_DIR"

ARTIFACTS_DIR="artifacts"
BIN_DIR="$ARTIFACTS_DIR/bin"
BACKUPS_DIR="$ARTIFACTS_DIR/backups"
PORT_FORWARD_DIR="$ARTIFACTS_DIR/port-forward"
K8S_DIR="k8s"
RENDERED_DIR="$ARTIFACTS_DIR/rendered"
ENV_FILE="$SCRIPT_DIR/.cluster.env"
EXAMPLE_ENV_FILE="$SCRIPT_DIR/.cluster.env.example"

to_local_path() {
  local path="$1"
  if require_cmd cygpath; then
    cygpath -u "$path"
    return
  fi

  echo "$path"
}

if [[ ! -f "$ENV_FILE" && -f "$EXAMPLE_ENV_FILE" ]]; then
  cp "$EXAMPLE_ENV_FILE" "$ENV_FILE"
fi

if [[ -f "$ENV_FILE" ]]; then
  set -a
  source "$ENV_FILE"
  set +a
fi

NAMESPACE="${NAMESPACE:-carelink}"
API_DEPLOYMENT="${API_DEPLOYMENT:-carelink-api}"
DB_DEPLOYMENT="${DB_DEPLOYMENT:-carelink-db}"
API_SERVICE="${API_SERVICE:-carelink-api}"
DB_SERVICE="${DB_SERVICE:-carelink-db}"
DB_SECRET="${DB_SECRET:-carelink-db-secret}"
DB_NAME="${DB_NAME:-CareLink}"
MSSQL_SA_PASSWORD="${MSSQL_SA_PASSWORD:-YourStrong!Passw0rd}"
API_LOCAL_PORT="${API_LOCAL_PORT:-8080}"
DB_LOCAL_PORT="${DB_LOCAL_PORT:-1433}"
API_REPLICAS="${API_REPLICAS:-2}"
DB_REPLICAS="${DB_REPLICAS:-1}"
API_AUTOSCALING_ENABLED="${API_AUTOSCALING_ENABLED:-false}"
API_HPA_MIN_REPLICAS="${API_HPA_MIN_REPLICAS:-1}"
API_HPA_MAX_REPLICAS="${API_HPA_MAX_REPLICAS:-10}"
API_HPA_TARGET_CPU_PERCENT="${API_HPA_TARGET_CPU_PERCENT:-70}"
DOCKER_IMAGE="${DOCKER_IMAGE:-carelink-api:local}"
DOCKER_REGISTRY="${DOCKER_REGISTRY:-}"
KIND_CLUSTER_NAME="${KIND_CLUSTER_NAME:-carelink-local}"
KIND_NODE_IMAGE="${KIND_NODE_IMAGE:-kindest/node:v1.30.0}"
ACTIVE_IMAGE="$DOCKER_IMAGE"
SKIP_CLEAR_ONCE="false"

if [[ -n "$DOCKER_REGISTRY" ]]; then
  ACTIVE_IMAGE="$DOCKER_REGISTRY/$DOCKER_IMAGE"
fi

is_windows() {
  local os
  os="$(uname -s | tr '[:upper:]' '[:lower:]')"
  [[ "$os" == *"mingw"* || "$os" == *"msys"* || "$os" == *"cygwin"* ]]
}

print_line() {
  printf '%*s\n' 70 '' | tr ' ' '-'
}

pause() {
  read -rp "Press Enter to continue..." _
}

require_cmd() {
  local cmd="$1"
  command -v "$cmd" >/dev/null 2>&1
}

kubectl_path() {
  if require_cmd kubectl; then
    command -v kubectl
    return
  fi

  if is_windows; then
    if [[ -x "$BIN_DIR/kubectl.exe" ]]; then
      echo "$BIN_DIR/kubectl.exe"
      return
    fi
  else
    if [[ -x "$BIN_DIR/kubectl" ]]; then
      echo "$BIN_DIR/kubectl"
      return
    fi
  fi

  echo ""
}

docker_path() {
  if require_cmd docker; then
    command -v docker
    return
  fi

  if is_windows; then
    if [[ -x "$BIN_DIR/docker.exe" ]]; then
      echo "$BIN_DIR/docker.exe"
      return
    fi
  else
    if [[ -x "$BIN_DIR/docker" ]]; then
      echo "$BIN_DIR/docker"
      return
    fi
  fi

  echo ""
}

kind_path() {
  if require_cmd kind; then
    command -v kind
    return
  fi

  if is_windows; then
    if [[ -x "$BIN_DIR/kind.exe" ]]; then
      echo "$BIN_DIR/kind.exe"
      return
    fi
  else
    if [[ -x "$BIN_DIR/kind" ]]; then
      echo "$BIN_DIR/kind"
      return
    fi
  fi

  echo ""
}

mkdir -p "$BIN_DIR" "$BACKUPS_DIR" "$RENDERED_DIR" "$PORT_FORWARD_DIR"

KUBECTL_BIN="$(kubectl_path)"
DOCKER_BIN="$(docker_path)"
KIND_BIN="$(kind_path)"

k() {
  MSYS_NO_PATHCONV=1 MSYS2_ARG_CONV_EXCL='*' "$KUBECTL_BIN" "$@"
}

d() {
  "$DOCKER_BIN" "$@"
}

kindc() {
  "$KIND_BIN" "$@"
}

download() {
  local url="$1"
  local out="$2"
  if require_cmd curl; then
    curl -fsSL "$url" -o "$out"
    return
  fi

  if require_cmd wget; then
    wget -q "$url" -O "$out"
    return
  fi

  echo "Neither curl nor wget is available."
  return 1
}

install_kubectl_local() {
  if [[ -n "$(kubectl_path)" ]]; then
    echo "kubectl already available."
    return 0
  fi

  mkdir -p "$BIN_DIR"

  if is_windows; then
    local target="$BIN_DIR/kubectl.exe"
    download "https://dl.k8s.io/release/v1.30.2/bin/windows/amd64/kubectl.exe" "$target"
    chmod +x "$target" || true
  else
    local target="$BIN_DIR/kubectl"
    download "https://dl.k8s.io/release/v1.30.2/bin/linux/amd64/kubectl" "$target"
    chmod +x "$target"
  fi

  KUBECTL_BIN="$(kubectl_path)"
  [[ -n "$KUBECTL_BIN" ]]
}

extract_zip() {
  local archive="$1"
  local output="$2"

  if require_cmd unzip; then
    unzip -q "$archive" -d "$output"
    return 0
  fi

  if require_cmd powershell.exe; then
    powershell.exe -NoProfile -Command "Expand-Archive -Path '$archive' -DestinationPath '$output' -Force" >/dev/null
    return 0
  fi

  echo "No zip extraction tool found (unzip or powershell.exe)."
  return 1
}

install_docker_local() {
  if [[ -n "$(docker_path)" ]]; then
    echo "docker already available."
    return 0
  fi

  mkdir -p "$BIN_DIR"

  if is_windows; then
    local archive="$ARTIFACTS_DIR/docker-cli.zip"
    local unpack="$ARTIFACTS_DIR/docker-unpack"
    rm -rf "$unpack"
    mkdir -p "$unpack"
    download "https://download.docker.com/win/static/stable/x86_64/docker-26.1.4.zip" "$archive"
    extract_zip "$archive" "$unpack"
    if [[ -f "$unpack/docker/docker.exe" ]]; then
      cp "$unpack/docker/docker.exe" "$BIN_DIR/docker.exe"
      chmod +x "$BIN_DIR/docker.exe" || true
    fi
  else
    local archive="$ARTIFACTS_DIR/docker-cli.tgz"
    local unpack="$ARTIFACTS_DIR/docker-unpack"
    rm -rf "$unpack"
    mkdir -p "$unpack"
    download "https://download.docker.com/linux/static/stable/x86_64/docker-26.1.4.tgz" "$archive"
    tar -xzf "$archive" -C "$unpack"
    if [[ -f "$unpack/docker/docker" ]]; then
      cp "$unpack/docker/docker" "$BIN_DIR/docker"
      chmod +x "$BIN_DIR/docker"
    fi
  fi

  DOCKER_BIN="$(docker_path)"
  [[ -n "$DOCKER_BIN" ]]
}

install_kind_local() {
  if [[ -n "$(kind_path)" ]]; then
    echo "kind already available."
    return 0
  fi

  mkdir -p "$BIN_DIR"

  if is_windows; then
    local target="$BIN_DIR/kind.exe"
    download "https://kind.sigs.k8s.io/dl/v0.23.0/kind-windows-amd64" "$target"
    chmod +x "$target" || true
  else
    local target="$BIN_DIR/kind"
    download "https://kind.sigs.k8s.io/dl/v0.23.0/kind-linux-amd64" "$target"
    chmod +x "$target"
  fi

  KIND_BIN="$(kind_path)"
  [[ -n "$KIND_BIN" ]]
}

ensure_kind_cluster() {
  if ! d version >/dev/null 2>&1; then
    echo "Docker engine is required by kind but is not reachable."
    echo "Start Docker Desktop or your Docker daemon first."
    return 1
  fi

  if ! kindc get clusters | grep -Fxq "$KIND_CLUSTER_NAME"; then
    echo "Creating kind cluster: $KIND_CLUSTER_NAME"
    kindc create cluster --name "$KIND_CLUSTER_NAME" --image "$KIND_NODE_IMAGE"
  else
    echo "kind cluster already exists: $KIND_CLUSTER_NAME"
  fi

  k config use-context "kind-$KIND_CLUSTER_NAME" >/dev/null 2>&1 || true
  return 0
}

check_or_install_prerequisites() {
  echo "Checking local prerequisites..."

  if [[ -z "$(kubectl_path)" ]]; then
    echo "kubectl not found. Installing into $BIN_DIR"
    if ! install_kubectl_local; then
      echo "kubectl install failed."
      return 1
    fi
  fi

  if [[ -z "$(docker_path)" ]]; then
    echo "docker CLI not found. Installing into $BIN_DIR"
    if ! install_docker_local; then
      echo "docker CLI install failed."
      return 1
    fi
  fi

  if [[ -z "$(kind_path)" ]]; then
    echo "kind not found. Installing into $BIN_DIR"
    if ! install_kind_local; then
      echo "kind install failed."
      return 1
    fi
  fi

  KUBECTL_BIN="$(kubectl_path)"
  DOCKER_BIN="$(docker_path)"
  KIND_BIN="$(kind_path)"

  echo "kubectl: $KUBECTL_BIN"
  echo "docker:  $DOCKER_BIN"
  echo "kind:    $KIND_BIN"

  if ! k version --client >/dev/null 2>&1; then
    echo "kubectl is present but cannot run correctly."
    return 1
  fi

  if ! d version >/dev/null 2>&1; then
    echo "docker CLI is present but docker engine is not reachable."
    echo "Start Docker Desktop or your Docker daemon before deploying."
    return 1
  fi

  if ! kindc version >/dev/null 2>&1; then
    echo "kind is present but cannot run correctly."
    return 1
  fi

  if ! ensure_kind_cluster; then
    return 1
  fi

  if ! k cluster-info >/dev/null 2>&1; then
    echo "kubectl cannot reach kind cluster $KIND_CLUSTER_NAME."
    return 1
  fi

  return 0
}

load_image_to_kind_if_needed() {
  if [[ -n "$DOCKER_REGISTRY" ]]; then
    return 0
  fi

  if ! d image inspect "$ACTIVE_IMAGE" >/dev/null 2>&1; then
    echo "Local image not found: $ACTIVE_IMAGE"
    echo "Run image build first (menu option 2)."
    return 1
  fi

  echo "Loading image into kind cluster: $ACTIVE_IMAGE"
  kindc load docker-image "$ACTIVE_IMAGE" --name "$KIND_CLUSTER_NAME"
}

render_manifest() {
  local input="$1"
  local output="$2"

  sed \
    -e "s|__NAMESPACE__|$NAMESPACE|g" \
    -e "s|__API_DEPLOYMENT__|$API_DEPLOYMENT|g" \
    -e "s|__DB_DEPLOYMENT__|$DB_DEPLOYMENT|g" \
    -e "s|__API_SERVICE__|$API_SERVICE|g" \
    -e "s|__DB_SERVICE__|$DB_SERVICE|g" \
    -e "s|__DB_SECRET__|$DB_SECRET|g" \
    -e "s|__DB_NAME__|$DB_NAME|g" \
    -e "s|__MSSQL_SA_PASSWORD__|$MSSQL_SA_PASSWORD|g" \
    -e "s|__API_REPLICAS__|$API_REPLICAS|g" \
    -e "s|__DB_REPLICAS__|$DB_REPLICAS|g" \
    -e "s|__API_IMAGE__|$ACTIVE_IMAGE|g" \
    "$input" > "$output"
}

render_all_manifests() {
  rm -rf "$RENDERED_DIR"
  mkdir -p "$RENDERED_DIR"

  for file in "$K8S_DIR"/*.yaml; do
    local name
    name="$(basename "$file")"
    render_manifest "$file" "$RENDERED_DIR/$name"
  done
}

build_image() {
  if [[ -z "$(docker_path)" ]]; then
    echo "docker CLI is not available."
    return 1
  fi

  local full_image="$DOCKER_IMAGE"
  if [[ -n "$DOCKER_REGISTRY" ]]; then
    full_image="$DOCKER_REGISTRY/$DOCKER_IMAGE"
  fi

  echo "Building image: $full_image"
  d build -f "$PROJECT_ROOT/Dockerfile" -t "$full_image" "$PROJECT_ROOT"

  if [[ -n "$DOCKER_REGISTRY" ]]; then
    read -rp "Push image to registry now? (y/n): " answer
    if [[ "${answer,,}" == "y" ]]; then
      d push "$full_image"
    fi
  fi

  ACTIVE_IMAGE="$full_image"

  if [[ -z "$DOCKER_REGISTRY" && -n "$(kind_path)" ]]; then
    if kindc get clusters | grep -Fxq "$KIND_CLUSTER_NAME"; then
      load_image_to_kind_if_needed || true
    fi
  fi
}

deploy_stack() {
  check_or_install_prerequisites
  load_image_to_kind_if_needed
  render_all_manifests

  k apply -f "$RENDERED_DIR/namespace.yaml"

  k apply -f "$RENDERED_DIR/db-secret.yaml"
  k apply -f "$RENDERED_DIR/db-pvc.yaml"
  k apply -f "$RENDERED_DIR/db-service.yaml"
  k apply -f "$RENDERED_DIR/db-deployment.yaml"
  wait_for_db_rollout

  k apply -f "$RENDERED_DIR/api-service.yaml"
  k apply -f "$RENDERED_DIR/api-deployment.yaml"
  k rollout status deployment "$API_DEPLOYMENT" -n "$NAMESPACE" --timeout=300s

  if [[ "${API_AUTOSCALING_ENABLED,,}" == "true" ]]; then
    enable_api_autoscaling
  fi

  stop_all_port_forwards_background
  start_all_port_forwards_background

  echo "Deployment submitted."
  k get pods -n "$NAMESPACE"
  k get svc -n "$NAMESPACE"
}

wait_for_db_rollout() {
  local timeout_seconds=600
  local interval_seconds=10
  local elapsed=0

  echo "Waiting for DB rollout (first SQL Server image pull can take several minutes)..."

  while (( elapsed < timeout_seconds )); do
    local ready
    ready="$(k get deployment "$DB_DEPLOYMENT" -n "$NAMESPACE" -o jsonpath='{.status.readyReplicas}' 2>/dev/null || true)"
    if [[ "${ready:-0}" =~ ^[1-9][0-9]*$ ]]; then
      echo "DB deployment is ready."
      return 0
    fi

    local pod phase reason
    pod="$(k get pods -n "$NAMESPACE" -l "app=$DB_DEPLOYMENT" -o jsonpath='{.items[0].metadata.name}' 2>/dev/null || true)"
    phase=""
    reason=""

    if [[ -n "$pod" ]]; then
      phase="$(k get pod "$pod" -n "$NAMESPACE" -o jsonpath='{.status.phase}' 2>/dev/null || true)"
      reason="$(k get pod "$pod" -n "$NAMESPACE" -o jsonpath='{.status.containerStatuses[0].state.waiting.reason}' 2>/dev/null || true)"
      if [[ -z "$reason" ]]; then
        reason="$(k get pod "$pod" -n "$NAMESPACE" -o jsonpath='{.status.containerStatuses[0].state.terminated.reason}' 2>/dev/null || true)"
      fi
    fi

    echo "DB status: pod=${pod:-n/a}, phase=${phase:-n/a}, reason=${reason:-n/a}, waited=${elapsed}s/${timeout_seconds}s"
    sleep "$interval_seconds"
    elapsed=$((elapsed + interval_seconds))
  done

  echo "DB rollout timed out after ${timeout_seconds}s. Recent diagnostics:"
  k get pods -n "$NAMESPACE" -l "app=$DB_DEPLOYMENT" -o wide || true
  k describe pod -n "$NAMESPACE" -l "app=$DB_DEPLOYMENT" || true
  return 1
}

show_status() {
  print_line
  echo "Namespace: $NAMESPACE"
  echo "Kind cluster: $KIND_CLUSTER_NAME"
  echo "API image: $ACTIVE_IMAGE"
  print_line
  k get all -n "$NAMESPACE" || true
  print_line
  k get pvc -n "$NAMESPACE" || true
}

create_kind_cluster() {
  check_or_install_prerequisites
  ensure_kind_cluster
  k cluster-info
}

delete_kind_cluster() {
  if [[ -z "$(kind_path)" ]]; then
    echo "kind is not installed."
    return 1
  fi

  if ! kindc get clusters | grep -Fxq "$KIND_CLUSTER_NAME"; then
    echo "kind cluster does not exist: $KIND_CLUSTER_NAME"
    return 0
  fi

  read -rp "Delete kind cluster $KIND_CLUSTER_NAME? (y/n): " answer
  if [[ "${answer,,}" != "y" ]]; then
    return
  fi
  kindc delete cluster --name "$KIND_CLUSTER_NAME"
}

watch_pods() {
  if require_cmd watch; then
    watch -n 2 "$KUBECTL_BIN get pods -n $NAMESPACE -o wide"
    return
  fi

  while true; do
    clear
    k get pods -n "$NAMESPACE" -o wide
    echo "Press Ctrl+C to stop watching."
    sleep 2
  done
}

metrics_api_available() {
  k get --raw /apis/metrics.k8s.io/v1beta1 >/dev/null 2>&1
}

enable_api_autoscaling() {
  if ! metrics_api_available; then
    echo "Metrics API not available in this cluster. Install metrics-server first."
    return 1
  fi

  if ! [[ "$API_HPA_MIN_REPLICAS" =~ ^[0-9]+$ && "$API_HPA_MAX_REPLICAS" =~ ^[0-9]+$ && "$API_HPA_TARGET_CPU_PERCENT" =~ ^[0-9]+$ ]]; then
    echo "HPA settings must be numeric."
    return 1
  fi

  if (( API_HPA_MIN_REPLICAS < 1 || API_HPA_MAX_REPLICAS < API_HPA_MIN_REPLICAS )); then
    echo "Invalid HPA range: min=$API_HPA_MIN_REPLICAS max=$API_HPA_MAX_REPLICAS"
    return 1
  fi

  echo "Enabling autoscaling for $API_DEPLOYMENT (min=$API_HPA_MIN_REPLICAS, max=$API_HPA_MAX_REPLICAS, cpu=$API_HPA_TARGET_CPU_PERCENT%)"
  k -n "$NAMESPACE" autoscale deployment "$API_DEPLOYMENT" \
    --cpu-percent="$API_HPA_TARGET_CPU_PERCENT" \
    --min="$API_HPA_MIN_REPLICAS" \
    --max="$API_HPA_MAX_REPLICAS"

  k -n "$NAMESPACE" get hpa "$API_DEPLOYMENT" || true
}

disable_api_autoscaling() {
  if k -n "$NAMESPACE" get hpa "$API_DEPLOYMENT" >/dev/null 2>&1; then
    k -n "$NAMESPACE" delete hpa "$API_DEPLOYMENT"
    echo "Autoscaling disabled for $API_DEPLOYMENT."
  else
    echo "No HPA found for $API_DEPLOYMENT."
  fi
}

show_api_autoscaling_status() {
  if ! metrics_api_available; then
    echo "Metrics API not available in this cluster."
    return 0
  fi

  k -n "$NAMESPACE" get hpa "$API_DEPLOYMENT" -o wide 2>/dev/null || echo "No HPA found for $API_DEPLOYMENT."
}

scale_deployment() {
  local deployment="$1"
  read -rp "Enter desired replica count for '$deployment': " replicas
  if ! [[ "$replicas" =~ ^[0-9]+$ ]]; then
    echo "Invalid number: $replicas"
    return 1
  fi
  kubectl -n "$NAMESPACE" scale deployment "$deployment" --replicas="$replicas"
  echo "Scaling '$deployment' to $replicas replica(s)..."
  kubectl -n "$NAMESPACE" rollout status deployment/"$deployment" --timeout=120s
}

watch_stats() {
  if ! metrics_api_available; then
    echo "Metrics API not available in this cluster."
    echo "Install metrics-server to use live resource stats, then try again."
    return 0
  fi

  if require_cmd watch; then
    watch -n 2 "$KUBECTL_BIN top pods -n $NAMESPACE"
    return
  fi

  while true; do
    clear
    k top pods -n "$NAMESPACE" || true
    echo "Press Ctrl+C to stop watching."
    sleep 2
  done
}

port_forward_pid_file() {
  local name="$1"
  echo "$PORT_FORWARD_DIR/$name.pid"
}

port_forward_log_file() {
  local name="$1"
  echo "$PORT_FORWARD_DIR/$name.log"
}

port_forward_is_running() {
  local name="$1"
  local pid_file
  pid_file="$(port_forward_pid_file "$name")"

  if [[ ! -f "$pid_file" ]]; then
    return 1
  fi

  local pid
  pid="$(cat "$pid_file")"
  if [[ -z "$pid" ]]; then
    return 1
  fi

  kill -0 "$pid" >/dev/null 2>&1
}

start_port_forward_background() {
  local name="$1"
  local service="$2"
  local mapping="$3"
  local local_port="${mapping%%:*}"

  local pid_file log_file
  pid_file="$(port_forward_pid_file "$name")"
  log_file="$(port_forward_log_file "$name")"

  if port_forward_is_running "$name"; then
    echo "$name port-forward already running (pid $(cat "$pid_file"))."
    return 0
  fi

  rm -f "$pid_file"
  : > "$log_file"

  nohup bash -c "MSYS_NO_PATHCONV=1 MSYS2_ARG_CONV_EXCL='*' '$KUBECTL_BIN' -n '$NAMESPACE' port-forward 'svc/$service' '$mapping'" >"$log_file" 2>&1 &
  local pid=$!
  echo "$pid" > "$pid_file"

  sleep 1
  if kill -0 "$pid" >/dev/null 2>&1; then
    echo "Started $name port-forward: $mapping (pid $pid)"
  else
    if grep -qi "unable to listen on any of the requested ports" "$log_file"; then
      if [[ "$name" == "api" ]] && curl -fsS "http://localhost:$local_port/swagger/index.html" >/dev/null 2>&1; then
        echo "API already reachable on localhost:$local_port, keeping existing forward."
        rm -f "$pid_file"
        return 0
      fi

      echo "$name local port $local_port is already in use."
      echo "If this is your existing forward, you can ignore this."
      rm -f "$pid_file"
      return 0
    fi

    echo "Failed to start $name port-forward. Check log: $log_file"
    rm -f "$pid_file"
    return 1
  fi
}

stop_port_forward_background() {
  local name="$1"
  local pid_file
  pid_file="$(port_forward_pid_file "$name")"

  if ! port_forward_is_running "$name"; then
    rm -f "$pid_file"
    echo "$name port-forward is not running."
    return 0
  fi

  local pid
  pid="$(cat "$pid_file")"
  kill "$pid" >/dev/null 2>&1 || true
  rm -f "$pid_file"
  echo "Stopped $name port-forward (pid $pid)."
}

start_all_port_forwards_background() {
  check_or_install_prerequisites
  start_port_forward_background "api" "$API_SERVICE" "$API_LOCAL_PORT:80"
  start_port_forward_background "db" "$DB_SERVICE" "$DB_LOCAL_PORT:1433"
  echo "Logs:"
  echo "  API -> $(port_forward_log_file "api")"
  echo "  DB  -> $(port_forward_log_file "db")"
}

stop_all_port_forwards_background() {
  stop_port_forward_background "api"
  stop_port_forward_background "db"
}

show_port_forwards_status() {
  local name pid_file log_file
  for name in api db; do
    pid_file="$(port_forward_pid_file "$name")"
    log_file="$(port_forward_log_file "$name")"
    if port_forward_is_running "$name"; then
      echo "$name: running (pid $(cat "$pid_file"))"
    else
      echo "$name: stopped"
    fi
    if [[ -f "$log_file" ]]; then
      echo "  log: $log_file"
    fi
  done
}
find_db_pod() {
  k get pods -n "$NAMESPACE" -l "app=$DB_DEPLOYMENT" -o jsonpath='{.items[0].metadata.name}'
}

ensure_database_exists() {
  local pod
  pod="$(find_db_pod)"
  if [[ -z "$pod" ]]; then
    echo "Database pod not found."
    return 1
  fi

  k exec -n "$NAMESPACE" "$pod" -- /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C \
    -Q "IF DB_ID(N'$DB_NAME') IS NULL CREATE DATABASE [$DB_NAME];"
}

backup_database() {
  local pod
  pod="$(find_db_pod)"
  if [[ -z "$pod" ]]; then
    echo "Database pod not found."
    return 1
  fi

  local backup_file
  backup_file="${DB_NAME}-$(date +%Y%m%d-%H%M%S).bak"

  ensure_database_exists
  k exec -n "$NAMESPACE" "$pod" -- mkdir -p /var/opt/mssql/backups
  k exec -n "$NAMESPACE" "$pod" -- /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C \
    -Q "BACKUP DATABASE [$DB_NAME] TO DISK = N'/var/opt/mssql/backups/$backup_file' WITH FORMAT, INIT"

  k exec -n "$NAMESPACE" "$pod" -- sh -c "cat /var/opt/mssql/backups/$backup_file" > "$BACKUPS_DIR/$backup_file"
  echo "Backup saved: $BACKUPS_DIR/$backup_file"
}

restore_database() {
  local pod
  pod="$(find_db_pod)"
  if [[ -z "$pod" ]]; then
    echo "Database pod not found."
    return 1
  fi

  mapfile -t files < <(ls -1 "$BACKUPS_DIR"/*.bak 2>/dev/null || true)
  if [[ "${#files[@]}" -eq 0 ]]; then
    echo "No local backup files found in $BACKUPS_DIR"
    return 1
  fi

  echo "Available backups:"
  local i=1
  for file in "${files[@]}"; do
    echo "$i) $(basename "$file")"
    ((i++))
  done

  local choice
  read -rp "Choose backup number: " choice
  if [[ ! "$choice" =~ ^[0-9]+$ ]] || (( choice < 1 || choice > ${#files[@]} )); then
    echo "Invalid selection."
    return 1
  fi

  local selected
  selected="${files[$((choice-1))]}"
  local selected_name
  selected_name="$(basename "$selected")"

  ensure_database_exists
  k exec -n "$NAMESPACE" "$pod" -- mkdir -p /var/opt/mssql/backups
  cat "$selected" | k exec -i -n "$NAMESPACE" "$pod" -- sh -c "cat > /var/opt/mssql/backups/$selected_name"

  k exec -n "$NAMESPACE" "$pod" -- /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C \
    -Q "ALTER DATABASE [$DB_NAME] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; RESTORE DATABASE [$DB_NAME] FROM DISK = N'/var/opt/mssql/backups/$selected_name' WITH REPLACE; ALTER DATABASE [$DB_NAME] SET MULTI_USER;"

  echo "Restore completed from $selected_name"
}

db_backup_menu() {
  while true; do
    print_line
    echo "Database backup menu"
    print_line
    echo "1) Create backup"
    echo "2) Restore from backup"
    echo "3) List local backups"
    echo "0) Back"
    read -rp "Choose option: " option

    case "$option" in
      1)
        backup_database
        pause
        ;;
      2)
        restore_database
        pause
        ;;
      3)
        ls -lh "$BACKUPS_DIR"/*.bak 2>/dev/null || echo "No backups yet."
        pause
        ;;
      0)
        return
        ;;
      *)
        echo "Invalid option"
        ;;
    esac
  done
}

restart_stack() {
  k rollout restart deployment "$DB_DEPLOYMENT" -n "$NAMESPACE"
  k rollout status deployment "$DB_DEPLOYMENT" -n "$NAMESPACE" --timeout=300s
  k rollout restart deployment "$API_DEPLOYMENT" -n "$NAMESPACE"
  k rollout status deployment "$API_DEPLOYMENT" -n "$NAMESPACE" --timeout=300s

  stop_all_port_forwards_background
  start_all_port_forwards_background
}

destroy_stack() {
  read -rp "Delete namespace $NAMESPACE and all resources? (y/n): " answer
  if [[ "${answer,,}" != "y" ]]; then
    return
  fi
  k delete namespace "$NAMESPACE"
}

show_menu() {
  if [[ "$SKIP_CLEAR_ONCE" == "true" ]]; then
    SKIP_CLEAR_ONCE="false"
  else
    clear
  fi
  print_line
  echo "CareLink Cluster Infrastructure Menu"
  print_line
  echo "Namespace: $NAMESPACE"
  echo "Kind:      $KIND_CLUSTER_NAME"
  echo "Image:     $ACTIVE_IMAGE"
  print_line
  echo "1) Check and install prerequisites (kubectl, docker CLI, kind)"
  echo "2) Create or ensure kind cluster"
  echo "3) Build API Docker image"
  echo "4) Deploy infrastructure and backend"
  echo "5) Show current status"
  echo "6) Watch pods in real time"
  echo "7) Watch pods resource stats"
  echo "8) Scale API deployment"
  echo "9) Scale DB deployment"
  echo "10) Database backup/restore menu"
  echo "11) Restart deployments"
  echo "12) Destroy deployed namespace"
  echo "13) Delete kind cluster"
  echo "14) Enable API autoscaling (HPA)"
  echo "15) Disable API autoscaling (HPA)"
  echo "16) Show API autoscaling status"
  echo "0) Exit"
  print_line
}

main_loop() {
  while true; do
    show_menu
    read -rp "Choose option: " choice

    case "$choice" in
      1)
        check_or_install_prerequisites && echo "Ready."
        pause
        ;;
      2)
        create_kind_cluster
        pause
        ;;
      3)
        build_image
        pause
        ;;
      4)
        deploy_stack
        pause
        ;;
      5)
        show_status
        SKIP_CLEAR_ONCE="true"
        ;;
      6)
        watch_pods
        ;;
      7)
        watch_stats
        ;;
      8)
        scale_deployment "$API_DEPLOYMENT"
        pause
        ;;
      9)
        scale_deployment "$DB_DEPLOYMENT"
        pause
        ;;
      10)
        db_backup_menu
        pause
        ;;
      11)
        restart_stack
        pause
        ;;
      12)
        destroy_stack
        pause
        ;;
      13)
        delete_kind_cluster
        pause
        ;;
      14)
        enable_api_autoscaling
        pause
        ;;
      15)
        disable_api_autoscaling
        pause
        ;;
      16)
        show_api_autoscaling_status
        pause
        ;;
      0)
        exit 0
        ;;
      *)
        echo "Invalid option"
        pause
        ;;
    esac
  done
}

main_loop