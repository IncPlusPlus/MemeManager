import 'dart:convert';

import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:realm/realm.dart';

class Config extends ChangeNotifier {
  late String appId;
  late String atlasUrl;
  late Uri baseUrl;

  Config._create(dynamic realmConfig) {
    appId = realmConfig['appId'];
    atlasUrl = realmConfig['dataExplorerLink'];
    baseUrl = Uri.parse(realmConfig['baseUrl']);
  }
  static Future<Config> getConfig(String jsonConfigPath) async {
    dynamic realmConfig =
    json.decode(await rootBundle.loadString(jsonConfigPath));
    var config = Config._create(realmConfig);

    return config;
  }
}

class AppServices with ChangeNotifier {
  // String id;
  // Uri baseUrl;
  // App app;
  User? currentUser;
  AppServices();
}