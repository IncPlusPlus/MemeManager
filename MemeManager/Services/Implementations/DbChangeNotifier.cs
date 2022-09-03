using System;
using System.Collections.Generic;
using MemeManager.Extensions;
using MemeManager.Services.Abstractions;

namespace MemeManager.Services.Implementations;

public class DbChangeNotifier : IDbChangeNotifier
{
    public event EventHandler<DbChangeEventArgs>? EntitiesUpdated;

    public void NotifyOfChanges(IEnumerable<Type> relevantTypes)
    {
        EntitiesUpdated.Raise(this, new DbChangeEventArgs(relevantTypes));
    }
}
