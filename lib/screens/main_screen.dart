import 'package:flutter/cupertino.dart';
import 'package:flutter/material.dart';
import 'package:meme_manager/widgets/category_list.dart';
import 'package:meme_manager/realm/realm_services.dart';
import 'package:meme_manager/realm/schemas.dart';
import 'package:meme_manager/widgets/meme_list.dart';
import 'package:provider/provider.dart';
import 'package:realm/realm.dart' hide ConnectionState;

class MainScreen extends StatefulWidget {
  const MainScreen({super.key});

  @override
  State<MainScreen> createState() => _MainScreenState();
}

class _MainScreenState extends State<MainScreen> {
  @override
  Widget build(BuildContext context) {
    final realmServices = Provider.of<RealmServices>(context);
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
      body: Center(
        child: Row(
          children: [
            Expanded(flex: 1,
                child: CategoryList()                ),
            // TODO: replace with https://pub.dev/packages/multi_split_view or https://pub.dev/packages/flutter_resizable_container in the future
            const VerticalDivider(
              width: 2,
              thickness: 1,
            ),
            Expanded(
                flex: 3,
                child: MemeList()),
            // const CustomScrollView(
            //   slivers: [
            //     SliverList(delegate: SliverChildBuilderDelegate(
            //         (context, index) => Text('index')),
            //     )
            //   ],
            // ),
            // Expanded(
            //     flex: 3,
            //     child: Column(
            //       children: [
            //         Text('String 1',style: Theme.of(context).textTheme.headlineMedium),
            //         Text('String 2',style: Theme.of(context).textTheme.headlineMedium),
            //         Text('String 3',style: Theme.of(context).textTheme.headlineMedium),
            //       ],
            //     )),
          ],
        ),
      ),
    );
  }
}
