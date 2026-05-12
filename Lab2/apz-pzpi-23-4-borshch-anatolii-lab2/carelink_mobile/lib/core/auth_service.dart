import 'package:flutter/foundation.dart';
import 'package:shared_preferences/shared_preferences.dart';

class AuthService extends ChangeNotifier {
  static const _kToken = 'auth_token';
  static const _kUserId = 'user_id';

  String? _token;
  int? _userId;

  String? get token => _token;
  int? get userId => _userId;
  bool get isLoggedIn => _token != null && _token!.isNotEmpty;

  Future<void> load() async {
    final prefs = await SharedPreferences.getInstance();
    _token = prefs.getString(_kToken);
    _userId = prefs.getInt(_kUserId);
    notifyListeners();
  }

  Future<void> save({required String token, required int userId}) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(_kToken, token);
    await prefs.setInt(_kUserId, userId);
    _token = token;
    _userId = userId;
    notifyListeners();
  }

  Future<void> clear() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove(_kToken);
    await prefs.remove(_kUserId);
    _token = null;
    _userId = null;
    notifyListeners();
  }
}
