using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarFlash : MonoBehaviour
{
    public PlayerController Player;
    public Image HealthBar;
    public SpriteRenderer TilingBackground;

    private void OnEnable()
    {
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
        StartCoroutine(FlashHealthBar(Color.red));
    }

    private void FlashHealthBarBlue(int amount)
    {
        StartCoroutine(FlashHealthBar(new Color(0f, 1f, 1f)));
        StartCoroutine(FlashTilingBackground());
    }

    private IEnumerator FlashHealthBar(Color flashColor)
    {
        Color originalColor = HealthBar.color;

        for (int frame = 0; frame <= 30; frame++)
        {
            float progress = frame / 30f;
            HealthBar.color = Color.Lerp(flashColor, originalColor, progress);
            yield return new WaitForSeconds(1f / 60f);
        }

        HealthBar.color = originalColor;
    }

    private IEnumerator FlashTilingBackground()
    {
        Color originalColor = TilingBackground.color;

        for (int frame = 0; frame <= 30; frame++)
        {
            float progress = frame / 30f;
            TilingBackground.color = Color.Lerp(new Color(0f, 1f, 1f), originalColor, progress);
            yield return new WaitForSeconds(1f / 60f);
        }
    }
}
