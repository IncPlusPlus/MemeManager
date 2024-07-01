import 'package:flutter/foundation.dart';

class SearchModel extends ChangeNotifier {
  int? selectedCategory;

  void selectCategory(int category) {
    selectedCategory = category;
    notifyListeners();
  }

  void clearCategory() {
    selectedCategory = null;
    notifyListeners();
  }
}