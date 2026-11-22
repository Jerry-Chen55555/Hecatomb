using System;
using UnityEngine;

public sealed class EnemyEventChannel : ScriptableObject
{
    public event Action<GameObject, int> Raised;

    public void Raise(GameObject subject, int amount = 0)
    {
        if (subject != null)
            Raised?.Invoke(subject, amount);
    }
}
