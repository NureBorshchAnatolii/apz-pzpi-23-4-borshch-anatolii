import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../core/api_client.dart';
import '../../core/auth_service.dart';
import '../../core/format.dart';

class ExerciseResultsScreen extends StatefulWidget {
  const ExerciseResultsScreen({super.key});

  @override
  State<ExerciseResultsScreen> createState() => _ExerciseResultsScreenState();
}

class _ExerciseResultsScreenState extends State<ExerciseResultsScreen> {
  late Future<List<dynamic>> _future;

  @override
  void initState() {
    super.initState();
    _future = _load();
  }

  Future<List<dynamic>> _load() async {
    final userId = context.read<AuthService>().userId;
    if (userId == null) return const [];
    final r = await context.read<ApiClient>().get(
      '/api/cognitive-exercise/$userId/result',
    );
    if (r is List) return r;
    if (r is Map && r['data'] is List) return r['data'];
    return const [];
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Results history')),
      body: FutureBuilder<List<dynamic>>(
        future: _future,
        builder: (context, snap) {
          if (snap.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          }
          if (snap.hasError) {
            return Center(child: Text('Error: ${snap.error}'));
          }
          final items = snap.data ?? const [];
          if (items.isEmpty) {
            return const Center(child: Text('No results yet'));
          }
          return ListView.builder(
            itemCount: items.length,
            itemBuilder: (context, i) {
              final r = items[i] as Map? ?? {};
              final score = r['score'] ?? 0;
              return Card(
                child: ListTile(
                  leading: CircleAvatar(
                    backgroundColor: Colors.purple.shade50,
                    child: Text(
                      '$score',
                      style: const TextStyle(
                        color: Colors.purple,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ),
                  title: Text(
                    (r['title'] ??
                            r['exerciseTitle'] ??
                            'Exercise #${r['exerciseId'] ?? '-'}')
                        .toString(),
                  ),
                  subtitle: Text(fmtDateTime(r['completedAt'])),
                ),
              );
            },
          );
        },
      ),
    );
  }
}
