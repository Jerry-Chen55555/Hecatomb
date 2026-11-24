using UnityEngine;

public class PunchingBehavior : MonoBehaviour, IDamageable, IEnemyPlayerHit
{
    public int CurrentHealth => Health;
    public float MoveSpeed = 3f;
    public float Acceleration = 18f;
    public float AttackRange = 1.2f;
    public float PunchCooldown = 2f;
    public float TelegraphDuration = 0.45f;
    public float PullbackSpeed = 3f;
    public float PunchDuration = 0.25f;
    public float PunchLungeSpeed = 14f;
    public float PunchAcceleration = 90f;
    public float HitWindow = 0.12f;
    public float RecoveryDuration = 0.4f;
    public float TurnSpeed = 360f;
    public Collider2D PunchHitbox;
    public int Health = 100;
    public EnemyEventChannel EnemyDamagedChannel;
    public EnemyEventChannel EnemyDamageRequestChannel;
    public EnemyEventChannel EnemyDiedChannel;
    public EnemyEventChannel PlayerHitChannel;

    private enum PunchPhase 
    { 
        Idle, 
        Windup, 
        Lunge, 
        Recovery
    }

    private Rigidbody2D _rigidbody;
    private GameObject _player;
    private SpriteRenderer[] _sprites;
    private PunchPhase _phase = PunchPhase.Idle;
    private float _phaseTimer;
    private float _punchCooldownTimer;
    private Vector2 _lungeDirection;
    private bool _hitPlayerThisPunch;

    void Start()
    {
        EnemyEventListener eventListener = GetComponent<EnemyEventListener>();
        if (eventListener == null) eventListener = gameObject.AddComponent<EnemyEventListener>();
        eventListener.ConfigureAll(EnemyDamageRequestChannel, EnemyDamagedChannel, null, EnemyDiedChannel);
        _rigidbody = GetComponent<Rigidbody2D>();
        _sprites = GetComponentsInChildren<SpriteRenderer>();
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
        float acceleration = Acceleration;
        Vector2 facing = direction;

        switch (_phase)
        {
            case PunchPhase.Windup:
            {
                _phaseTimer -= deltaTime;

                float coil = Mathf.Clamp01(_phaseTimer / TelegraphDuration);
                targetVelocity = -direction * PullbackSpeed * coil;
                acceleration = PunchAcceleration;

                if (_phaseTimer <= 0f)
                {
                    BeginLunge(direction);
                }
                break;
            }

            case PunchPhase.Lunge:
            {
                _phaseTimer -= deltaTime;

                float remaining = Mathf.Clamp01(_phaseTimer / PunchDuration);
                targetVelocity = _lungeDirection * PunchLungeSpeed * remaining;
                acceleration = PunchAcceleration;
                facing = _lungeDirection;

                if (!PunchHitbox.enabled && _phaseTimer <= HitWindow)
                {
                    PunchHitbox.enabled = true;
                }

                if (_phaseTimer <= 0f)
                {
                    BeginRecovery();
                }
                break;
            }

            case PunchPhase.Recovery:
            {
                _phaseTimer -= deltaTime;
                targetVelocity = Vector2.zero;
                acceleration = PunchAcceleration;

                if (_phaseTimer <= 0f)
                {
                    _phase = PunchPhase.Idle;
                }
                break;
            }
        }

        _rigidbody.linearVelocity = Vector2.MoveTowards(
            _rigidbody.linearVelocity,
            targetVelocity,
            acceleration * deltaTime
        );

        float targetAngle = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;
        float angle = Mathf.MoveTowardsAngle(_rigidbody.rotation, targetAngle, TurnSpeed * deltaTime);
        _rigidbody.MoveRotation(angle);

        if (_phase == PunchPhase.Windup || distance > AttackRange)
        {
            return;
        }

        _punchCooldownTimer -= deltaTime;
        if (_phase == PunchPhase.Idle && _punchCooldownTimer <= 0f)
        {
            BeginWindup();
        }
    }

    void BeginWindup()
    {
        _phase = PunchPhase.Windup;
        _phaseTimer = TelegraphDuration;
        EnemyVisualEffects.FlashAggression(gameObject);
    }

    void BeginLunge(Vector2 direction)
    {
        _phase = PunchPhase.Lunge;
        _phaseTimer = PunchDuration;
        _lungeDirection = direction;
        _hitPlayerThisPunch = false;
        _punchCooldownTimer = PunchCooldown;
        PunchHitbox.enabled = false;
    }

    void BeginRecovery()
    {
        _phase = PunchPhase.Recovery;
        _phaseTimer = RecoveryDuration;
        PunchHitbox.enabled = false;
    }

    public bool TryRegisterPlayerHit()
    {
        if (_phase != PunchPhase.Lunge || !PunchHitbox.enabled || _hitPlayerThisPunch)
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
        if (EnemyDamagedChannel != null) EnemyDamagedChannel.Raise(gameObject, damageAmount);
        EnemyVisualEffects.FlashOnHit(gameObject);
        if (Health == 0)
        {
            if (EnemyDiedChannel != null) EnemyDiedChannel.Raise(gameObject);
        }
    }
}
