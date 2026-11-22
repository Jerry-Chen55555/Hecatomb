using UnityEngine;

public class PunchingBehavior : MonoBehaviour
{
    public float MoveSpeed = 3f;
    public float Acceleration = 18f;
    public float AttackRange = 1.2f;
    public float PunchCooldown = 2f;
    public float PunchDuration = 0.2f;
    public float TurnSpeed = 360f;
    public Collider2D PunchHitbox;
    public int Health = 100;

    private Rigidbody2D _rigidbody;
    private GameObject _player;
    private float _punchCooldownTimer;
    private float _punchDurationTimer;
    private bool _isPunching;
    private bool _hitPlayerThisPunch;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _player = GameObject.Find("Player");
        DetectHit.IgnoreEnemyCollisions(gameObject);

        PunchHitbox.enabled = false;

        Collider2D[] enemyColliders = GetComponentsInChildren<Collider2D>();
        Collider2D[] playerColliders = _player.GetComponentsInChildren<Collider2D>();

        foreach (Collider2D enemyCollider in enemyColliders)
        {
            if (enemyCollider.isTrigger)
            {
                continue;
            }

            foreach (Collider2D playerCollider in playerColliders)
            {
                Physics2D.IgnoreCollision(enemyCollider, playerCollider);
            }
        }
    }

    void FixedUpdate()
    {
        float deltaTime = Time.fixedDeltaTime;
        Vector2 toPlayer = (Vector2)_player.transform.position - _rigidbody.position;
        float distance = toPlayer.magnitude;
        Vector2 direction = distance > 0.01f ? toPlayer.normalized : Vector2.right;
        Vector2 targetVelocity = distance > AttackRange ? direction * MoveSpeed : Vector2.zero;

        _rigidbody.linearVelocity = Vector2.MoveTowards(
            _rigidbody.linearVelocity,
            targetVelocity,
            Acceleration * deltaTime
        );

        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float angle = Mathf.MoveTowardsAngle(_rigidbody.rotation, targetAngle, TurnSpeed * deltaTime);
        _rigidbody.MoveRotation(angle);

        if (_isPunching)
        {
            _punchDurationTimer -= deltaTime;
            if (_punchDurationTimer <= 0f)
            {
                SetPunching(false);
            }
        }

        if (distance > AttackRange)
        {
            return;
        }

        _punchCooldownTimer -= deltaTime;
        if (!_isPunching && _punchCooldownTimer <= 0f)
        {
            BeginPunch();
        }
    }

    void BeginPunch()
    {
        _isPunching = true;
        _hitPlayerThisPunch = false;
        _punchDurationTimer = PunchDuration;
        _punchCooldownTimer = PunchCooldown;

        PunchHitbox.enabled = true;
    }

    void SetPunching(bool isPunching)
    {
        _isPunching = isPunching;
        if (!isPunching)
        {
            PunchHitbox.enabled = false;
        }
    }

    public bool TryRegisterPlayerHit()
    {
        if (!_isPunching || _hitPlayerThisPunch)
        {
            return false;
        }

        _hitPlayerThisPunch = true;
        return true;
    }

    public void TakeDamage(int damageAmount)
    {
        if (damageAmount <= 0 || Health <= 0)
        {
            return;
        }

        Health = Mathf.Max(0, Health - damageAmount);
        if (Health == 0)
        {
            Destroy(gameObject);
        }
    }
}
