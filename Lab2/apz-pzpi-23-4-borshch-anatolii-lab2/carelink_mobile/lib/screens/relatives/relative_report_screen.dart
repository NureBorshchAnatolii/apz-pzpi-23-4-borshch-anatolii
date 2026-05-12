import 'dart:typed_data';

import 'package:flutter/material.dart';
import 'package:printing/printing.dart';
import 'package:provider/provider.dart';

import '../../core/api_client.dart';

class RelativeReportScreen extends StatefulWidget {
  const RelativeReportScreen({
    super.key,
    required this.relativeId,
    required this.relativeName,
  });
  final int relativeId;
  final String relativeName;

  @override
  State<RelativeReportScreen> createState() => _RelativeReportScreenState();
}

class _RelativeReportScreenState extends State<RelativeReportScreen> {
  late Future<Uint8List> _future;

  @override
  void initState() {
    super.initState();
    _future = _load();
  }

  Future<Uint8List> _load() async {
    final bytes = await context.read<ApiClient>().getBytes(
      '/api/relatives/${widget.relativeId}/report',
    );
    return Uint8List.fromList(bytes);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text('${widget.relativeName} · Report')),
      body: FutureBuilder<Uint8List>(
        future: _future,
        builder: (context, snap) {
          if (snap.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          }
          if (snap.hasError) {
            return Center(
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Text('Error: ${snap.error}'),
              ),
            );
          }
          final data = snap.data;
          if (data == null || data.isEmpty) {
            return const Center(child: Text('No report available'));
          }
          return PdfPreview(
            build: (format) async => data,
            canChangePageFormat: false,
            canChangeOrientation: false,
            canDebug: false,
            pdfFileName: 'RelativeReport_${widget.relativeId}.pdf',
          );
        },
      ),
    );
  }
}
