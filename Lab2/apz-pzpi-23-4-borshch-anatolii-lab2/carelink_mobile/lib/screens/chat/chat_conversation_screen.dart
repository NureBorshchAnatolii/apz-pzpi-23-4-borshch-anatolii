import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../core/api_client.dart';
import '../../core/auth_service.dart';
import '../../core/format.dart';

class ChatConversationScreen extends StatefulWidget {
  const ChatConversationScreen({
    super.key,
    required this.receiverId,
    required this.receiverName,
  });
  final int receiverId;
  final String receiverName;

  @override
  State<ChatConversationScreen> createState() => _ChatConversationScreenState();
}

class _ChatConversationScreenState extends State<ChatConversationScreen> {
  final _input = TextEditingController();
  List<dynamic> _messages = const [];
  bool _loading = true;
  bool _sending = false;

  @override
  void initState() {
    super.initState();
    _load();
  }

  @override
  void dispose() {
    _input.dispose();
    super.dispose();
  }

  Future<void> _load() async {
    setState(() => _loading = true);
    try {
      final r = await context.read<ApiClient>().get(
        '/api/messages/user/${widget.receiverId}',
      );
      List items = const [];
      if (r is List)
        items = r;
      else if (r is Map && r['data'] is List)
        items = r['data'];
      setState(() {
        _messages = items;
        _loading = false;
      });
    } catch (e) {
      setState(() => _loading = false);
      if (!mounted) return;
      ScaffoldMessenger.of(
        context,
      ).showSnackBar(SnackBar(content: Text(e.toString())));
    }
  }

  Future<void> _send() async {
    final text = _input.text.trim();
    if (text.isEmpty) return;
    setState(() => _sending = true);
    try {
      await context.read<ApiClient>().post(
        '/api/messages',
        body: {'content': text, 'receiverId': widget.receiverId},
      );
      _input.clear();
      await _load();
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(
        context,
      ).showSnackBar(SnackBar(content: Text(e.toString())));
    } finally {
      if (mounted) setState(() => _sending = false);
    }
  }

  Future<void> _editMessage(Map m) async {
    final id = m['id'] ?? m['messageId'];
    if (id == null) return;
    final ctrl = TextEditingController(text: (m['content'] ?? '').toString());
    final newText = await showDialog<String>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Edit message'),
        content: TextField(
          controller: ctrl,
          decoration: const InputDecoration(),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx),
            child: const Text('Cancel'),
          ),
          ElevatedButton(
            onPressed: () => Navigator.pop(ctx, ctrl.text.trim()),
            child: const Text('Save'),
          ),
        ],
      ),
    );
    if (newText == null || newText.isEmpty) return;
    try {
      await context.read<ApiClient>().put(
        '/api/messages/$id',
        body: {'newContent': newText},
      );
      await _load();
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(
        context,
      ).showSnackBar(SnackBar(content: Text(e.toString())));
    }
  }

  Future<void> _deleteMessage(Map m) async {
    final id = m['id'] ?? m['messageId'];
    if (id == null) return;
    try {
      await context.read<ApiClient>().delete('/api/messages/$id');
      await _load();
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(
        context,
      ).showSnackBar(SnackBar(content: Text(e.toString())));
    }
  }

  @override
  Widget build(BuildContext context) {
    final myId = context.watch<AuthService>().userId;
    return Scaffold(
      appBar: AppBar(title: Text(widget.receiverName)),
      body: Column(
        children: [
          Expanded(
            child: _loading
                ? const Center(child: CircularProgressIndicator())
                : _messages.isEmpty
                ? const Center(child: Text('No messages yet'))
                : ListView.builder(
                    reverse: false,
                    padding: const EdgeInsets.all(12),
                    itemCount: _messages.length,
                    itemBuilder: (context, i) {
                      final m = _messages[i] as Map? ?? {};
                      final senderId = (m['senderId'] ?? 0) as num;
                      final mine = senderId.toInt() == myId;
                      return Align(
                        alignment: mine
                            ? Alignment.centerRight
                            : Alignment.centerLeft,
                        child: GestureDetector(
                          onLongPress: mine
                              ? () async {
                                  final action =
                                      await showModalBottomSheet<String>(
                                        context: context,
                                        builder: (ctx) => SafeArea(
                                          child: Column(
                                            mainAxisSize: MainAxisSize.min,
                                            children: [
                                              ListTile(
                                                leading: const Icon(
                                                  Icons.edit_outlined,
                                                ),
                                                title: const Text('Edit'),
                                                onTap: () =>
                                                    Navigator.pop(ctx, 'edit'),
                                              ),
                                              ListTile(
                                                leading: const Icon(
                                                  Icons.delete_outline,
                                                  color: Colors.red,
                                                ),
                                                title: const Text(
                                                  'Delete',
                                                  style: TextStyle(
                                                    color: Colors.red,
                                                  ),
                                                ),
                                                onTap: () => Navigator.pop(
                                                  ctx,
                                                  'delete',
                                                ),
                                              ),
                                            ],
                                          ),
                                        ),
                                      );
                                  if (action == 'edit') {
                                    _editMessage(m);
                                  } else if (action == 'delete') {
                                    _deleteMessage(m);
                                  }
                                }
                              : null,
                          child: Container(
                            margin: const EdgeInsets.symmetric(vertical: 4),
                            padding: const EdgeInsets.symmetric(
                              horizontal: 14,
                              vertical: 10,
                            ),
                            constraints: BoxConstraints(
                              maxWidth:
                                  MediaQuery.of(context).size.width * 0.75,
                            ),
                            decoration: BoxDecoration(
                              color: mine
                                  ? Theme.of(context).colorScheme.primary
                                  : Colors.white,
                              borderRadius: BorderRadius.circular(14),
                              border: mine
                                  ? null
                                  : Border.all(color: Colors.grey.shade300),
                            ),
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Text(
                                  (m['content'] ?? '').toString(),
                                  style: TextStyle(
                                    color: mine ? Colors.white : Colors.black87,
                                  ),
                                ),
                                const SizedBox(height: 4),
                                Text(
                                  fmtDateTime(m['sentAt'] ?? m['createdAt']),
                                  style: TextStyle(
                                    fontSize: 11,
                                    color: mine
                                        ? Colors.white70
                                        : Colors.black54,
                                  ),
                                ),
                              ],
                            ),
                          ),
                        ),
                      );
                    },
                  ),
          ),
          SafeArea(
            top: false,
            child: Padding(
              padding: const EdgeInsets.all(8),
              child: Row(
                children: [
                  Expanded(
                    child: TextField(
                      controller: _input,
                      minLines: 1,
                      maxLines: 4,
                      decoration: const InputDecoration(
                        hintText: 'Type a message...',
                      ),
                    ),
                  ),
                  const SizedBox(width: 8),
                  IconButton.filled(
                    onPressed: _sending ? null : _send,
                    icon: const Icon(Icons.send),
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }
}
