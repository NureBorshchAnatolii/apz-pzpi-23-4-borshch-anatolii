import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../core/api_client.dart';
import '../../core/format.dart';

class NotificationDetailsScreen extends StatefulWidget {
  const NotificationDetailsScreen({super.key, required this.notification});
  final Map notification;

  @override
  State<NotificationDetailsScreen> createState() =>
      _NotificationDetailsScreenState();
}

class _NotificationDetailsScreenState extends State<NotificationDetailsScreen> {
  bool _busy = false;

  Future<void> _markRead() async {
    final id =
        widget.notification['id'] ?? widget.notification['notificationId'];
    if (id == null) return;
    setState(() => _busy = true);
    try {
      await context.read<ApiClient>().put('/api/notifications/$id');
      if (!mounted) return;
      ScaffoldMessenger.of(
        context,
      ).showSnackBar(const SnackBar(content: Text('Marked as read')));
      Navigator.of(context).pop();
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(
        context,
      ).showSnackBar(SnackBar(content: Text(e.toString())));
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final n = widget.notification;
    return Scaffold(
      appBar: AppBar(title: const Text('Notification details')),
      body: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Card(
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      (n['title'] ?? n['type'] ?? 'Notification').toString(),
                      style: const TextStyle(
                        fontSize: 20,
                        fontWeight: FontWeight.w700,
                      ),
                    ),
                    const SizedBox(height: 8),
                    Text(
                      fmtDateTime(n['createdAt'] ?? n['date']),
                      style: const TextStyle(color: Colors.black54),
                    ),
                    const Divider(height: 24),
                    Text(
                      (n['message'] ?? n['content'] ?? '-').toString(),
                      style: const TextStyle(fontSize: 16),
                    ),
                  ],
                ),
              ),
            ),
            const Spacer(),
            ElevatedButton.icon(
              onPressed: _busy ? null : _markRead,
              icon: const Icon(Icons.done_all),
              label: Text(_busy ? 'Working...' : 'Mark as read'),
            ),
          ],
        ),
      ),
    );
  }
}
