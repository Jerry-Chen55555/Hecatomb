using UnityEngine;

public class MotorcycleBehavior : MonoBehaviour, IDamageable
{
    public int CurrentHealth => Health;
    public float Speed = 8f;
    public float TurnRate = 150f;
    public float Acceleration = 14f;

    public float DashSpeedMultiplier = 1.5f;
    public float CommitDistance = 3.5f;
    public float CommitAngle = 35f;
    public float CommitTime = 0.7f;

    public int Damage = 20;
    public float HitRadius = 0.8f;
    public float HitCooldown = 1f;
    public int Health = 120;
    public EnemyEventChannel EnemyDamagedChannel;
    public EnemyEventChannel EnemyDamageRequestChannel;
    public EnemyEventChannel EnemyDiedChannel;
    public EnemyEventChannel PlayerHitChannel;

    private Transform _player;
    private PlayerController _playerController;
    private ParryHandler _parry;
    private Vector2 _heading = Vector2.right;
    private float _currentSpeed;
    private float _commitTimer;
    private float _hitTimer;
    private bool _dead;
    private AudioSource _engineAudioSource;

    void Start()
    {
        EnemyEventListener eventListener = GetComponent<EnemyEventListener>();
        if (eventListener == null) eventListener = gameObject.AddComponent<EnemyEventListener>();
        eventListener.ConfigureAll(EnemyDamageRequestChannel, EnemyDamagedChannel, null, EnemyDiedChannel);
        GameObject playerObject = GameObject.Find("Player");
        _player = playerObject.transform;
        _playerController = playerObject.GetComponent<PlayerController>();
        _parry = playerObject.GetComponent<ParryHandler>();

        Vector2 toPlayer = (Vector2)_player.position - (Vector2)transform.position;
        if (toPlayer.sqrMagnitude > 0.0001f) _heading = toPlayer.normalized;

        _currentSpeed = Speed;
        _engineAudioSource = gameObject.AddComponent<AudioSource>();
        SfxPlayer.Instance?.StartBikeEngine(_engineAudioSource);
    }

    void Update()
    {
        if (_dead || _player == null) return;

        float dt = Time.deltaTime;
        Vector2 toPlayer = (Vector2)_player.position - (Vector2)transform.position;
        float distance = toPlayer.magnitude;
        Vector2 toPlayerDirection = distance > 0.001f ? toPlayer / distance : _heading;

        _hitTimer -= dt;

        if (_commitTimer > 0f)
        {
            _commitTimer -= dt;
        }
        else
        {
            float headingAngle = Mathf.Atan2(_heading.y, _heading.x) * Mathf.Rad2Deg;
            float targetAngle = Mathf.Atan2(toPlayerDirection.y, toPlayerDirection.x) * Mathf.Rad2Deg;
            float newAngle = Mathf.MoveTowardsAngle(headingAngle, targetAngle, TurnRate * dt);
            _heading = new Vector2(Mathf.Cos(newAngle * Mathf.Deg2Rad), Mathf.Sin(newAngle * Mathf.Deg2Rad));

            float offAngle = Mathf.Abs(Mathf.DeltaAngle(newAngle, targetAngle));
            if (distance <= CommitDistance && offAngle <= CommitAngle)
            {
                _commitTimer = CommitTime;
                SfxPlayer.Instance?.PlayBikeRev();
            }
        }

        float targetSpeed = _commitTimer > 0f ? Speed * DashSpeedMultiplier : Speed;
        _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, Acceleration * dt);

        transform.position += (Vector3)(_heading * _currentSpeed * dt);
        transform.right = _heading;

        if (_hitTimer <= 0f && distance <= HitRadius)
        {
            HitPlayer();
        }
    }

    void HitPlayer()
    {
        _hitTimer = HitCooldown;

        float parryElapsed = _parry == null ? float.PositiveInfinity : Time.time - _parry.TimeLastParried;
        bool perfectParry = _parry != null && _parry.isParrying
            && parryElapsed >= 0f
            && parryElapsed <= _parry.ParryWindow;
        bool parried = _parry != null && _parry.isParrying
            && parryElapsed >= 0f
            && parryElapsed <= _parry.BlockWindow;
        if (parried)
        {
            SfxPlayer.Instance?.PlayBikeParried();
            if (perfectParry)
                _parry.RegisterPerfectParrySuccess();
            else
                _parry.RegisterParrySuccess();
            if (perfectParry)
            {
                _parry.RegisterPerfectParry(gameObject);
                _playerController.Heal(_parry.ParryHealAmount);
            }
            else
            {
                _playerController.Heal(Mathf.CeilToInt(_parry.ParryHealAmount / 3f));
            }
            TakeDamage(perfectParry ? 999999 : Mathf.CeilToInt(Health * 0.5f));
            return;
        }

        SfxPlayer.Instance?.PlayBikeHit();
        if (PlayerHitChannel != null) PlayerHitChannel.Raise(_playerController.gameObject, Damage);
    }

    public void TakeDamage(int amount)
    {
        if (_dead) return;

        Health -= amount;
        if (EnemyDamagedChannel != null) EnemyDamagedChannel.Raise(gameObject, amount);
        EnemyVisualEffects.FlashOnHit(gameObject);
        if (Health > 0) return;

        _dead = true;
        SfxPlayer.Instance?.StopBikeEngine(_engineAudioSource);

        if (EnemyDiedChannel != null) EnemyDiedChannel.Raise(gameObject);
    }
}
