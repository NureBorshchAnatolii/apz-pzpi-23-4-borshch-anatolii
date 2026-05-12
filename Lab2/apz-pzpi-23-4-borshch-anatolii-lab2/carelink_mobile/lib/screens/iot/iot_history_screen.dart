import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';

import '../../core/api_client.dart';
import '../../core/auth_service.dart';
import '../../core/format.dart';

class IoTHistoryScreen extends StatefulWidget {
  const IoTHistoryScreen({super.key});

  @override
  State<IoTHistoryScreen> createState() => _IoTHistoryScreenState();
}

class _IoTHistoryScreenState extends State<IoTHistoryScreen> {
  DateTime _from = DateTime.now().subtract(const Duration(days: 7));
  DateTime _to = DateTime.now();
  Future<List<dynamic>>? _future;

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
      '/api/iot-readings/$userId/range',
      query: {
        'from': _from.toUtc().toIso8601String(),
        'to': _to.toUtc().toIso8601String(),
      },
    );
    if (r is List) return r;
    if (r is Map && r['data'] is List) return r['data'];
    return const [];
  }

  Future<void> _pickFrom() async {
    final p = await showDatePicker(
      context: context,
      firstDate: DateTime(2020),
      lastDate: DateTime.now(),
      initialDate: _from,
    );
    if (p != null) setState(() => _from = p);
  }

  Future<void> _pickTo() async {
    final p = await showDatePicker(
      context: context,
      firstDate: DateTime(2020),
      lastDate: DateTime.now(),
      initialDate: _to,
    );
    if (p != null) setState(() => _to = p);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('IoT History / Range')),
      body: Column(
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(12, 12, 12, 4),
            child: Row(
              children: [
                Expanded(
                  child: OutlinedButton.icon(
                    icon: const Icon(Icons.calendar_today_outlined),
                    label: Text(
                      'From: ${DateFormat('yyyy-MM-dd').format(_from)}',
                    ),
                    onPressed: _pickFrom,
                  ),
                ),
                const SizedBox(width: 8),
                Expanded(
                  child: OutlinedButton.icon(
                    icon: const Icon(Icons.event_outlined),
                    label: Text('To: ${DateFormat('yyyy-MM-dd').format(_to)}'),
                    onPressed: _pickTo,
                  ),
                ),
              ],
            ),
          ),
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 12),
            child: SizedBox(
              width: double.infinity,
              child: ElevatedButton.icon(
                icon: const Icon(Icons.search),
                label: const Text('Load range'),
                onPressed: () => setState(() => _future = _load()),
              ),
            ),
          ),
          Expanded(
            child: FutureBuilder<List<dynamic>>(
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
                  return const Center(child: Text('No data for range'));
                }
                return ListView.builder(
                  itemCount: items.length,
                  itemBuilder: (context, i) {
                    final r = items[i] as Map? ?? {};
                    return Card(
                      child: ListTile(
                        title: Text(
                          'Pulse ${r['pulse'] ?? '-'} · Activity ${r['activityLevel'] ?? '-'} · Temp ${r['temperature'] ?? '-'}',
                        ),
                        subtitle: Text(fmtDateTime(r['readDateTime'])),
                      ),
                    );
                  },
                );
              },
            ),
          ),
        ],
      ),
    );
  }
}
