import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../core/api_client.dart';
import '../../core/auth_service.dart';
import 'iot_history_screen.dart';

class IoTReadingsScreen extends StatefulWidget {
  const IoTReadingsScreen({super.key});

  @override
  State<IoTReadingsScreen> createState() => _IoTReadingsScreenState();
}

class _IoTReadingsScreenState extends State<IoTReadingsScreen> {
  late Future<List<dynamic>> _future;

  @override
  void initState() {
    super.initState();
    _future = _load();
  }

  Future<List<dynamic>> _load() async {
    final api = context.read<ApiClient>();
    final userId = context.read<AuthService>().userId;
    if (userId == null) return const [];
    final r = await api.get(
      '/api/iot-readings/$userId/latest',
      query: {'count': 20},
    );
    if (r is List) return r;
    if (r is Map && r['data'] is List) return r['data'];
    return const [];
  }

  Future<void> _refresh() async {
    setState(() => _future = _load());
    await _future;
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('IoT Monitoring'),
        actions: [
          IconButton(
            tooltip: 'History',
            icon: const Icon(Icons.history),
            onPressed: () => Navigator.of(
              context,
            ).push(MaterialPageRoute(builder: (_) => const IoTHistoryScreen())),
          ),
        ],
      ),
      body: RefreshIndicator(
        onRefresh: _refresh,
        child: FutureBuilder<List<dynamic>>(
          future: _future,
          builder: (context, snap) {
            if (snap.connectionState == ConnectionState.waiting) {
              return const Center(child: CircularProgressIndicator());
            }
            if (snap.hasError) {
              return ListView(
                children: [
                  const SizedBox(height: 80),
                  Center(child: Text('Error: ${snap.error}')),
                ],
              );
            }
            final items = snap.data ?? const [];
            if (items.isEmpty) {
              return ListView(
                children: const [
                  SizedBox(height: 80),
                  Center(child: Text('No readings yet')),
                ],
              );
            }
            return ListView.builder(
              itemCount: items.length,
              itemBuilder: (context, i) {
                final r = items[i] as Map? ?? {};
                final date =
                    r['readDateTime'] ?? r['ReadDateTime'] ?? r['timestamp'];
                return Card(
                  child: ListTile(
                    leading: const CircleAvatar(
                      backgroundColor: Color(0xFFE0F2F1),
                      child: Icon(Icons.favorite, color: Colors.teal),
                    ),
                    title: Text(
                      'Pulse: ${r['pulse'] ?? '-'} bpm',
                      style: const TextStyle(fontWeight: FontWeight.w600),
                    ),
                    subtitle: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          'Activity: ${r['activityLevel'] ?? '-'} · Temp: ${r['temperature'] ?? '-'}',
                        ),
                        const SizedBox(height: 2),
                        Text(
                          date?.toString() ?? '-',
                          style: const TextStyle(fontSize: 12),
                        ),
                      ],
                    ),
                  ),
                );
              },
            );
          },
        ),
      ),
    );
  }
}
