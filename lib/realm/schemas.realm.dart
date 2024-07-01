// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'schemas.dart';

// **************************************************************************
// RealmObjectGenerator
// **************************************************************************

// ignore_for_file: type=lint
class Meme extends $Meme with RealmEntity, RealmObjectBase, RealmObject {
  Meme(
    int id,
    String name,
    String path,
    DateTime timeAdded,
    String additionalTerms,
    int mediaType,
    bool hasBeenCategorized, {
    String? cachedThumbnailPath,
  }) {
    RealmObjectBase.set(this, 'id', id);
    RealmObjectBase.set(this, 'name', name);
    RealmObjectBase.set(this, 'path', path);
    RealmObjectBase.set(this, 'cachedThumbnailPath', cachedThumbnailPath);
    RealmObjectBase.set(this, 'timeAdded', timeAdded);
    RealmObjectBase.set(this, 'additionalTerms', additionalTerms);
    RealmObjectBase.set(this, 'mediaType', mediaType);
    RealmObjectBase.set(this, 'hasBeenCategorized', hasBeenCategorized);
  }

  Meme._();

  @override
  int get id => RealmObjectBase.get<int>(this, 'id') as int;
  @override
  set id(int value) => RealmObjectBase.set(this, 'id', value);

  @override
  String get name => RealmObjectBase.get<String>(this, 'name') as String;
  @override
  set name(String value) => RealmObjectBase.set(this, 'name', value);

  @override
  String get path => RealmObjectBase.get<String>(this, 'path') as String;
  @override
  set path(String value) => RealmObjectBase.set(this, 'path', value);

  @override
  String? get cachedThumbnailPath =>
      RealmObjectBase.get<String>(this, 'cachedThumbnailPath') as String?;
  @override
  set cachedThumbnailPath(String? value) =>
      RealmObjectBase.set(this, 'cachedThumbnailPath', value);

  @override
  DateTime get timeAdded =>
      RealmObjectBase.get<DateTime>(this, 'timeAdded') as DateTime;
  @override
  set timeAdded(DateTime value) =>
      RealmObjectBase.set(this, 'timeAdded', value);

  @override
  String get additionalTerms =>
      RealmObjectBase.get<String>(this, 'additionalTerms') as String;
  @override
  set additionalTerms(String value) =>
      RealmObjectBase.set(this, 'additionalTerms', value);

  @override
  int get mediaType => RealmObjectBase.get<int>(this, 'mediaType') as int;
  @override
  set mediaType(int value) => RealmObjectBase.set(this, 'mediaType', value);

  @override
  bool get hasBeenCategorized =>
      RealmObjectBase.get<bool>(this, 'hasBeenCategorized') as bool;
  @override
  set hasBeenCategorized(bool value) =>
      RealmObjectBase.set(this, 'hasBeenCategorized', value);

  @override
  Stream<RealmObjectChanges<Meme>> get changes =>
      RealmObjectBase.getChanges<Meme>(this);

  @override
  Stream<RealmObjectChanges<Meme>> changesFor([List<String>? keyPaths]) =>
      RealmObjectBase.getChangesFor<Meme>(this, keyPaths);

  @override
  Meme freeze() => RealmObjectBase.freezeObject<Meme>(this);

  EJsonValue toEJson() {
    return <String, dynamic>{
      'id': id.toEJson(),
      'name': name.toEJson(),
      'path': path.toEJson(),
      'cachedThumbnailPath': cachedThumbnailPath.toEJson(),
      'timeAdded': timeAdded.toEJson(),
      'additionalTerms': additionalTerms.toEJson(),
      'mediaType': mediaType.toEJson(),
      'hasBeenCategorized': hasBeenCategorized.toEJson(),
    };
  }

  static EJsonValue _toEJson(Meme value) => value.toEJson();
  static Meme _fromEJson(EJsonValue ejson) {
    return switch (ejson) {
      {
        'id': EJsonValue id,
        'name': EJsonValue name,
        'path': EJsonValue path,
        'cachedThumbnailPath': EJsonValue cachedThumbnailPath,
        'timeAdded': EJsonValue timeAdded,
        'additionalTerms': EJsonValue additionalTerms,
        'mediaType': EJsonValue mediaType,
        'hasBeenCategorized': EJsonValue hasBeenCategorized,
      } =>
        Meme(
          fromEJson(id),
          fromEJson(name),
          fromEJson(path),
          fromEJson(timeAdded),
          fromEJson(additionalTerms),
          fromEJson(mediaType),
          fromEJson(hasBeenCategorized),
          cachedThumbnailPath: fromEJson(cachedThumbnailPath),
        ),
      _ => raiseInvalidEJson(ejson),
    };
  }

  static final schema = () {
    RealmObjectBase.registerFactory(Meme._);
    register(_toEJson, _fromEJson);
    return SchemaObject(ObjectType.realmObject, Meme, 'Meme', [
      SchemaProperty('id', RealmPropertyType.int, primaryKey: true),
      SchemaProperty('name', RealmPropertyType.string),
      SchemaProperty('path', RealmPropertyType.string),
      SchemaProperty('cachedThumbnailPath', RealmPropertyType.string,
          optional: true),
      SchemaProperty('timeAdded', RealmPropertyType.timestamp),
      SchemaProperty('additionalTerms', RealmPropertyType.string),
      SchemaProperty('mediaType', RealmPropertyType.int),
      SchemaProperty('hasBeenCategorized', RealmPropertyType.bool),
    ]);
  }();

  @override
  SchemaObject get objectSchema => RealmObjectBase.getSchema(this) ?? schema;
}

class Category extends $Category
    with RealmEntity, RealmObjectBase, RealmObject {
  Category(
    int id,
    String name, {
    Category? parent,
    Iterable<Category> children = const [],
    Iterable<Meme> memes = const [],
  }) {
    RealmObjectBase.set(this, 'id', id);
    RealmObjectBase.set(this, 'name', name);
    RealmObjectBase.set(this, 'parent', parent);
    RealmObjectBase.set<RealmList<Category>>(
        this, 'children', RealmList<Category>(children));
    RealmObjectBase.set<RealmList<Meme>>(this, 'memes', RealmList<Meme>(memes));
  }

  Category._();

  @override
  int get id => RealmObjectBase.get<int>(this, 'id') as int;
  @override
  set id(int value) => RealmObjectBase.set(this, 'id', value);

  @override
  String get name => RealmObjectBase.get<String>(this, 'name') as String;
  @override
  set name(String value) => RealmObjectBase.set(this, 'name', value);

  @override
  Category? get parent =>
      RealmObjectBase.get<Category>(this, 'parent') as Category?;
  @override
  set parent(covariant Category? value) =>
      RealmObjectBase.set(this, 'parent', value);

  @override
  RealmList<Category> get children =>
      RealmObjectBase.get<Category>(this, 'children') as RealmList<Category>;
  @override
  set children(covariant RealmList<Category> value) =>
      throw RealmUnsupportedSetError();

  @override
  RealmList<Meme> get memes =>
      RealmObjectBase.get<Meme>(this, 'memes') as RealmList<Meme>;
  @override
  set memes(covariant RealmList<Meme> value) =>
      throw RealmUnsupportedSetError();

  @override
  Stream<RealmObjectChanges<Category>> get changes =>
      RealmObjectBase.getChanges<Category>(this);

  @override
  Stream<RealmObjectChanges<Category>> changesFor([List<String>? keyPaths]) =>
      RealmObjectBase.getChangesFor<Category>(this, keyPaths);

  @override
  Category freeze() => RealmObjectBase.freezeObject<Category>(this);

  EJsonValue toEJson() {
    return <String, dynamic>{
      'id': id.toEJson(),
      'name': name.toEJson(),
      'parent': parent.toEJson(),
      'children': children.toEJson(),
      'memes': memes.toEJson(),
    };
  }

  static EJsonValue _toEJson(Category value) => value.toEJson();
  static Category _fromEJson(EJsonValue ejson) {
    return switch (ejson) {
      {
        'id': EJsonValue id,
        'name': EJsonValue name,
        'parent': EJsonValue parent,
        'children': EJsonValue children,
        'memes': EJsonValue memes,
      } =>
        Category(
          fromEJson(id),
          fromEJson(name),
          parent: fromEJson(parent),
          children: fromEJson(children),
          memes: fromEJson(memes),
        ),
      _ => raiseInvalidEJson(ejson),
    };
  }

  static final schema = () {
    RealmObjectBase.registerFactory(Category._);
    register(_toEJson, _fromEJson);
    return SchemaObject(ObjectType.realmObject, Category, 'Category', [
      SchemaProperty('id', RealmPropertyType.int, primaryKey: true),
      SchemaProperty('name', RealmPropertyType.string),
      SchemaProperty('parent', RealmPropertyType.object,
          optional: true, linkTarget: 'Category'),
      SchemaProperty('children', RealmPropertyType.object,
          linkTarget: 'Category', collectionType: RealmCollectionType.list),
      SchemaProperty('memes', RealmPropertyType.object,
          linkTarget: 'Meme', collectionType: RealmCollectionType.list),
    ]);
  }();

  @override
  SchemaObject get objectSchema => RealmObjectBase.getSchema(this) ?? schema;
}
