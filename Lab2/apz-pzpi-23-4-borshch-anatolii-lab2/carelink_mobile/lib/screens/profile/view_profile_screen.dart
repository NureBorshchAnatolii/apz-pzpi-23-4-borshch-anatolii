import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../core/api_client.dart';
import '../../core/auth_service.dart';
import '../../core/format.dart';
import 'edit_profile_screen.dart';

class ViewProfileScreen extends StatefulWidget {
  const ViewProfileScreen({super.key});

  @override
  State<ViewProfileScreen> createState() => _ViewProfileScreenState();
}

class _ViewProfileScreenState extends State<ViewProfileScreen> {
  Future<Map?>? _future;

  @override
  void initState() {
    super.initState();
    _future = _load();
  }

  Future<Map?> _load() async {
    final id = context.read<AuthService>().userId;
    if (id == null) return null;
    final api = context.read<ApiClient>();
    final userRes = await api.get('/api/v1/users/$id');
    Map? user;
    if (userRes is Map) {
      user = userRes['data'] is Map ? userRes['data'] as Map : userRes;
    }
    if (user == null) return null;

    final hasRoleName =
        (user['role'] ?? user['roleName']) != null &&
        (user['role'] ?? user['roleName']).toString().isNotEmpty;
    if (!hasRoleName) {
      final roleId = (user['roleId'] ?? user['RoleId']) as num?;
      if (roleId != null) {
        try {
          final r = await api.get('/api/roles');
          List list = const [];
          if (r is List) {
            list = r;
          } else if (r is Map && r['data'] is List) {
            list = r['data'];
          }
          for (final item in list) {
            if (item is Map) {
              final rid = (item['id'] ?? item['roleId']) as num?;
              if (rid != null && rid.toInt() == roleId.toInt()) {
                user['role'] =
                    (item['name'] ?? item['title'] ?? item['roleName'])
                        ?.toString();
                break;
              }
            } else if (item is String && list.length > roleId.toInt() - 1) {
              user['role'] = item;
            }
          }
        } catch (_) {
        }
      }
      if ((user['role'] ?? '').toString().isEmpty && user['roleId'] != null) {
        user['role'] = 'Role #${user['roleId']}';
      }
    }
    return user;
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Profile'),
        actions: [
          IconButton(
            tooltip: 'Edit',
            icon: const Icon(Icons.edit_outlined),
            onPressed: () async {
              final user = await _future;
              if (!mounted) return;
              await Navigator.of(context).push(
                MaterialPageRoute(
                  builder: (_) => EditProfileScreen(initial: user ?? const {}),
                ),
              );
              setState(() => _future = _load());
            },
          ),
        ],
      ),
      body: FutureBuilder<Map?>(
        future: _future,
        builder: (context, snap) {
          if (snap.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          }
          if (snap.hasError) {
            return Center(child: Text('Error: ${snap.error}'));
          }
          final u = snap.data ?? const {};
          final name = '${u['firstName'] ?? ''} ${u['lastName'] ?? ''}'.trim();
          return ListView(
            padding: const EdgeInsets.all(16),
            children: [
              Center(
                child: Column(
                  children: [
                    CircleAvatar(
                      radius: 44,
                      backgroundColor: Theme.of(
                        context,
                      ).colorScheme.primaryContainer,
                      child: Text(
                        name.isNotEmpty ? name[0].toUpperCase() : '?',
                        style: const TextStyle(
                          fontSize: 32,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                    ),
                    const SizedBox(height: 12),
                    Text(
                      name.isEmpty ? 'CareLink user' : name,
                      style: const TextStyle(
                        fontSize: 22,
                        fontWeight: FontWeight.w700,
                      ),
                    ),
                    Text(
                      (u['email'] ?? '').toString(),
                      style: const TextStyle(color: Colors.black54),
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 24),
              _row(Icons.phone_outlined, 'Phone', u['phoneNumber']),
              _row(Icons.home_outlined, 'Address', u['address']),
              _row(
                Icons.cake_outlined,
                'Birth date',
                fmtDate(u['dateOfBirth'] ?? u['birthDate']),
              ),
              _row(Icons.shield_outlined, 'Role', u['role'] ?? u['roleName']),
            ],
          );
        },
      ),
    );
  }

  Widget _row(IconData icon, String label, dynamic value) {
    return Card(
      child: ListTile(
        leading: Icon(icon),
        title: Text(label),
        subtitle: Text(
          (value == null || value.toString().isEmpty) ? '-' : value.toString(),
        ),
      ),
    );
  }
}
