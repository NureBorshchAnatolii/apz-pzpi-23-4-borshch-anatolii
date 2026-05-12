import 'package:flutter_test/flutter_test.dart';

import 'package:carelink_mobile/main.dart';

void main() {
  testWidgets('App boots to splash', (WidgetTester tester) async {
    await tester.pumpWidget(const CareLinkApp());
    expect(find.text('CareLink'), findsWidgets);
  });
}
