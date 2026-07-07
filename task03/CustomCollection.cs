using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CustomCollection<T> : IEnumerable<T>
{
    private readonly List<T> _items = new();

    public void Add(T item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        _items.Add(item);
    }

    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerable<T> GetReverseEnumerator()
    {
        for (int i = _items.Count - 1; i >= 0; i--)
            yield return _items[i];
    }

    public static IEnumerable<int> GenerateSequence(int start, int count)
    {
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
        for (int i = 0; i < count; i++)
            yield return start + i;
    }

    public IEnumerable<T> FilterAndSort(Func<T, bool> predicate, Func<T, IComparable> keySelector)
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
        return _items.Where(predicate).OrderBy(keySelector);
    }
}
