import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../core/auth_service.dart';
import '../home/home_screen.dart';
import 'login_screen.dart';

class SplashScreen extends StatefulWidget {
  const SplashScreen({super.key});

  @override
  State<SplashScreen> createState() => _SplashScreenState();
}

class _SplashScreenState extends State<SplashScreen> {
  @override
  void initState() {
    super.initState();
    _bootstrap();
  }

  Future<void> _bootstrap() async {
    final auth = context.read<AuthService>();
    await auth.load();
    await Future.delayed(const Duration(milliseconds: 600));
    if (!mounted) return;
    Navigator.of(context).pushReplacement(
      MaterialPageRoute(
        builder: (_) =>
            auth.isLoggedIn ? const HomeScreen() : const LoginScreen(),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final cs = Theme.of(context).colorScheme;
    return Scaffold(
      backgroundColor: cs.primary,
      body: Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(Icons.favorite_rounded, size: 96, color: cs.onPrimary),
            const SizedBox(height: 16),
            Text(
              'CareLink',
              style: TextStyle(
                fontSize: 36,
                fontWeight: FontWeight.bold,
                color: cs.onPrimary,
              ),
            ),
            const SizedBox(height: 8),
            Text(
              'Care, anywhere.',
              style: TextStyle(
                color: cs.onPrimary.withOpacity(0.85),
                fontSize: 16,
              ),
            ),
            const SizedBox(height: 36),
            CircularProgressIndicator(color: cs.onPrimary),
          ],
        ),
      ),
    );
  }
}
