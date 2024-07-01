import 'package:realm/realm.dart';

part 'schemas.realm.dart';

@RealmModel()
class $Meme {
  @PrimaryKey()
  late int id;
  /// The name of the meme. If a meme hasn't been explicitly named, it'll just be displayed with its file name.
  late String name;
  late String path;
  late String? cachedThumbnailPath;
  late DateTime timeAdded;
  // TODO: Add Tags and Category fields
  late String additionalTerms;
  /// One of the constants that represent a type as defined in the constants below the [FileMediaType] enum. realm-dart doesn't support enums yet. See https://github.com/realm/realm-dart/issues/681
  late int mediaType;
  late bool hasBeenCategorized;
}

@RealmModel()
class $Category {
  @PrimaryKey()
  late int id;
  late String name;
  late $Category? parent;
  late List<$Category> children;
  late List<$Meme> memes;
}

