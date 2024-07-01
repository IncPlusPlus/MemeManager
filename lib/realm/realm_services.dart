import 'package:meme_manager/realm/schemas.dart';
import 'package:realm/realm.dart';
import 'package:flutter/material.dart';

class RealmServices with ChangeNotifier {
  bool offlineModeOn = false;
  bool isWaiting = false;
  late Realm realm;
  // late Configuration config;
  User? currentUser;

  RealmServices() {
    var config = Configuration.local([Meme.schema, Category.schema], initialDataCallback: dataCb);
    realm = Realm(config);
  }

  void dataCb(Realm realm) {
    var memes = <Meme>[];
    for (var i = 0; i < 25; i++) {
      memes.add(Meme(i, 'Meme $i', 'path', DateTime.now(), 'additionalTerms', 0, false));
    }
    realm.addAll(memes);
    var categories = <Category>[];
    for (var i = 0; i < 5; i++) {
      categories.add(Category(i, 'Category $i', children: [], memes: [memes[i*5], memes[(i*5)+1], memes[(i*5)+2], memes[(i*5)+3], memes[(i*5)+4]]));
    }
    realm.addAll(categories);
  }
}