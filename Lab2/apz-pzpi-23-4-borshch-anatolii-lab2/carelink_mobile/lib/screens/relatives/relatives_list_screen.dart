import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../core/api_client.dart';
import 'add_relative_screen.dart';
import 'relative_report_screen.dart';

class RelativesListScreen extends StatefulWidget {
  const RelativesListScreen({super.key});

  @override
  State<RelativesListScreen> createState() => _RelativesListScreenState();
}

class _RelativesListScreenState extends State<RelativesListScreen> {
  late Future<List<dynamic>> _future;

  @override
  void initState() {
    super.initState();
    _future = _load();
  }

  Future<List<dynamic>> _load() async {
    final r = await context.read<ApiClient>().get('/api/relatives');
    if (r is List) return r;
    if (r is Map && r['data'] is List) return r['data'];
    return const [];
  }

  Future<void> _refresh() async {
    setState(() => _future = _load());
    await _future;
  }

  Future<void> _delete(int id) async {
    final ok = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Remove relative?'),
        content: const Text('This action cannot be undone.'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx, false),
            child: const Text('Cancel'),
          ),
          TextButton(
            onPressed: () => Navigator.pop(ctx, true),
            child: const Text('Remove', style: TextStyle(color: Colors.red)),
          ),
        ],
      ),
    );
    if (ok != true) return;
    try {
      await context.read<ApiClient>().delete('/api/relatives/$id');
      _refresh();
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(
        context,
      ).showSnackBar(SnackBar(content: Text(e.toString())));
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Relatives')),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () async {
          await Navigator.of(
            context,
          ).push(MaterialPageRoute(builder: (_) => const AddRelativeScreen()));
          _refresh();
        },
        icon: const Icon(Icons.person_add_alt),
        label: const Text('Add'),
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
                  Center(child: Text('No relatives yet')),
                ],
              );
            }
            return ListView.builder(
              itemCount: items.length,
              itemBuilder: (context, i) {
                final r = items[i] as Map? ?? {};
                final id = (r['relativeId'] ?? r['id'] ?? 0) as num;
                final name =
                    (r['firstName'] != null
                            ? '${r['firstName']} ${r['lastName'] ?? ''}'
                            : (r['name'] ?? 'User #$id'))
                        .toString()
                        .trim();
                return Card(
                  child: ListTile(
                    leading: CircleAvatar(
                      child: Text(
                        name.isNotEmpty ? name[0].toUpperCase() : '?',
                      ),
                    ),
                    title: Text(name),
                    subtitle: Text(
                      (r['relationType'] ?? r['relation'] ?? '').toString(),
                    ),
                    trailing: PopupMenuButton<String>(
                      onSelected: (v) {
                        if (v == 'report') {
                          Navigator.of(context).push(
                            MaterialPageRoute(
                              builder: (_) => RelativeReportScreen(
                                relativeId: id.toInt(),
                                relativeName: name,
                              ),
                            ),
                          );
                        } else if (v == 'delete') {
                          _delete(id.toInt());
                        }
                      },
                      itemBuilder: (_) => const [
                        PopupMenuItem(
                          value: 'report',
                          child: Text('View report'),
                        ),
                        PopupMenuItem(value: 'delete', child: Text('Remove')),
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
