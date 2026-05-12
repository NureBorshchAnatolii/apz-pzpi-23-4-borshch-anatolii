import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../core/api_client.dart';

class AddRelativeScreen extends StatefulWidget {
  const AddRelativeScreen({super.key});

  @override
  State<AddRelativeScreen> createState() => _AddRelativeScreenState();
}

class _AddRelativeScreenState extends State<AddRelativeScreen> {
  final _formKey = GlobalKey<FormState>();
  final _relativeId = TextEditingController();
  int? _relationTypeId;
  List<dynamic> _types = const [];
  bool _loadingTypes = true;
  bool _busy = false;

  @override
  void initState() {
    super.initState();
    _loadTypes();
  }

  Future<void> _loadTypes() async {
    try {
      final r = await context.read<ApiClient>().get('/api/relation-types');
      List items = const [];
      if (r is List) items = r;
      else if (r is Map && r['data'] is List) items = r['data'];
      setState(() {
        _types = items;
        _loadingTypes = false;
      });
    } catch (_) {
      setState(() => _loadingTypes = false);
    }
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    if (_relationTypeId == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Pick a relation type')),
      );
      return;
    }
    setState(() => _busy = true);
    try {
      await context.read<ApiClient>().post('/api/relatives', body: {
        'relativeId': int.parse(_relativeId.text.trim()),
        'relationTypeId': _relationTypeId,
      });
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Relative added')),
      );
      Navigator.of(context).pop();
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(e.toString())),
      );
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  @override
  void dispose() {
    _relativeId.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Add relative')),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Form(
          key: _formKey,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              TextFormField(
                controller: _relativeId,
                keyboardType: TextInputType.number,
                decoration: const InputDecoration(
                  labelText: 'Relative user ID',
                  prefixIcon: Icon(Icons.badge_outlined),
                ),
                validator: (v) {
                  final id = int.tryParse((v ?? '').trim());
                  return (id == null || id <= 0) ? 'Enter a valid ID' : null;
                },
              ),
              const SizedBox(height: 12),
              _loadingTypes
                  ? const LinearProgressIndicator()
                  : DropdownButtonFormField<int>(
                      initialValue: _relationTypeId,
                      decoration: const InputDecoration(
                        labelText: 'Relation type',
                      ),
                      items: _types.map<DropdownMenuItem<int>>((r) {
                        final id = (r is Map
                            ? (r['id'] ?? r['relationTypeId'])
                            : null) as num?;
                        final name = (r is Map
                                ? (r['name'] ?? r['title'])
                                : r)
                            ?.toString() ??
                            'Type';
                        return DropdownMenuItem<int>(
                          value: id?.toInt(),
                          child: Text(name),
                        );
                      }).toList(),
                      onChanged: (v) =>
                          setState(() => _relationTypeId = v),
                    ),
              const SizedBox(height: 24),
              ElevatedButton(
                onPressed: _busy ? null : _submit,
                child: _busy
                    ? const SizedBox(
                        height: 22,
                        width: 22,
                        child: CircularProgressIndicator(
                            strokeWidth: 2.4, color: Colors.white),
                      )
                    : const Text('Add relative'),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
