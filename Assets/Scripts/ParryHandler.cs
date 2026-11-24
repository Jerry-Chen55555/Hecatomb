using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class ParryHandler : MonoBehaviour
{
    public float TimeLastParried;
    public float ParryWindow = 0.15f;
    public float BlockWindow = 0.35f;
    public InputAction ParryAction;
    public GameObject Shield;
    public GameObject PerfectParryVFX;
    public bool isParrying = false;
    public bool blockSuccess = false;
    public bool parrySuccess = false;
    public float ParryCooldown = 0.4f;
    public int ParryHealAmount = 20;

    public float ParryOffset;
    private float _nextParryTime = 0f;
    private Coroutine _animateCoroutine;
    private Coroutine _shakeCoroutine;
    private Coroutine _pauseCoroutine;
    private float _timeScaleBeforeParryPause;

    public void AllowQuickParry()
    {
        _nextParryTime = Time.time;
    }

    public void RegisterParrySuccess()
    {
        if (!isParrying)
        {
            return;
        }

        blockSuccess = true;
        SfxPlayer.Instance?.PlayBlock();
        ShakeCamera();
        StartParry();
    }

    public void RegisterPerfectParrySuccess()
    {
        if (!isParrying)
        {
            return;
        }

        parrySuccess = true;
        SfxPlayer.Instance?.PlayParry();
        ShakeCamera();
        StartParry();
    }

    public void RegisterPerfectParry(Transform enemy)
    {
        Vector2 direction = (Vector2)enemy.position - (Vector2)transform.position;
        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = -(Vector2)enemy.right;
        }
        RegisterPerfectParry(direction);
    }

    public void RegisterPerfectParry(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = Vector2.right;
        }
        else
        {
            direction.Normalize();
        }

        Vector3 position = transform.position + (Vector3)(direction * ParryOffset);
        Instantiate(PerfectParryVFX, position, Quaternion.identity);

        if (_pauseCoroutine == null)
        {
            if (Time.timeScale <= 0f)
                return;

            _timeScaleBeforeParryPause = Time.timeScale;
            Time.timeScale = 0f;
            _pauseCoroutine = StartCoroutine(BriefPause());
        }
        else
        {
            StopCoroutine(_pauseCoroutine);
            _pauseCoroutine = StartCoroutine(BriefPause());
        }
    }

    private IEnumerator BriefPause()
    {
        yield return new WaitForSecondsRealtime(0.05f);
        Time.timeScale = _timeScaleBeforeParryPause;
        _pauseCoroutine = null;
    }

    private void ShakeCamera()
    {
        Camera camera = Camera.main;
        if (camera == null) return;
        if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);
        _shakeCoroutine = StartCoroutine(CameraShake(camera.transform));
    }

    private IEnumerator CameraShake(Transform cameraTransform)
    {
        Vector3 origin = cameraTransform.localPosition;
        const float duration = 0.18f;
        const float magnitude = 0.12f;
        for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
        {
            cameraTransform.localPosition = origin + (Vector3)(Random.insideUnitCircle * magnitude);
            yield return null;
        }
        cameraTransform.localPosition = origin;
        _shakeCoroutine = null;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ParryAction.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        if (ParryAction.WasPressedThisFrame()  && Time.time >= _nextParryTime)
        {
            StartParry();
        }
        if (ParryAction.WasReleasedThisFrame())
        {
            Shield.SetActive(false);
            isParrying = false;

            if (blockSuccess || parrySuccess)
            {
                _nextParryTime = Time.time;
            }
            blockSuccess = false;
            parrySuccess = false;

            if (_animateCoroutine != null)
            {
                StopCoroutine(_animateCoroutine);
                _animateCoroutine = null;
            }
        }
    }

    public void StartParry()
    {
        if (_animateCoroutine != null)
        {
            StopCoroutine(_animateCoroutine);
        }
        TimeLastParried = Time.time;
        Shield.SetActive(true);
        Shield.transform.localScale = Vector3.one * 2f;
        Shield.GetComponent<SpriteRenderer>().color = new Color(0.2f, 1f, 1f, 0.5f);
        isParrying = true;
        if (!blockSuccess && !parrySuccess)
        {
            _nextParryTime = Time.time + ParryCooldown;
        }

        _animateCoroutine = StartCoroutine("AnimateShield");
    }

    IEnumerator AnimateShield()
    {
        SpriteRenderer spriteRenderer = Shield.GetComponent<SpriteRenderer>();
        for (int i = 0; i <= 60; i++)
        {
            Vector3 scale = Vector3.one * (2 - 2*Mathf.Pow(i / 60f, 2));
            Shield.transform.localScale = scale;
            if (i < 20 && !blockSuccess)
            {
                spriteRenderer.color = new Color(0.2f, 1f, 1f, 0.5f);
            }
            else
            {
                spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);
            }

            yield return new WaitForSeconds(1/120f);
        }
    }
}
