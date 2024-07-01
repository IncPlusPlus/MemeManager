import 'package:flutter/material.dart';
import 'package:meme_manager/models/search_model.dart';
import 'package:meme_manager/realm/realm_services.dart';
import 'package:meme_manager/realm/schemas.dart';
import 'package:provider/provider.dart';
import 'package:realm/realm.dart' hide ConnectionState;

class CategoryList extends StatefulWidget {
  const CategoryList({super.key});

  @override
  State<CategoryList> createState() => _CategoryListState();
}

class _CategoryListState extends State<CategoryList> {
  int? selectedIndex;

  @override
  Widget build(BuildContext context) {
    final realmServices = Provider.of<RealmServices>(context);
    return StreamBuilder<RealmResultsChanges<Category>>(
        stream: realmServices.realm.all<Category>().changes,
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
              return ListView.builder(
                itemCount: results.realm.isClosed ? 0 : results.length,
                itemBuilder: (context, index) {
                  return results[index].isValid
                      ? ListTile(
                          title: Text(results[index].name),
                          // subtitle: Text(results[index].path),
                          tileColor:
                              selectedIndex == index ? Colors.blue : null,
                          onTap: () {
                            var searchModel = context.read<SearchModel>();
                            // Allow the user to deselect the category
                            if (index == selectedIndex) {
                              setState(() {
                                selectedIndex = null;
                              });
                              searchModel.clearCategory();
                            } else {
                              setState(() {
                                selectedIndex = index;
                              });
                              searchModel.selectCategory(results[index].id);
                            }
                          },
                        )
                      : const SizedBox(height: 0);
                },
              );
            } else {
              return const Center(
                child: Text('No categories found'),
              );
            }
          }
        });
  }
}
