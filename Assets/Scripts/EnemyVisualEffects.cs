using System.Collections;
using UnityEngine;

public static class EnemyVisualEffects
{
    public static void FlashOnHit(GameObject enemy)
    {
        Flash(enemy, new Color(4f, 4f, 4f, 1f));
    }

    public static void FlashAggression(GameObject enemy)
    {
        Flash(enemy, Color.red);
    }

    private static void Flash(GameObject enemy, Color flashColor)
    {
        EnemyHitFlash flash = enemy.GetComponent<EnemyHitFlash>();
        if (flash == null) flash = enemy.AddComponent<EnemyHitFlash>();
        flash.Play(flashColor);
    }

    public static IEnumerator FadeAndDestroy(GameObject enemy, float duration = 0.45f)
    {
        foreach (Behaviour behaviour in enemy.GetComponents<Behaviour>())
            if (!(behaviour is EnemyDeathFade)) behaviour.enabled = false;
        foreach (Collider2D collider in enemy.GetComponentsInChildren<Collider2D>()) collider.enabled = false;
        Rigidbody2D body = enemy.GetComponent<Rigidbody2D>();
        GameObject player = GameObject.Find("Player");
        Vector2 away = player == null ? Random.insideUnitCircle.normalized : ((Vector2)enemy.transform.position - (Vector2)player.transform.position).normalized;
        float recoilSpeed = Random.Range(8f, 14f);
        if (body != null) body.linearVelocity = away * recoilSpeed;
        SpriteRenderer[] renderers = enemy.GetComponentsInChildren<SpriteRenderer>();
        Color[] colors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++) colors[i] = renderers[i].color;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (body == null) enemy.transform.position += (Vector3)(away * recoilSpeed * Time.deltaTime);
            float alpha = 1f - Mathf.Clamp01(elapsed / duration);
            for (int i = 0; i < renderers.Length; i++)
                if (renderers[i] != null) { Color c = colors[i]; c.a *= alpha; renderers[i].color = c; }
            yield return null;
        }
        Object.Destroy(enemy);
    }
}

public class EnemyHitFlash : MonoBehaviour
{
    private SpriteRenderer[] _renderers;
    private Color[] _baseColors;
    private Coroutine _flash;

    public void Play(Color flashColor)
    {
        if (_flash != null)
        {
            StopCoroutine(_flash);
            for (int i = 0; i < _renderers.Length; i++)
                if (_renderers[i] != null) _renderers[i].color = _baseColors[i];
        }
        else
        {
            _renderers = GetComponentsInChildren<SpriteRenderer>();
            _baseColors = new Color[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++) _baseColors[i] = _renderers[i].color;
        }
        _flash = StartCoroutine(FadeFlash(flashColor));
    }

    private IEnumerator FadeFlash(Color flashColor)
    {
        const float duration = 0.18f;
        for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
        {
            float amount = elapsed < duration * 0.35f ? elapsed / (duration * 0.35f) : 1f - (elapsed - duration * 0.35f) / (duration * 0.65f);
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] == null) continue;
                Color target = flashColor;
                target.a = _baseColors[i].a;
                _renderers[i].color = Color.Lerp(_baseColors[i], target, amount);
            }
            yield return null;
        }
        for (int i = 0; i < _renderers.Length; i++)
            if (_renderers[i] != null) _renderers[i].color = _baseColors[i];
        _flash = null;
    }
}
