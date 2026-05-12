import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';

import '../../core/api_client.dart';

class RegisterScreen extends StatefulWidget {
  const RegisterScreen({super.key});

  @override
  State<RegisterScreen> createState() => _RegisterScreenState();
}

class _RegisterScreenState extends State<RegisterScreen> {
  final _formKey = GlobalKey<FormState>();
  final _firstName = TextEditingController();
  final _lastName = TextEditingController();
  final _email = TextEditingController();
  final _password = TextEditingController();
  final _phone = TextEditingController();
  final _address = TextEditingController();
  DateTime? _birthDate;
  List<dynamic> _roles = [];
  int? _roleId;
  bool _loadingRoles = true;
  bool _busy = false;
  bool _obscure = true;

  @override
  void initState() {
    super.initState();
    _loadRoles();
  }

  Future<void> _loadRoles() async {
    try {
      final api = context.read<ApiClient>();
      final r = await api.get('/api/roles');
      List items = const [];
      if (r is List)
        items = r;
      else if (r is Map && r['data'] is List)
        items = r['data'];
      setState(() {
        _roles = items;
        _loadingRoles = false;
      });
    } catch (_) {
      setState(() => _loadingRoles = false);
    }
  }

  Future<void> _pickDate() async {
    final now = DateTime.now();
    final picked = await showDatePicker(
      context: context,
      firstDate: DateTime(now.year - 120),
      lastDate: now,
      initialDate: _birthDate ?? DateTime(now.year - 30),
    );
    if (picked != null) setState(() => _birthDate = picked);
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    if (_birthDate == null) {
      ScaffoldMessenger.of(
        context,
      ).showSnackBar(const SnackBar(content: Text('Please pick a birth date')));
      return;
    }
    if (_roleId == null) {
      ScaffoldMessenger.of(
        context,
      ).showSnackBar(const SnackBar(content: Text('Please select a role')));
      return;
    }
    setState(() => _busy = true);
    try {
      final api = context.read<ApiClient>();
      final res = await api.post(
        '/api/v1/auth/register',
        body: {
          'firstName': _firstName.text.trim(),
          'lastName': _lastName.text.trim(),
          'roleId': _roleId,
          'email': _email.text.trim(),
          'password': _password.text,
          'birthDate': DateFormat('yyyy-MM-dd').format(_birthDate!),
          'address': _address.text.trim(),
          'phoneNumber': _phone.text.trim(),
        },
      );
      final ok = res is Map && res['success'] == true;
      if (!ok) {
        throw Exception(
          res is Map
              ? res['message'] ?? 'Registration failed'
              : 'Registration failed',
        );
      }
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Registration successful, please log in')),
      );
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
    return Scaffold(
      appBar: AppBar(title: const Text('Create account')),
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(20),
          child: Form(
            key: _formKey,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                TextFormField(
                  controller: _firstName,
                  decoration: const InputDecoration(labelText: 'First name'),
                  validator: (v) =>
                      (v == null || v.trim().isEmpty) ? 'Required' : null,
                ),
                const SizedBox(height: 12),
                TextFormField(
                  controller: _lastName,
                  decoration: const InputDecoration(labelText: 'Last name'),
                  validator: (v) =>
                      (v == null || v.trim().isEmpty) ? 'Required' : null,
                ),
                const SizedBox(height: 12),
                TextFormField(
                  controller: _email,
                  keyboardType: TextInputType.emailAddress,
                  decoration: const InputDecoration(labelText: 'Email'),
                  validator: (v) =>
                      (v == null || v.trim().isEmpty) ? 'Required' : null,
                ),
                const SizedBox(height: 12),
                TextFormField(
                  controller: _password,
                  obscureText: _obscure,
                  decoration: InputDecoration(
                    labelText: 'Password',
                    suffixIcon: IconButton(
                      onPressed: () => setState(() => _obscure = !_obscure),
                      icon: Icon(
                        _obscure ? Icons.visibility_off : Icons.visibility,
                      ),
                    ),
                  ),
                  validator: (v) =>
                      v == null || v.length < 6 ? 'Min 6 characters' : null,
                ),
                const SizedBox(height: 12),
                TextFormField(
                  controller: _phone,
                  keyboardType: TextInputType.phone,
                  decoration: const InputDecoration(labelText: 'Phone number'),
                  validator: (v) => (v == null || v.trim().length < 10)
                      ? 'Min 10 chars'
                      : null,
                ),
                const SizedBox(height: 12),
                TextFormField(
                  controller: _address,
                  decoration: const InputDecoration(labelText: 'Address'),
                  validator: (v) =>
                      (v == null || v.trim().length < 5) ? 'Min 5 chars' : null,
                ),
                const SizedBox(height: 12),
                InkWell(
                  onTap: _pickDate,
                  child: InputDecorator(
                    decoration: const InputDecoration(
                      labelText: 'Birth date',
                      prefixIcon: Icon(Icons.calendar_today_outlined),
                    ),
                    child: Text(
                      _birthDate == null
                          ? 'Pick a date'
                          : DateFormat('yyyy-MM-dd').format(_birthDate!),
                    ),
                  ),
                ),
                const SizedBox(height: 12),
                _loadingRoles
                    ? const LinearProgressIndicator()
                    : DropdownButtonFormField<int>(
                        initialValue: _roleId,
                        decoration: const InputDecoration(labelText: 'Role'),
                        items: _roles.map<DropdownMenuItem<int>>((r) {
                          final id =
                              (r is Map
                                      ? (r['id'] ?? r['roleId'] ?? r['Id'])
                                      : null)
                                  as num?;
                          final name =
                              (r is Map
                                      ? (r['name'] ?? r['title'] ?? r['Name'])
                                      : r)
                                  ?.toString() ??
                              'Role';
                          return DropdownMenuItem<int>(
                            value: id?.toInt(),
                            child: Text(name),
                          );
                        }).toList(),
                        onChanged: (v) => setState(() => _roleId = v),
                      ),
                const SizedBox(height: 24),
                ElevatedButton(
                  onPressed: _busy ? null : _submit,
                  child: _busy
                      ? const SizedBox(
                          height: 22,
                          width: 22,
                          child: CircularProgressIndicator(
                            strokeWidth: 2.4,
                            color: Colors.white,
                          ),
                        )
                      : const Text('Create account'),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
