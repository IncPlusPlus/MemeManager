using System;
using System.Collections.Generic;
using System.Linq;

namespace MemeManager.Services.Abstractions;

public interface IDbChangeNotifier
{
    event EventHandler<DbChangeEventArgs> EntitiesUpdated;

    void NotifyOfChanges(IEnumerable<Type> relevantTypes);
}

public class DbChangeEventArgs : EventArgs
{
    private readonly IEnumerable<Type> _relevantTypes;

    public DbChangeEventArgs(IEnumerable<Type> relevantTypes)
    {
        _relevantTypes = relevantTypes;
    }

    public bool TypeRelevant(Type type)
    {
        return _relevantTypes.Contains(type);
    }
}

