import 'package:flutter/material.dart';
import 'package:meme_manager/models/search_model.dart';
import 'package:meme_manager/realm/realm_services.dart';
import 'package:provider/provider.dart';
import 'package:realm/realm.dart' hide ConnectionState;

import 'package:meme_manager/realm/schemas.dart';

class MemeList extends StatefulWidget {
  const MemeList({Key? key}) : super(key: key);

  @override
  State<MemeList> createState() => _MemeListState();
}

class _MemeListState extends State<MemeList> {
  @override
  Widget build(BuildContext context) {
    final realmServices = Provider.of<RealmServices>(context);
    final int? selectedCategory =
        context.select((SearchModel model) => model.selectedCategory);
    // Stream<RealmResultsChanges<Meme>> allMemes = realmServices.realm.all<Meme>().changes;
    // Stream<RealmListChanges<Meme>> allMemesWhereCategoryIsCategory = realmServices.realm.find<Category>(selectedCategory)!.memes.changes;
    if (selectedCategory == null) {
      return StreamBuilder<RealmResultsChanges<Meme>>(
          stream: realmServices.realm.all<Meme>().changes,
          builder: (context, snapshot) {
            if (snapshot.connectionState == ConnectionState.waiting) {
              return const Center(
                child: CircularProgressIndicator(),
              );
            } else {
              if (snapshot.hasData &&
                  snapshot.data != null &&
                  snapshot.data!.results.isNotEmpty) {
                final results = snapshot.data!.results;
                return GridView.builder(
                  gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                    crossAxisCount: 5,
                  ),
                  itemCount: results.realm.isClosed ? 0 : results.length,
                  itemBuilder: (context, index) {
                    return results[index].isValid
                        ? ListTile(
                            title: Text(results[index].name),
                            subtitle: Text(results[index].path),
                          )
                        : const SizedBox(height: 0);
                  },
                );
              } else {
                return const Center(
                  child: Text('No memes found'),
                );
              }
            }
          });
    } else {
      return StreamBuilder<RealmListChanges<Meme>>(
          stream: realmServices.realm
              .find<Category>(selectedCategory)!
              .memes
              .changes,
          builder: (context, snapshot) {
            if (snapshot.connectionState == ConnectionState.waiting) {
              return const Center(
                child: CircularProgressIndicator(),
              );
            } else {
              if (snapshot.hasData &&
                  snapshot.data != null &&
                  snapshot.data!.list.isNotEmpty) {
                final results = snapshot.data!.list;
                return GridView.builder(
                  gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                    crossAxisCount: 5,
                  ),
                  itemCount: results.realm.isClosed ? 0 : results.length,
                  itemBuilder: (context, index) {
                    return results[index].isValid
                        ? ListTile(
                            title: Text(results[index].name),
                            subtitle: Text(results[index].path),
                          )
                        : const SizedBox(height: 0);
                  },
                );
              } else {
                return const Center(
                  child: Text('No memes found'),
                );
              }
            }
          });
    }
  }
}
