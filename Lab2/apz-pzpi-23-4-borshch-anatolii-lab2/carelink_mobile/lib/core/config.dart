class AppConfig {
  // Backend base URL.
  // - Windows/macOS/Linux desktop, web, iOS simulator: use http://localhost:5000
  // - Android emulator: localhost on the device != host machine, use http://10.0.2.2:5000
  // - Physical phone: use the LAN IP of the machine running the API (e.g. http://192.168.x.x:5000)
  static const String apiBaseUrl = 'http://localhost:5000';
}
