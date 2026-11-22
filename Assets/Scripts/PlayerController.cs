using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public EnemyEventChannel PlayerHitChannel;
    private EnemyEventListener _eventListener;
    public event System.Action<int> DamageTaken;
    public event System.Action<int> HealthHealed;

    public InputAction MoveAction;
    public Vector2 MoveInput;
    public float Speed = 10.0f;
    public int Health = 100;
    public int MaxHealth = 100;
    private Image _healthFill;
    private Animator animator;


    void Start()
    {
        _eventListener = GetComponent<EnemyEventListener>();
        _eventListener.Configure(null, PlayerHitChannel, null);
        animator = GetComponent<Animator>();
        Invoke("StopAnimator", 0.75f);

        MoveAction.Enable();
        Health = Mathf.Clamp(Health, 0, MaxHealth);
        SetupHealthBar();
        UpdateHealthBar();
    }

    void StopAnimator()
    {
        animator.enabled = false;
    }

    void Update()
    {
        Health = Mathf.Clamp(Health, 0, MaxHealth);
        UpdateHealthBar();
        MoveInput = MoveAction.ReadValue<Vector2>();
        transform.Translate(Vector3.right * MoveInput.x * Time.deltaTime * Speed);
        transform.Translate(Vector3.up * MoveInput.y * Time.deltaTime * Speed);
    }

    public void Heal(int amount)
    {
        int previousHealth = Health;
        Health = Mathf.Clamp(Health + Mathf.Max(0, amount), 0, MaxHealth);
        UpdateHealthBar();

        int healedAmount = Health - previousHealth;
        if (healedAmount > 0)
            HealthHealed?.Invoke(healedAmount);
    }

    public void TakeDamage(int amount)
    {
        int previousHealth = Health;
        Health = Mathf.Clamp(Health - Mathf.Max(0, amount), 0, MaxHealth);
        UpdateHealthBar();

        int damageAmount = previousHealth - Health;
        if (damageAmount > 0)
        {
            DamageTaken?.Invoke(damageAmount);
            if (Health <= 0)
                SfxPlayer.Instance?.PlayPlayerDeath();
            else
                SfxPlayer.Instance?.PlayPlayerHit();
        }
    }

    void SetupHealthBar()
    {
        foreach (Image image in FindObjectsByType<Image>(FindObjectsSortMode.None))
        {
            if (image.gameObject.name == "HealthRemaining")
            {
                _healthFill = image;
                break;
            }
        }

        if (_healthFill.sprite == null)
        {
            Texture2D white = Texture2D.whiteTexture;
            _healthFill.sprite = Sprite.Create(
                white,
                new Rect(0, 0, white.width, white.height),
                new Vector2(0.5f, 0.5f));
        }

        _healthFill.type = Image.Type.Filled;
        _healthFill.fillMethod = Image.FillMethod.Horizontal;
        _healthFill.fillOrigin = (int)Image.OriginHorizontal.Left;
    }

    void UpdateHealthBar()
    {
        _healthFill.fillAmount = MaxHealth > 0 ? (float)Health / MaxHealth : 0f;
    }
}
