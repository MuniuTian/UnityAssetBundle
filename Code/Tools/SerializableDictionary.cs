using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    public void OnBeforeSerialize()
    {
        _keys.Clear();
        _values.Clear();
        _keys.Capacity = Count;
        _values.Capacity = Count;

        foreach (var kvp in this)
        {
            _keys.Add(kvp.Key);
            _values.Add(kvp.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        Clear();
        var count = Mathf.Min(_keys.Count, _values.Count);
        for (var i = 0; i < count; ++i)
        {
            Add(_keys[i], _values[i]);
        }
    }

    [SerializeField]
    private List<TKey> _keys = new List<TKey>();
    [SerializeField]
    private List<TValue> _values = new List<TValue>();
}