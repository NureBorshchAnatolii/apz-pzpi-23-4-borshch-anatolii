import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../core/api_client.dart';
import '../../core/auth_service.dart';
import 'chat_conversation_screen.dart';

class ChatListScreen extends StatefulWidget {
  const ChatListScreen({super.key});

  @override
  State<ChatListScreen> createState() => _ChatListScreenState();
}

class _ChatListScreenState extends State<ChatListScreen> {
  late Future<List<dynamic>> _future;

  @override
  void initState() {
    super.initState();
    _future = _load();
  }

  Future<List<dynamic>> _load() async {
    final api = context.read<ApiClient>();
    final myId = context.read<AuthService>().userId;
    final r = await api.get('/api/relatives');
    List items = const [];
    if (r is List) {
      items = r;
    } else if (r is Map && r['data'] is List) {
      items = r['data'];
    }
    final seen = <int>{};
    final result = <dynamic>[];
    for (final item in items) {
      if (item is! Map) continue;
      final otherId = (item['relativeId'] ?? item['userId'] ?? 0) as num;
      final otherIdInt = otherId.toInt();
      if (otherIdInt == 0 || otherIdInt == myId) continue;
      if (!seen.add(otherIdInt)) continue;
      result.add(item);
    }
    return result;
  }

  Future<void> _refresh() async {
    setState(() => _future = _load());
    await _future;
  }

  Future<void> _openManualChat() async {
    final ctrl = TextEditingController();
    final id = await showDialog<int>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Open chat with user'),
        content: TextField(
          controller: ctrl,
          keyboardType: TextInputType.number,
          decoration: const InputDecoration(labelText: 'Receiver user ID'),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx),
            child: const Text('Cancel'),
          ),
          ElevatedButton(
            onPressed: () => Navigator.pop(ctx, int.tryParse(ctrl.text.trim())),
            child: const Text('Open'),
          ),
        ],
      ),
    );
    if (id != null && id > 0 && mounted) {
      Navigator.of(context).push(
        MaterialPageRoute(
          builder: (_) =>
              ChatConversationScreen(receiverId: id, receiverName: 'User #$id'),
        ),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Chats'),
        actions: [
          IconButton(
            tooltip: 'Open by ID',
            icon: const Icon(Icons.person_add_alt),
            onPressed: _openManualChat,
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
                  Center(child: Text('No contacts')),
                ],
              );
            }
            return ListView.separated(
              itemCount: items.length,
              separatorBuilder: (_, __) => const Divider(height: 1, indent: 72),
              itemBuilder: (context, i) {
                final r = items[i] as Map? ?? {};
                final id =
                    (r['relativeId'] ?? r['id'] ?? r['userId'] ?? 0) as num;
                final name =
                    (r['relativeFullName'] ??
                            (r['firstName'] != null
                                ? '${r['firstName']} ${r['lastName'] ?? ''}'
                                : (r['name'] ?? 'User #$id')))
                        .toString()
                        .trim();
                return ListTile(
                  leading: CircleAvatar(
                    child: Text(name.isNotEmpty ? name[0].toUpperCase() : '?'),
                  ),
                  title: Text(name),
                  subtitle: Text(
                    (r['relationType'] ?? r['email'] ?? '').toString(),
                  ),
                  trailing: const Icon(Icons.chevron_right),
                  onTap: () => Navigator.of(context).push(
                    MaterialPageRoute(
                      builder: (_) => ChatConversationScreen(
                        receiverId: id.toInt(),
                        receiverName: name,
                      ),
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
