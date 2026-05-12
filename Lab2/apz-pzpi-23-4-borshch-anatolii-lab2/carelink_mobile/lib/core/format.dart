import 'package:intl/intl.dart';

String fmtDateTime(dynamic value) {
  if (value == null) return '-';
  final dt = value is DateTime ? value : DateTime.tryParse(value.toString());
  if (dt == null) return value.toString();
  return DateFormat('yyyy-MM-dd HH:mm').format(dt.toLocal());
}

String fmtDate(dynamic value) {
  if (value == null) return '-';
  final dt = value is DateTime ? value : DateTime.tryParse(value.toString());
  if (dt == null) return value.toString();
  return DateFormat('yyyy-MM-dd').format(dt.toLocal());
}

String fmtTime(dynamic value) {
  if (value == null) return '-';
  final dt = value is DateTime ? value : DateTime.tryParse(value.toString());
  if (dt == null) return value.toString();
  return DateFormat('HH:mm').format(dt.toLocal());
}
