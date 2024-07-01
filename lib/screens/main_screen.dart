import 'package:flutter/material.dart';
import 'package:meme_manager/widgets/category_list.dart';
import 'package:meme_manager/widgets/meme_list.dart';

class MainScreen extends StatefulWidget {
  const MainScreen({super.key});

  @override
  State<MainScreen> createState() => _MainScreenState();
}

class _MainScreenState extends State<MainScreen> {
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        // TRY THIS: Try changing the color here to a specific color (to
        // Colors.amber, perhaps?) and trigger a hot reload to see the AppBar
        // change color while the other colors stay the same.
        backgroundColor: Theme.of(context).colorScheme.inversePrimary,
        // Here we take the value from the MyHomePage object that was created by
        // the App.build method, and use it to set our appbar title.
        title: const Text('Test AppBar'),
      ),
      body: const Center(
        child: Row(
          children: [
            Expanded(flex: 1, child: CategoryList()),
            // TODO: replace with https://pub.dev/packages/multi_split_view or https://pub.dev/packages/flutter_resizable_container in the future
            VerticalDivider(
              width: 2,
              thickness: 1,
            ),
            Expanded(flex: 3, child: MemeList()),
          ],
        ),
      ),
    );
  }
}
