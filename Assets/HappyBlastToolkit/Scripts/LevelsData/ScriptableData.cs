using System;
using UnityEngine;

public abstract class ScriptableData : ScriptableObject
{
    [IconPreview] public FillAndPreview prefab;

    public virtual void OnValidate()
    {
        OnChange?.Invoke();
    }

    public event Action OnChange;
}