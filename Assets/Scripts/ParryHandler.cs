using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class ParryHandler : MonoBehaviour
{
    public float TimeLastParried;
    public InputAction ParryAction;
    public GameObject Shield;
    public bool isParrying = false;
    public bool parrySuccess = false;
    public float ParryCooldown = 0.5f;
    private float _nextParryTime = 0f;
    private Coroutine _animateCoroutine;

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
            TimeLastParried = Time.time;
            Shield.SetActive(true);
            isParrying = true;
            _nextParryTime = Time.time + ParryCooldown;

            _animateCoroutine = StartCoroutine("AnimateShield");
        }
        if (ParryAction.WasReleasedThisFrame())
        {
            Shield.SetActive(false);
            isParrying = false;

            StopCoroutine(_animateCoroutine);
        }
    }

    IEnumerator AnimateShield()
    {
        SpriteRenderer spriteRenderer = Shield.GetComponent<SpriteRenderer>();
        for (int i = 0; i <= 60; i++)
        {
            Vector3 scale = Vector3.one * (2 - 2*Mathf.Pow(i / 60f, 2));
            Shield.transform.localScale = scale;
            if (i < 20)
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
