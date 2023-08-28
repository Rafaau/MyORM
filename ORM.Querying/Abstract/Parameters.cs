using System.Collections;

namespace ORM.Querying.Abstract;

public class Where : ExtendedDictionary
{
}

public class Order : ExtendedDictionary
{
}

public class ExtendedDictionary : IEnumerable<KeyValuePair<string, List<object>>>
{
private readonly Dictionary<string, List<object>> _internalDict = new();

public void Add(string key, params object[] values)
{
    _internalDict[key] = new List<object>(values);
}

public IEnumerable<object> this[string key] => _internalDict[key];

public IEnumerator<KeyValuePair<string, List<object>>> GetEnumerator()
{
    return _internalDict.GetEnumerator();
}

IEnumerator IEnumerable.GetEnumerator()
{
    return _internalDict.GetEnumerator();
}
}