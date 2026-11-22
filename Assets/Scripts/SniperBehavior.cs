using UnityEngine;

public class SniperBehavior : MonoBehaviour, IDamageable
{
    public float StayBack = 9f;
    public float StayBackTolerance = 2f;
    public float MoveSpeed = 3f;
    public float Acceleration = 6f;
    public float TurnSpeed = 360f;
    public bool FacePlayer = true;

    public int Damage = 60;
    public float FireRange = 20f;
    public float AcquireTime = 0.8f;
    public float ChargeTime = 3f;
    public float RedHoldTime = 0.15f;
    public float YellowFlashTime = 0.2f;
    public float FireFlashTime = 0.12f;
    public float ReloadTime = 2.5f;

    public float MinFlashRate = 1.5f;
    public float MaxFlashRate = 14f;
    public float StartWidth = 0.03f;
    public float EndWidth = 0.09f;

    public int ParryReflectDamage = 80;
    public float StunTime = 1.5f;

    public int Health = 80;
    public EnemyEventChannel EnemyDamagedChannel;
    public EnemyEventChannel EnemyDamageRequestChannel;
    public EnemyEventChannel EnemyDiedChannel;
    public EnemyEventChannel PlayerHitChannel;

    private enum SniperState 
    { 
        Idle, 
        Acquire, 
        Charge, 
        Fire, 
        Recover 
    }

    private SniperState _state = SniperState.Recover;
    private Transform _player;
    private PlayerController _playerController;
    private ParryHandler _parry;
    private LineRenderer _line;
    private Vector2 _velocity;
    private Vector3 _fireEnd;
    private Color _fireColor;
    private float _timer = 1f;
    private float _flashPhase;
    private float _nextRecoverTime;
    private float _nextBeepPhase;
    private bool _stunned;
    private bool _dead;

    void Start()
    {
        EnemyEventListener eventListener = GetComponent<EnemyEventListener>();
        if (eventListener == null) eventListener = gameObject.AddComponent<EnemyEventListener>();
        eventListener.ConfigureAll(EnemyDamageRequestChannel, EnemyDamagedChannel, null, EnemyDiedChannel);
        GameObject playerObject = GameObject.Find("Player");
        _player = playerObject.transform;
        _playerController = playerObject.GetComponent<PlayerController>();
        _parry = playerObject.GetComponent<ParryHandler>();

        _line = GetComponent<LineRenderer>();
        if (_line == null) _line = gameObject.AddComponent<LineRenderer>();
        _line.positionCount = 2;
        _line.useWorldSpace = true;
        _line.sortingOrder = 20;
        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null) _line.material = new Material(shader);
        _line.enabled = false;
    }

    void Update()
    {
        if (_dead || _player == null) return;

        float dt = Time.deltaTime;
        Vector2 position = transform.position;
        Vector2 toPlayer = (Vector2)_player.position - position;
        float distance = toPlayer.magnitude;
        Vector2 aim = distance > 0.001f ? toPlayer / distance : Vector2.right;

        Vector2 desired = Vector2.zero;

        switch (_state)
        {
            case SniperState.Idle:
            {
                desired = BandVelocity(distance, aim);
                if (distance <= FireRange)
                {
                    _state = SniperState.Acquire;
                    _timer = AcquireTime;
                    _flashPhase = 0f;
                    _line.enabled = true;
                }
                break;
            }

            case SniperState.Acquire:
            {
                if (distance > FireRange * 1.25f)
                {
                    Cancel(1f, false);
                    break;
                }

                _timer -= dt;
                float fadeIn = 1f - Mathf.Clamp01(_timer / Mathf.Max(AcquireTime, 0.01f));
                Color color = Color.white;
                color.a = Mathf.Lerp(0.15f, 0.6f, fadeIn);
                DrawLine(color, StartWidth, _player.position);

                if (_timer <= 0f)
                {
                    _state = SniperState.Charge;
                    _timer = ChargeTime;
                    _nextBeepPhase = Mathf.Floor(_flashPhase) + 0.25f;
                }
                break;
            }

            case SniperState.Charge:
            {
                _timer -= dt;
                UpdateChargeLine();

                if (_timer <= 0f)
                {
                    Fire();
                }
                break;
            }

            case SniperState.Fire:
            {
                _timer -= dt;
                float fade = Mathf.Clamp01(_timer / Mathf.Max(FireFlashTime, 0.01f));
                Color color = _fireColor;
                color.a = fade;
                DrawLine(color, EndWidth * 3f * fade, _fireEnd);

                if (_timer <= 0f)
                {
                    _line.enabled = false;
                    _state = SniperState.Recover;
                    _timer = _nextRecoverTime;
                }
                break;
            }

            case SniperState.Recover:
            {
                _timer -= dt;
                if (!_stunned) desired = BandVelocity(distance, aim);

                if (_timer <= 0f)
                {
                    _stunned = false;
                    _state = SniperState.Idle;
                }
                break;
            }
        }

        _velocity = Vector2.MoveTowards(_velocity, desired, Acceleration * dt);
        transform.position += (Vector3)(_velocity * dt);

        if (FacePlayer)
        {
            float targetAngle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, targetAngle, TurnSpeed * dt);
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    void UpdateChargeLine()
    {
        if (_timer <= YellowFlashTime)
        {
            DrawLine(Color.yellow, EndWidth * 1.6f, _player.position);
            return;
        }

        if (_timer <= YellowFlashTime + RedHoldTime)
        {
            DrawLine(Color.red, EndWidth * 1.3f, _player.position);
            return;
        }

        float flashSpan = Mathf.Max(ChargeTime - RedHoldTime - YellowFlashTime, 0.01f);
        float progress = Mathf.Clamp01((ChargeTime - _timer) / flashSpan);

        float previousFlashPhase = _flashPhase;
        _flashPhase += Mathf.Lerp(MinFlashRate, MaxFlashRate, progress * progress) * Time.deltaTime;
        while (previousFlashPhase < _nextBeepPhase && _flashPhase >= _nextBeepPhase)
        {
            SfxPlayer.Instance?.PlaySniperBeep();
            _nextBeepPhase += 1f;
        }
        float wave = 0.5f + 0.5f * Mathf.Sin(_flashPhase * 2f * Mathf.PI);
        float redAmount = Mathf.Clamp01(progress * 0.3f + wave * Mathf.Lerp(0.4f, 0.7f, progress));

        Color color = Color.Lerp(Color.white, Color.red, redAmount);
        color.a = Mathf.Lerp(0.6f, 1f, progress);
        DrawLine(color, Mathf.Lerp(StartWidth, EndWidth, progress), _player.position);
    }

    void Fire()
    {
        SfxPlayer.Instance?.PlaySniperShot();
        bool parried = _parry != null && _parry.isParrying;
        bool perfectParry = parried
            && Time.time - _parry.TimeLastParried >= 0f
            && Time.time - _parry.TimeLastParried <= _parry.ParryWindow;

        _fireEnd = _player.position;
        _fireColor = parried ? Color.cyan : Color.white;
        _nextRecoverTime = parried ? StunTime : ReloadTime;
        _stunned = parried;
        _state = SniperState.Fire;
        _timer = FireFlashTime;

        if (parried)
        {
            _parry.RegisterParrySuccess();
            if (perfectParry)
                _playerController.Heal(_parry.ParryHealAmount);
            TakeDamage(ParryReflectDamage);
        }
        else
        {
            if (PlayerHitChannel != null) PlayerHitChannel.Raise(_playerController.gameObject, Damage);
        }
    }

    void Cancel(float recoverTime, bool stunned)
    {
        _line.enabled = false;
        _stunned = stunned;
        _state = SniperState.Recover;
        _timer = recoverTime;
    }

    void DrawLine(Color color, float width, Vector3 end)
    {
        _line.startColor = color;
        _line.endColor = color;
        _line.startWidth = width;
        _line.endWidth = width;
        _line.SetPosition(0, transform.position);
        _line.SetPosition(1, end);
    }

    Vector2 BandVelocity(float distance, Vector2 aim)
    {
        float low = StayBack;
        float high = StayBack + StayBackTolerance;
        float push = 0f;

        if (distance < low) push = -Mathf.Clamp01((low - distance) / 1.5f);
        else if (distance > high) push = Mathf.Clamp01((distance - high) / 1.5f);

        return aim * (MoveSpeed * push);
    }

    public void TakeDamage(int amount)
    {
        if (_dead) return;

        Health -= amount;
        if (EnemyDamagedChannel != null) EnemyDamagedChannel.Raise(gameObject, amount);
        if (Health <= 0)
        {
            _dead = true;
            if (_line != null) _line.enabled = false;

            if (EnemyDiedChannel != null) EnemyDiedChannel.Raise(gameObject);
            return;
        }

        if (_state == SniperState.Acquire || _state == SniperState.Charge)
        {
            Cancel(StunTime, true);
        }
    }
}
