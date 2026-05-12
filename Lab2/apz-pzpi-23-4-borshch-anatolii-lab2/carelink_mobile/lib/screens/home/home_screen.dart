import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../core/api_client.dart';
import '../../core/auth_service.dart';
import '../auth/login_screen.dart';
import '../chat/chat_list_screen.dart';
import '../cognitive/exercise_list_screen.dart';
import '../iot/iot_readings_screen.dart';
import '../notifications/notifications_list_screen.dart';
import '../profile/view_profile_screen.dart';
import '../relatives/relatives_list_screen.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  String? _displayName;

  @override
  void initState() {
    super.initState();
    _loadUser();
  }

  Future<void> _loadUser() async {
    final id = context.read<AuthService>().userId;
    if (id == null) return;
    try {
      final r = await context.read<ApiClient>().get('/api/v1/users/$id');
      Map? user;
      if (r is Map) {
        user = r['data'] is Map ? r['data'] as Map : r;
      }
      if (user != null && mounted) {
        final name =
            '${user['firstName'] ?? ''} ${user['lastName'] ?? ''}'.trim();
        setState(() => _displayName = name.isEmpty ? null : name);
      }
    } catch (_) {
    }
  }

  Future<void> _logout(BuildContext context) async {
    try {
      await context.read<ApiClient>().post('/api/v1/auth/logout');
    } catch (_) {}
    await context.read<AuthService>().clear();
    if (!context.mounted) return;
    Navigator.of(context).pushAndRemoveUntil(
      MaterialPageRoute(builder: (_) => const LoginScreen()),
      (_) => false,
    );
  }

  @override
  Widget build(BuildContext context) {
    final userId = context.watch<AuthService>().userId;
    final tiles = <_HomeTile>[
      _HomeTile(
        'IoT Monitoring',
        Icons.monitor_heart_outlined,
        Colors.teal,
        () => const IoTReadingsScreen(),
      ),
      _HomeTile(
        'Notifications',
        Icons.notifications_outlined,
        Colors.orange,
        () => const NotificationsListScreen(),
      ),
      _HomeTile(
        'Chat',
        Icons.chat_bubble_outline,
        Colors.indigo,
        () => const ChatListScreen(),
      ),
      _HomeTile(
        'Cognitive',
        Icons.psychology_outlined,
        Colors.purple,
        () => const ExerciseListScreen(),
      ),
      _HomeTile(
        'Profile',
        Icons.person_outline,
        Colors.blueGrey,
        () => const ViewProfileScreen(),
      ),
      _HomeTile(
        'Relatives',
        Icons.people_outline,
        Colors.green,
        () => const RelativesListScreen(),
      ),
    ];

    return Scaffold(
      appBar: AppBar(
        title: const Text('CareLink'),
        actions: [
          IconButton(
            tooltip: 'Logout',
            icon: const Icon(Icons.logout),
            onPressed: () => _logout(context),
          ),
        ],
      ),
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const Text(
                'Control Panel',
                style: TextStyle(fontSize: 22, fontWeight: FontWeight.w700),
              ),
              const SizedBox(height: 4),
              Text(
                _displayName == null
                    ? 'Welcome back, user #${userId ?? '-'}'
                    : 'Welcome back, $_displayName (ID: ${userId ?? '-'})',
                style: const TextStyle(color: Colors.black54),
              ),
              const SizedBox(height: 16),
              Expanded(
                child: GridView.count(
                  crossAxisCount: 2,
                  mainAxisSpacing: 12,
                  crossAxisSpacing: 12,
                  childAspectRatio: 1.05,
                  children: tiles
                      .map(
                        (t) => _TileCard(
                          tile: t,
                          onTap: () => Navigator.of(context).push(
                            MaterialPageRoute(builder: (_) => t.builder()),
                          ),
                        ),
                      )
                      .toList(),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _HomeTile {
  final String title;
  final IconData icon;
  final Color color;
  final Widget Function() builder;
  _HomeTile(this.title, this.icon, this.color, this.builder);
}

class _TileCard extends StatelessWidget {
  const _TileCard({required this.tile, required this.onTap});
  final _HomeTile tile;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return Material(
      color: Colors.white,
      borderRadius: BorderRadius.circular(16),
      elevation: 1,
      child: InkWell(
        borderRadius: BorderRadius.circular(16),
        onTap: onTap,
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              CircleAvatar(
                radius: 26,
                backgroundColor: tile.color.withOpacity(0.12),
                child: Icon(tile.icon, color: tile.color, size: 28),
              ),
              Text(
                tile.title,
                style: const TextStyle(
                  fontSize: 16,
                  fontWeight: FontWeight.w600,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
