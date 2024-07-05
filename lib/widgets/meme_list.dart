import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:meme_manager/models/search_model.dart';
import 'package:meme_manager/realm/realm_services.dart';
import 'package:provider/provider.dart';
import 'package:quiver/iterables.dart';
import 'package:realm/realm.dart' hide ConnectionState;

import 'package:meme_manager/realm/schemas.dart';

import 'inkwell_splash.dart';

class MemeList extends StatefulWidget {
  const MemeList({super.key});

  @override
  State<MemeList> createState() => _MemeListState();
}

class _MemeListState extends State<MemeList> {
  @override
  Widget build(BuildContext context) {
    final realmServices = Provider.of<RealmServices>(context);
    final int? selectedCategory =
        context.select((SearchModel model) => model.selectedCategory);
    // This is an ugly, ugly hack.
    // I wish I could combine the two stream types so I don't repeat code.
    // The problem is one class has the results property and one has the list property.
    // Alas, my feeble grasp of Dart prevents such a feat.
    if (selectedCategory == null) {
      return StreamBuilder<RealmResultsChanges<Meme>>(
          stream: realmServices.realm.all<Meme>().changes,
          builder: (context, snapshot) => memeListBuilder(
              context,
              snapshot,
              snapshot.hasData &&
                  snapshot.data != null &&
                  snapshot.data!.results.isNotEmpty,
              snapshot.data?.results));
    } else {
      return StreamBuilder<RealmListChanges<Meme>>(
          stream: realmServices.realm
              .find<Category>(selectedCategory)!
              .memes
              .changes,
          builder: (context, snapshot) => memeListBuilder(
              context,
              snapshot,
              snapshot.hasData &&
                  snapshot.data != null &&
                  snapshot.data!.list.isNotEmpty,
              snapshot.data?.list));
    }
  }

  Widget memeListBuilder(context, snapshot, hasDataBool, resultsVar) {
    if (snapshot.connectionState == ConnectionState.waiting) {
      return const Center(
        child: CircularProgressIndicator(),
      );
    } else {
      if (hasDataBool) {
        final results = resultsVar;
        return InnerMemeList(results: results);
      } else {
        return const Center(
          child: Text('No memes found'),
        );
      }
    }
  }
}

class InnerMemeList extends StatefulWidget {
  // Will either be a RealmResults or a RealmList
  final dynamic results;

  const InnerMemeList({super.key, required this.results});

  @override
  State<InnerMemeList> createState() => _InnerMemeListState();
}

class _InnerMemeListState extends State<InnerMemeList> {
  Set<int> selectedIndexList = <int>{};
  int? lastSelectedIndex;

  /*
   * The pivot point is what I am calling the start point of the most recent selection.
   * Here's where it comes into effect: say you just single clicked meme 12. Then, you
   * hold shift and click meme 18. Memes 12-18 become selected. In this case, the pivot
   * point is 12. If you then hold shift and click meme 6, memes 6-12 should become selected.
   * If I relied solely on the last selected index, the selection would be from 18-6, which is
   * not the behavior exhibited in Windows File Explorer.
   */
  int? selectionPivotPoint;

  @override
  Widget build(BuildContext context) {
    return GridView.builder(
      gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
        crossAxisCount: 5,
      ),
      itemCount: widget.results.realm.isClosed ? 0 : widget.results.length,
      itemBuilder: (context, index) {
        return widget.results[index].isValid
            ? InkWellSplash(
                doubleTapTime: const Duration(milliseconds: 150),
                key: ValueKey(widget.results[index].id),
                onTap: () {
                  /*
                   * The goal of this ugly block of logic is to emulate the
                   * selection behavior of Windows Explorer since that's what
                   * most people are familiar with.
                   */
                  var shiftPressed = HardwareKeyboard.instance.isShiftPressed;
                  var ctrlPressed = HardwareKeyboard.instance.isControlPressed;
                  // print('Current selection: ${selectedIndexList.toString()}');
                  if (lastSelectedIndex == null) {
                    setState(() {
                      selectionPivotPoint = null;
                      lastSelectedIndex = index;
                      selectedIndexList = <int>{index};
                    });
                  } else {
                    if (shiftPressed && ctrlPressed) {
                      // Do a shift select and include previous selections
                      Iterable<int> newSelection = range(
                              lastSelectedIndex! +
                                  (lastSelectedIndex! < index ? 1 : -1),
                              index + (lastSelectedIndex! < index ? 1 : -1),
                              lastSelectedIndex! < index ? 1 : -1)
                          .map((e) => e.toInt());
                      setState(() {
                        selectionPivotPoint =
                            selectionPivotPoint ?? lastSelectedIndex;
                        lastSelectedIndex = index;
                        // selectedIndexList.addAll(newSelection);
                        performSelection(
                            selectedIndexList, newSelection.toSet());
                      });
                    } else if (shiftPressed) {
                      // Do a shift select
                      Iterable<int> newSelection = range(
                              selectionPivotPoint ??
                                  lastSelectedIndex! +
                                      ((selectionPivotPoint ??
                                                  lastSelectedIndex!) <
                                              index
                                          ? 1
                                          : -1),
                              index +
                                  ((selectionPivotPoint ?? lastSelectedIndex!) <
                                          index
                                      ? 1
                                      : -1),
                              (selectionPivotPoint ?? lastSelectedIndex!) <
                                      index
                                  ? 1
                                  : -1)
                          .map((e) => e.toInt());
                      setState(() {
                        selectionPivotPoint =
                            selectionPivotPoint ?? lastSelectedIndex;
                        // Include the most recently selected item as part of the shift select
                        selectedIndexList = {
                          selectionPivotPoint!,
                          ...newSelection.toSet()
                        };
                        lastSelectedIndex = index;
                      });
                    } else if (ctrlPressed) {
                      // Do a ctrl select
                      setState(() {
                        selectionPivotPoint = null;
                        lastSelectedIndex = index;
                        if (selectedIndexList.contains(index)) {
                          selectedIndexList.remove(index);
                        } else {
                          selectedIndexList.add(index);
                        }
                      });
                    } else {
                      // Do a single select
                      setState(() {
                        selectionPivotPoint = null;
                        lastSelectedIndex = index;
                        if (selectedIndexList.length == 1 &&
                            selectedIndexList.first == index) {
                          selectedIndexList = <int>{};
                        } else {
                          selectedIndexList = <int>{index};
                        }
                      });
                    }
                  }
                  // print('New selection: ${selectedIndexList.toString()}');
                },
                // onSecondaryTap: () {
                //   print(
                //       'Right clicked. Selection: ${selectedIndexList.toString()}');
                // },
                // onDoubleTap: () {
                //   print(
                //       'Double tap element $index with ${selectedIndexList.toString()}');
                // },
                child: Padding(
                  padding: const EdgeInsets.all(8.0),
                  child: ListTile(
                    selected: selectedIndexList.contains(index),
                    tileColor: Theme.of(context).colorScheme.inversePrimary,
                    selectedTileColor: Theme.of(context).colorScheme.primary,
                    selectedColor: Theme.of(context).colorScheme.inversePrimary,
                    title: Text(widget.results[index].name),
                    subtitle: Text(widget.results[index].path),
                  ),
                ))
            : const SizedBox(height: 200);
      },
    );
  }

  Set<int> performSelection(Set<int> initialSelection, Set<int> newSelectors) {
    // Takes the initialSelection and adds the newSelectors to it. If the newSelectors are already in the initialSelection, they are removed.
    bool selectionRemovesElements = false;
    // Check if initialSelection contains any element in newSelection
    // If so, set selectionRemovesElements to true
    for (var element in newSelectors) {
      if (initialSelection.contains(element)) {
        selectionRemovesElements = true;
        // No need to keep checking
        break;
      }
    }
    for (var element in newSelectors) {
      if (initialSelection.contains(element)) {
        initialSelection.remove(element);
      } else {
        // When doing a CTRL+Shift select, Windows explorer only gets rid of
        // overlapping selections. If you extend the selection past elements
        // that are already selected, no new elements get selected.
        if (!selectionRemovesElements) {
          initialSelection.add(element);
        }
      }
    }
    return initialSelection;
  }
}
