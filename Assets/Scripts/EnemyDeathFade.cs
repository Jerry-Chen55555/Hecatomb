using UnityEngine;

public class EnemyDeathFade : MonoBehaviour, IEnemyDeath
{
    private bool _begun;

    public void Begin()
    {
        if (_begun) return;
        _begun = true;
        StartCoroutine(EnemyVisualEffects.FadeAndDestroy(gameObject));
    }

    public void Die() => Begin();
}
