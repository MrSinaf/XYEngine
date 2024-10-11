using System.Collections;

namespace XYEngine.Essentials;

public class ListEvolute<T>(Action<T> onAddValue, Action<T> onRemoveValue) : IEnumerable<T> where T : class
{
    public readonly List<T> valuesAdded = [];
    public readonly List<T> valuesRemoved = [];
    public readonly List<T> values = [];

    public int length => values.Count + valuesAdded.Count - valuesRemoved.Count;
    public Action onUpdate;

    public void Add(T value) => valuesAdded.Add(value);
    public void Remove(T value) => valuesRemoved.Add(value);

    public void Update()
    {
        var tempValues = new List<T>();
        if (valuesAdded.Count > 0)
        {
            tempValues.AddRange(valuesAdded);
            valuesAdded.Clear();

            foreach (var tempValue in tempValues)
                onAddValue(tempValue);
        }

        if (tempValues.Count > 0)
            values.AddRange(tempValues);

        foreach (var value in valuesRemoved)
        {
            onRemoveValue(value);
            values.Remove(value);
        }

        valuesRemoved.Clear();

        onUpdate?.Invoke();
    }

    public bool Exist(T value)
    {
        foreach (var cValue in valuesAdded)
        {
            if (cValue == value)
                return true;
        }

        foreach (var cValue in values)
        {
            if (cValue == value)
                return true;
        }

        foreach (var cValue in valuesRemoved)
        {
            if (cValue == value)
                return true;
        }

        return false;
    }

    public void Clear()
    {
        valuesAdded.Clear();
        values.Clear();
        valuesRemoved.Clear();
    }

    public IEnumerator<T> GetEnumerator()
    {
        foreach (var value in values)
            yield return value;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}