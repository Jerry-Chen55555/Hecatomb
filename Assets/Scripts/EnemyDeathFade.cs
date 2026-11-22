using UnityEngine;

public class EnemyDeathFade : MonoBehaviour
{
    public void Begin()
    {
        StartCoroutine(EnemyVisualEffects.FadeAndDestroy(gameObject));
    }
}
