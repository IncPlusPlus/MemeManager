﻿using System;
using System.Threading;

namespace MemeManager.Extensions;

public static class EventExtensions
{
    public static void Raise<TEventArgs>(
        this EventHandler<TEventArgs>? eventHandler,
        object sender,
        TEventArgs args) where TEventArgs : EventArgs
    {
        var handler = Volatile.Read(ref eventHandler);

        handler?.Invoke(sender, args);
    }
}
