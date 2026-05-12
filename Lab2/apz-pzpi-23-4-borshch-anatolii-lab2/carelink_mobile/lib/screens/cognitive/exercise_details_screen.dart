import 'dart:math';

import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../core/api_client.dart';
import '../../core/auth_service.dart';

class ExerciseDetailsScreen extends StatefulWidget {
  const ExerciseDetailsScreen({super.key, required this.exercise});
  final Map exercise;

  @override
  State<ExerciseDetailsScreen> createState() => _ExerciseDetailsScreenState();
}

class _ExerciseDetailsScreenState extends State<ExerciseDetailsScreen> {
  bool _playing = false;
  int _score = 0;
  int _round = 0;
  int? _target;
  List<int> _options = const [];

  void _startGame() {
    setState(() {
      _playing = true;
      _score = 0;
      _round = 0;
      _nextRound();
    });
  }

  void _nextRound() {
    final rng = Random();
    final a = rng.nextInt(20) + 1;
    final b = rng.nextInt(20) + 1;
    _target = a + b;
    final opts = <int>{_target!};
    while (opts.length < 4) {
      opts.add(_target! + rng.nextInt(11) - 5);
    }
    final list = opts.toList()..shuffle();
    _options = list;
  }

  Future<void> _pick(int v) async {
    if (v == _target) _score += 10;
    _round++;
    if (_round >= 5) {
      await _submit();
    } else {
      setState(_nextRound);
    }
  }

  Future<void> _submit() async {
    final id =
        widget.exercise['id'] ??
        widget.exercise['exerciseId'] ??
        widget.exercise['Id'];
    final userId = context.read<AuthService>().userId;
    if (id == null || userId == null) {
      setState(() => _playing = false);
      return;
    }
    try {
      await context.read<ApiClient>().post(
        '/api/cognitive-exercise/$id/result',
        body: {
          'userId': userId,
          'score': _score,
          'completedAt': DateTime.now().toUtc().toIso8601String(),
        },
      );
      if (!mounted) return;
      showDialog(
        context: context,
        builder: (ctx) => AlertDialog(
          title: const Text('Game finished'),
          content: Text('Your score: $_score'),
          actions: [
            TextButton(
              onPressed: () => Navigator.pop(ctx),
              child: const Text('OK'),
            ),
          ],
        ),
      );
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(
        context,
      ).showSnackBar(SnackBar(content: Text(e.toString())));
    } finally {
      if (mounted) setState(() => _playing = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final e = widget.exercise;
    return Scaffold(
      appBar: AppBar(title: Text((e['title'] ?? 'Exercise').toString())),
      body: Padding(
        padding: const EdgeInsets.all(16),
        child: _playing ? _buildGame() : _buildIntro(e),
      ),
    );
  }

  Widget _buildIntro(Map e) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Card(
          child: Padding(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  (e['title'] ?? 'Exercise').toString(),
                  style: const TextStyle(
                    fontSize: 22,
                    fontWeight: FontWeight.w700,
                  ),
                ),
                const SizedBox(height: 8),
                Text(
                  (e['description'] ??
                          'A small cognitive game. Answer 5 quick questions.')
                      .toString(),
                ),
                const SizedBox(height: 12),
                Wrap(
                  spacing: 8,
                  children: [
                    if (e['difficulty'] != null)
                      Chip(label: Text('Difficulty: ${e['difficulty']}')),
                    if (e['type'] != null)
                      Chip(label: Text('Type: ${e['type']}')),
                  ],
                ),
              ],
            ),
          ),
        ),
        const Spacer(),
        ElevatedButton.icon(
          onPressed: _startGame,
          icon: const Icon(Icons.play_arrow),
          label: const Text('Start exercise'),
        ),
      ],
    );
  }

  Widget _buildGame() {
    final t = _target!;
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        LinearProgressIndicator(value: _round / 5),
        const SizedBox(height: 16),
        Text(
          'Round ${_round + 1}/5 · Score $_score',
          style: const TextStyle(color: Colors.black54),
        ),
        const SizedBox(height: 32),
        Center(
          child: Text(
            'What is the answer?\n(target = $t)',
            textAlign: TextAlign.center,
            style: const TextStyle(fontSize: 22, fontWeight: FontWeight.w600),
          ),
        ),
        const SizedBox(height: 32),
        ..._options.map(
          (o) => Padding(
            padding: const EdgeInsets.symmetric(vertical: 6),
            child: OutlinedButton(
              onPressed: () => _pick(o),
              child: Text(o.toString()),
            ),
          ),
        ),
      ],
    );
  }
}
