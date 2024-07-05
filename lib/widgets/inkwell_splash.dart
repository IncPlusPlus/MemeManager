// This is a rewrite of https://github.com/ManuelRohrauer/inkwell_splash
// This is required to make selection not feel sluggish when using both an onTap
// and onDoubleTap callback defined for an InkWell.
// See https://stackoverflow.com/a/61295302/1687436
// Use until https://github.com/flutter/flutter/issues/106170 is resolved.
// Although, I wonder if https://stackoverflow.com/a/78217050/1687436
// would work as well. Maybe I'll try that later. It's certainly more concise.

import 'package:flutter/material.dart';
import 'dart:async';

class InkWellSplash extends StatefulWidget {
  const InkWellSplash({
    super.key,
    this.child,
    this.onTap,
    this.onDoubleTap,
    this.doubleTapTime = const Duration(milliseconds: 300),
    this.onLongPress,
    this.onTapDown,
    this.onTapUp,
    this.onTapCancel,
    this.onSecondaryTap,
    this.onSecondaryTapUp,
    this.onSecondaryTapDown,
    this.onSecondaryTapCancel,
    this.onHighlightChanged,
    this.onHover,
    this.mouseCursor,
    this.focusColor,
    this.hoverColor,
    this.highlightColor,
    this.overlayColor,
    this.splashColor,
    this.splashFactory,
    this.radius,
    this.borderRadius,
    this.customBorder,
    this.enableFeedback = true,
    this.excludeFromSemantics = false,
    this.focusNode,
    this.canRequestFocus = true,
    this.onFocusChange,
    this.autofocus = false,
    this.statesController,
    this.hoverDuration,
  });

  final Widget? child;
  final GestureTapCallback? onTap;
  final GestureTapCallback? onDoubleTap;
  final Duration doubleTapTime;
  final GestureLongPressCallback? onLongPress;
  final GestureTapDownCallback? onSecondaryTapDown;
  final GestureTapUpCallback? onSecondaryTapUp;
  final GestureTapCallback? onSecondaryTapCancel;
  final GestureTapDownCallback? onTapDown;
  final GestureTapUpCallback? onTapUp;
  final GestureTapCancelCallback? onTapCancel;
  final GestureTapCallback? onSecondaryTap;
  final ValueChanged<bool>? onHighlightChanged;
  final ValueChanged<bool>? onHover;
  final MouseCursor? mouseCursor;
  final Color? focusColor;
  final Color? hoverColor;
  final Color? highlightColor;
  final WidgetStateProperty<Color?>? overlayColor;
  final Color? splashColor;
  final InteractiveInkFeatureFactory? splashFactory;
  final double? radius;
  final BorderRadius? borderRadius;
  final ShapeBorder? customBorder;
  final bool enableFeedback;
  final bool excludeFromSemantics;
  final FocusNode? focusNode;
  final bool canRequestFocus;
  final WidgetStatesController? statesController;
  final Duration? hoverDuration;
  final ValueChanged<bool>? onFocusChange;
  final bool autofocus;

  @override
  State<InkWellSplash> createState() => _InkWellSplashState();
}

class _InkWellSplashState extends State<InkWellSplash> {
  Timer? doubleTapTimer;
  bool isPressed = false;
  bool isSingleTap = false;
  bool isDoubleTap = false;

  void _doubleTapTimerElapsed() {
    if (isPressed) {
      isSingleTap = true;
    } else {
      if (widget.onTap != null) widget.onTap!();
    }
  }

  void _onTap() {
    isPressed = false;
    if (isSingleTap) {
      isSingleTap = false;
      if (widget.onTap != null) widget.onTap!(); // call user onTap function
    }
    if (isDoubleTap) {
      isDoubleTap = false;
      if (widget.onDoubleTap != null) widget.onDoubleTap!();
    }
  }

  void _onTapDown(TapDownDetails details) {
    isPressed = true;
    if (doubleTapTimer != null && doubleTapTimer!.isActive) {
      isDoubleTap = true;
      doubleTapTimer!.cancel();
    } else {
      doubleTapTimer = Timer(widget.doubleTapTime, _doubleTapTimerElapsed);
    }
    if (widget.onTapDown != null) widget.onTapDown!(details);
  }

  void _onTapCancel() {
    isPressed = isSingleTap = isDoubleTap = false;
    if (doubleTapTimer != null && doubleTapTimer!.isActive) {
      doubleTapTimer!.cancel();
    }
    if (widget.onTapCancel != null) widget.onTapCancel!();
  }

  @override
  Widget build(BuildContext context) {
    return InkWell(
      key: widget.key,
      onTap: (widget.onDoubleTap != null) ? _onTap : widget.onTap,
      // if onDoubleTap is not used from user, then route further to onTap
      onLongPress: widget.onLongPress,
      onSecondaryTapDown: widget.onSecondaryTapDown,
      onSecondaryTapUp: widget.onSecondaryTapUp,
      onSecondaryTapCancel: widget.onSecondaryTapCancel,
      onTapDown: (widget.onDoubleTap != null) ? _onTapDown : widget.onTapDown,
      onTapUp: widget.onTapUp,
      onTapCancel:
          (widget.onDoubleTap != null) ? _onTapCancel : widget.onTapCancel,
      onSecondaryTap: widget.onSecondaryTap,
      onHighlightChanged: widget.onHighlightChanged,
      onHover: widget.onHover,
      mouseCursor: widget.mouseCursor,
      focusColor: widget.focusColor,
      hoverColor: widget.hoverColor,
      highlightColor: widget.highlightColor,
      splashColor: widget.splashColor,
      splashFactory: widget.splashFactory,
      radius: widget.radius,
      borderRadius: widget.borderRadius,
      customBorder: widget.customBorder,
      enableFeedback: widget.enableFeedback,
      excludeFromSemantics: widget.excludeFromSemantics,
      focusNode: widget.focusNode,
      canRequestFocus: widget.canRequestFocus,
      onFocusChange: widget.onFocusChange,
      autofocus: widget.autofocus,
      child: widget.child,
    );
  }
}
