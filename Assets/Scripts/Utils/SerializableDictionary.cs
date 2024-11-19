using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class SerializableDictionary<TKey, TValue> : IEnumerable
{
    [SerializeField] private List<TKey> keys = new();

    [SerializeField] private List<TValue> values = new();

    private Dictionary<TKey, TValue> thisDictionary = new();

    public TValue this[TKey key]
    {
        get
        {
            if (thisDictionary.Count == 0)
            {
                OnAfterDeserialize();
            }

            return thisDictionary[key];
        }
        set
        {
            if (thisDictionary.Count == 0)
            {
                OnAfterDeserialize();
            }

            thisDictionary[key] = value;
        }
    }

    // New indexer that returns an object
    public object this[int index]
    {
        get
        {
            if (thisDictionary.Count == 0)
            {
                OnAfterDeserialize();
            }

            return new KeyValuePair<TKey, TValue>(keys[index], values[index]);
        }
    }

    // save the dictionary to lists
    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();
        foreach (var pair in thisDictionary)
        {
            keys.Add(pair.Key);
            values.Add(pair.Value);
        }
    }

    // load dictionary from lists
    public void OnAfterDeserialize()
    {
        thisDictionary.Clear();

        if (keys.Count != values.Count)
        {
            throw new Exception(string.Format(
                "there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable."));
        }

        for (var i = 0; i < keys.Count; i++)
        {
            if (thisDictionary.ContainsKey(keys[i]))
            {
                continue;
            }

            thisDictionary.Add(keys[i], values[i]);
        }
    }

    public IEnumerator GetEnumerator()
    {
        if (thisDictionary.Count == 0)
        {
            OnAfterDeserialize();
        }

        for (var i = 0; i < keys.Count; i++)
        {
            yield return this[i];
        }
    }

    public int Count => keys.Count;

    public bool TryToGetPair(Func<KeyValuePair<TKey, TValue>, bool> predicate, out KeyValuePair<TKey, TValue> result)
    {
        if (thisDictionary.Count == 0)
        {
            OnAfterDeserialize();
        }

        for (var i = 0; i < keys.Count; i++)
        {
            var kvp = new KeyValuePair<TKey, TValue>(keys[i], values[i]);
            if (predicate(kvp))
            {
                result = kvp;
                return true;
            }
        }

        result = default;
        return false;
    }


    public KeyValuePair<TKey, TValue> First(Func<KeyValuePair<TKey, TValue>, bool> predicate)
    {
        if (thisDictionary.Count == 0)
        {
            OnAfterDeserialize();
        }

        for (var i = 0; i < keys.Count; i++)
        {
            var kvp = new KeyValuePair<TKey, TValue>(keys[i], values[i]);
            if (predicate(kvp))
            {
                return kvp;
            }
        }

        throw new InvalidOperationException("No element satisfying the predicate was found.");
    }

    public bool Any(Func<KeyValuePair<TKey, TValue>, bool> predicate)
    {
        if (thisDictionary.Count == 0)
        {
            OnAfterDeserialize();
        }

        for (var i = 0; i < keys.Count; i++)
        {
            var kvp = new KeyValuePair<TKey, TValue>(keys[i], values[i]);
            if (predicate(kvp))
            {
                return true;
            }
        }

        return false;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        if (thisDictionary.Count == 0)
        {
            OnAfterDeserialize();
        }

        return thisDictionary.TryGetValue(key, out value);
    }

    public IReadOnlyCollection<TKey> GetKeys()
    {
        if (thisDictionary.Count == 0)
        {
            OnAfterDeserialize();
        }

        return keys.AsReadOnly();
    }

    public bool ContainsKey(TKey key)
    {
        if (thisDictionary.Count == 0)
        {
            OnAfterDeserialize();
        }

        return thisDictionary.ContainsKey(key);
    }

    public void Remove(TKey key)
    {
        if (thisDictionary.Count == 0)
        {
            OnAfterDeserialize();
        }

        thisDictionary.Remove(key);
    }
}