import 'dart:async';
import 'dart:convert';

import 'package:http/http.dart' as http;

import 'auth_service.dart';
import 'config.dart';

class ApiException implements Exception {
  final int statusCode;
  final String message;
  ApiException(this.statusCode, this.message);
  @override
  String toString() => 'ApiException($statusCode): $message';
}

class ApiClient {
  ApiClient(this._auth);
  final AuthService _auth;

  Uri _uri(String path, [Map<String, dynamic>? query]) {
    final base = Uri.parse(AppConfig.apiBaseUrl);
    final qp = <String, String>{};
    query?.forEach((k, v) {
      if (v != null) qp[k] = v.toString();
    });
    return base.replace(
      path: '${base.path}$path',
      queryParameters: qp.isEmpty ? null : qp,
    );
  }

  Map<String, String> _headers({bool json = true}) {
    final h = <String, String>{'Accept': 'application/json'};
    if (json) h['Content-Type'] = 'application/json';
    final t = _auth.token;
    if (t != null && t.isNotEmpty) h['Authorization'] = 'Bearer $t';
    return h;
  }

  Future<dynamic> get(String path, {Map<String, dynamic>? query}) async {
    final r = await http.get(_uri(path, query), headers: _headers(json: false));
    return _decode(r);
  }

  Future<List<int>> getBytes(
    String path, {
    Map<String, dynamic>? query,
  }) async {
    final r = await http.get(_uri(path, query), headers: _headers(json: false));
    if (r.statusCode >= 200 && r.statusCode < 300) {
      return r.bodyBytes;
    }
    String message = 'Request failed';
    try {
      final decoded = jsonDecode(r.body);
      if (decoded is Map && decoded['message'] != null) {
        message = decoded['message'].toString();
      }
    } catch (_) {
      if (r.body.isNotEmpty) message = r.body;
    }
    throw ApiException(r.statusCode, message);
  }

  Future<dynamic> post(String path, {Object? body}) async {
    final r = await http.post(
      _uri(path),
      headers: _headers(),
      body: body == null ? null : jsonEncode(body),
    );
    return _decode(r);
  }

  Future<dynamic> put(String path, {Object? body}) async {
    final r = await http.put(
      _uri(path),
      headers: _headers(),
      body: body == null ? null : jsonEncode(body),
    );
    return _decode(r);
  }

  Future<dynamic> delete(String path, {Object? body}) async {
    final r = await http.delete(
      _uri(path),
      headers: _headers(),
      body: body == null ? null : jsonEncode(body),
    );
    return _decode(r);
  }

  dynamic _decode(http.Response r) {
    final code = r.statusCode;
    final body = r.body;
    if (code >= 200 && code < 300) {
      if (body.isEmpty) return null;
      try {
        return jsonDecode(body);
      } catch (_) {
        return body;
      }
    }
    String message = 'Request failed';
    try {
      final decoded = jsonDecode(body);
      if (decoded is Map && decoded['message'] != null) {
        message = decoded['message'].toString();
      } else {
        message = body;
      }
    } catch (_) {
      if (body.isNotEmpty) message = body;
    }
    throw ApiException(code, message);
  }
}
