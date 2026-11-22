using UnityEngine;

public class MotorcycleBehavior : MonoBehaviour
{
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

    private Transform _player;
    private PlayerController _playerController;
    private ParryHandler _parry;
    private Vector2 _heading = Vector2.right;
    private float _currentSpeed;
    private float _commitTimer;
    private float _hitTimer;
    private bool _dead;

    void Start()
    {
        GameObject playerObject = GameObject.Find("Player");
        _player = playerObject.transform;
        _playerController = playerObject.GetComponent<PlayerController>();
        _parry = playerObject.GetComponent<ParryHandler>();

        Vector2 toPlayer = (Vector2)_player.position - (Vector2)transform.position;
        if (toPlayer.sqrMagnitude > 0.0001f) _heading = toPlayer.normalized;

        _currentSpeed = Speed;
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

        bool parried = _parry != null && _parry.isParrying;
        bool perfectParry = parried
            && Time.time - _parry.TimeLastParried >= 0f
            && Time.time - _parry.TimeLastParried <= _parry.ParryWindow;
        if (parried)
        {
            _parry.RegisterParrySuccess();
            if (perfectParry)
            {
                _parry.RegisterPerfectParry(gameObject);
                _playerController.Heal(_parry.ParryHealAmount);
            }
            TakeDamage(999);
            return;
        }

        _playerController.TakeDamage(Damage);
    }

    public void TakeDamage(int amount)
    {
        if (_dead) return;

        Health -= amount;
        EnemyVisualEffects.FlashOnHit(gameObject);
        if (Health > 0) return;

        _dead = true;

        EnemyDeathFade fade = GetComponent<EnemyDeathFade>();
        fade = gameObject.AddComponent<EnemyDeathFade>();
        fade.Begin();
    }
}
