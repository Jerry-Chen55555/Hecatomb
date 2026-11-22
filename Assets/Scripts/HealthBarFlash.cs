using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarFlash : MonoBehaviour
{
    public PlayerController Player;
    public Image HealthBar;
    public SpriteRenderer TilingBackground;
    private Coroutine _healthBarFlashCoroutine;
    private Coroutine _tilingBackgroundFlashCoroutine;
    private Color _healthBarOriginalColor;
    private Color _tilingBackgroundOriginalColor;

    private void OnEnable()
    {
        _healthBarOriginalColor = HealthBar.color;
        _tilingBackgroundOriginalColor = TilingBackground.color;
        Player.DamageTaken += FlashHealthBarRed;
        Player.HealthHealed += FlashHealthBarBlue;
    }

    private void OnDisable()
    {
        Player.DamageTaken -= FlashHealthBarRed;
        Player.HealthHealed -= FlashHealthBarBlue;
    }

    private void FlashHealthBarRed(int amount)
    {
        if (_healthBarFlashCoroutine != null)
            StopCoroutine(_healthBarFlashCoroutine);

        _healthBarFlashCoroutine = StartCoroutine(FlashHealthBar(Color.red));
    }

    private void FlashHealthBarBlue(int amount)
    {
        if (_healthBarFlashCoroutine != null)
            StopCoroutine(_healthBarFlashCoroutine);
        if (_tilingBackgroundFlashCoroutine != null)
            StopCoroutine(_tilingBackgroundFlashCoroutine);

        _healthBarFlashCoroutine = StartCoroutine(FlashHealthBar(new Color(0f, 1f, 1f)));
        _tilingBackgroundFlashCoroutine = StartCoroutine(FlashTilingBackground());
    }

    private IEnumerator FlashHealthBar(Color flashColor)
    {
        for (int frame = 0; frame <= 30; frame++)
        {
            float progress = frame / 30f;
            HealthBar.color = Color.Lerp(flashColor, _healthBarOriginalColor, progress);
            yield return new WaitForSeconds(1f / 60f);
        }

        HealthBar.color = _healthBarOriginalColor;
        _healthBarFlashCoroutine = null;
    }

    private IEnumerator FlashTilingBackground()
    {
        for (int frame = 0; frame <= 30; frame++)
        {
            float progress = frame / 30f;
            TilingBackground.color = Color.Lerp(new Color(0f, 1f, 1f), _tilingBackgroundOriginalColor, progress);
            yield return new WaitForSeconds(1f / 60f);
        }

        TilingBackground.color = _tilingBackgroundOriginalColor;
        _tilingBackgroundFlashCoroutine = null;
    }
}
