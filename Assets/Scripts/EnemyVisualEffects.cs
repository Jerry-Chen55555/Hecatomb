using System.Collections;
using UnityEngine;

public static class EnemyVisualEffects
{
    public static IEnumerator WhiteFlash(SpriteRenderer[] renderers, float duration)
    {
        Color[] original = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                original[i] = renderers[i].color;
                // HDR tint brightens even sprites whose authored color is white.
                renderers[i].color = new Color(4f, 4f, 4f, original[i].a);
            }
        }
        yield return new WaitForSeconds(duration);
        for (int i = 0; i < renderers.Length; i++)
            if (renderers[i] != null) renderers[i].color = original[i];
    }

    public static IEnumerator FadeAndDestroy(GameObject enemy, float duration = 0.45f)
    {
        foreach (Behaviour behaviour in enemy.GetComponents<Behaviour>())
            if (!(behaviour is EnemyDeathFade)) behaviour.enabled = false;
        foreach (Collider2D collider in enemy.GetComponentsInChildren<Collider2D>()) collider.enabled = false;
        Rigidbody2D body = enemy.GetComponent<Rigidbody2D>();
        if (body != null) body.linearVelocity = Vector2.zero;
        SpriteRenderer[] renderers = enemy.GetComponentsInChildren<SpriteRenderer>();
        Color[] colors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++) colors[i] = renderers[i].color;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(elapsed / duration);
            for (int i = 0; i < renderers.Length; i++)
                if (renderers[i] != null) { Color c = colors[i]; c.a *= alpha; renderers[i].color = c; }
            yield return null;
        }
        Object.Destroy(enemy);
    }
}
