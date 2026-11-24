using UnityEngine;

public delegate void EnemyEventRaised(GameObject subject, int amount);

public sealed class EnemyEventChannel : ScriptableObject
{
    public event EnemyEventRaised Raised;

    public void Raise(GameObject subject, int amount = 0)
    {
        if (subject != null)
            Raised?.Invoke(subject, amount);
    }
}
