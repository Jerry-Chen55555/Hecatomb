using UnityEngine;

public class LungeBehavior : MonoBehaviour, IDamageable, IEnemyPlayerHit
{
    public int CurrentHealth => Health;
    public float ApproachSpeed = 3f;
    public float AggroRange = 7f;
    public float Acceleration = 18f;

    public float RetreatSpeed = 2f;
    public float RetreatDuration = 0.7f;

    public float ChargeSpeed = 15f;
    public float ChargeDuration = 0.5f;
    public float ChargeAcceleration = 150f;
    public Collider2D ChargeHitbox;
    public int Health = 100;
    public EnemyEventChannel EnemyDamagedChannel;
    public EnemyEventChannel EnemyDamageRequestChannel;
    public EnemyEventChannel EnemyDiedChannel;
    public EnemyEventChannel PlayerHitChannel;
    public float ChargePathWidth = 1f;
    public float ChargePathAlpha = 0.3f;
    public float ChargePathLineWidth = 0.045f;

    public float RestDuration = 1f;
    public float Braking = 30f;

    public float TurnSmoothTime = 0.25f;
    public float MaxTurnSpeed = 360f;
    public float TurnTolerance = 3f;

    private Rigidbody2D _lungerRb;
    private GameObject _player;

    private enum LungeState
    {
        Approach,
        Retreat,
        Charge,
        Rest,
        Turn
    }

    private LungeState _currentState = LungeState.Approach;
    private float _stateTimer;
    private float _rotationVelocity;
    private Vector2 _chargeDirection;
    private Vector2 _chargeStartPosition;
    private bool _hasDamagedPlayerThisCharge;
    private Mesh _chargePathMesh;
    private MeshFilter _chargePathMeshFilter;
    private Material _chargePathMaterial;
    private SpriteRenderer[] _sprites;

    void Start()
    {
        EnemyEventListener eventListener = GetComponent<EnemyEventListener>();
        if (eventListener == null) eventListener = gameObject.AddComponent<EnemyEventListener>();
        eventListener.ConfigureAll(EnemyDamageRequestChannel, EnemyDamagedChannel, null, EnemyDiedChannel);
        _lungerRb = GetComponent<Rigidbody2D>();
        _sprites = GetComponentsInChildren<SpriteRenderer>();
        _player = GameObject.Find("Player");
        CreateChargePathIndicator();
        DetectHit.IgnoreEnemyCollisions(gameObject);

        ChargeHitbox.enabled = false;

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
        Vector2 direction = ((Vector2)_player.transform.position - _lungerRb.position).normalized;
        float distance = Vector2.Distance(_lungerRb.position, _player.transform.position);

        switch (_currentState)
        {
            case LungeState.Approach:
                MoveSmoothly(direction * ApproachSpeed, Acceleration);
                FaceDirection(direction);

                if (distance <= AggroRange)
                {
                    _currentState = LungeState.Retreat;
                    _stateTimer = RetreatDuration;
                    EnemyVisualEffects.FlashAggression(gameObject);
                }
                break;

            case LungeState.Retreat:
                MoveSmoothly(-direction * RetreatSpeed, Acceleration);
                FaceDirection(direction);
                _stateTimer -= Time.fixedDeltaTime;

                if (_stateTimer <= 0)
                {
                    _chargeDirection = direction;
                    _chargeStartPosition = _lungerRb.position;
                    _hasDamagedPlayerThisCharge = false;
                    _currentState = LungeState.Charge;
                    _stateTimer = ChargeDuration;
                    ChargeHitbox.enabled = true;
                }
                break;

            case LungeState.Charge:
                MoveSmoothly(_chargeDirection * ChargeSpeed, ChargeAcceleration);
                FaceDirection(_chargeDirection);
                _stateTimer -= Time.fixedDeltaTime;

                if (_stateTimer <= 0)
                {
                    _currentState = LungeState.Rest;
                    _stateTimer = RestDuration;
                    ChargeHitbox.enabled = false;
                }
                break;

            case LungeState.Rest:
                MoveSmoothly(Vector2.zero, Braking);
                _stateTimer -= Time.fixedDeltaTime;

                if (_stateTimer <= 0)
                {
                    _currentState = LungeState.Turn;
                }
                break;

            case LungeState.Turn:
                MoveSmoothly(Vector2.zero, Braking);
                bool finishedTurning = FaceDirection(direction);

                if (finishedTurning)
                {
                    _currentState = LungeState.Approach;
                }
                break;
        }

        UpdateChargePathIndicator(direction);
    }

    void CreateChargePathIndicator()
    {
        GameObject indicatorObject = new GameObject("Charge Path Indicator");
        indicatorObject.transform.SetParent(transform, false);

        _chargePathMeshFilter = indicatorObject.AddComponent<MeshFilter>();
        _chargePathMesh = new Mesh { name = "Charge Path Area" };
        _chargePathMesh.vertices = new Vector3[4];
        _chargePathMesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
        _chargePathMeshFilter.sharedMesh = _chargePathMesh;
        MeshRenderer indicatorRenderer = indicatorObject.AddComponent<MeshRenderer>();
        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            _chargePathMaterial = new Material(shader);
            indicatorRenderer.material = _chargePathMaterial;
        }

        Color color = new Color(1f, 0.25f, 0.15f, ChargePathAlpha);
        indicatorRenderer.sortingOrder = 5;
        if (_chargePathMaterial != null)
        {
            _chargePathMaterial.color = color;
        }
    }

    void UpdateChargePathIndicator(Vector2 directionToPlayer)
    {
        Vector2 pathOrigin;
        Vector2 pathDirection;

        if (_currentState == LungeState.Retreat)
        {
            pathOrigin = _lungerRb.position;
            pathDirection = directionToPlayer;
        }
        else if (_currentState == LungeState.Charge)
        {
            pathOrigin = _chargeStartPosition;
            pathDirection = _chargeDirection;
        }
        else
        {
            _chargePathMeshFilter.gameObject.SetActive(false);
            return;
        }

        if (pathDirection.sqrMagnitude < 0.0001f)
        {
            _chargePathMeshFilter.gameObject.SetActive(false);
            return;
        }

        pathDirection.Normalize();
        Vector2 side = new Vector2(-pathDirection.y, pathDirection.x) * (ChargePathWidth * 0.5f);
        Vector2 end = pathOrigin + pathDirection * (ChargeSpeed * ChargeDuration);
        float z = transform.position.z;

        _chargePathMeshFilter.gameObject.SetActive(true);
        _chargePathMesh.vertices = new Vector3[]
        {
            transform.InverseTransformPoint(new Vector3(pathOrigin.x - side.x, pathOrigin.y - side.y, z)),
            transform.InverseTransformPoint(new Vector3(pathOrigin.x + side.x, pathOrigin.y + side.y, z)),
            transform.InverseTransformPoint(new Vector3(end.x + side.x, end.y + side.y, z)),
            transform.InverseTransformPoint(new Vector3(end.x - side.x, end.y - side.y, z))
        };
        _chargePathMesh.RecalculateBounds();
    }

    void OnDestroy()
    {
        if (_chargePathMaterial != null)
        {
            Destroy(_chargePathMaterial);
        }
        if (_chargePathMesh != null)
            Destroy(_chargePathMesh);
    }

    public bool TryRegisterPlayerHit()
    {
        if (_currentState != LungeState.Charge || _hasDamagedPlayerThisCharge)
        {
            return false;
        }

        _hasDamagedPlayerThisCharge = true;
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

    void MoveSmoothly(Vector2 targetVelocity, float accelerationRate)
    {
        _lungerRb.linearVelocity = Vector2.MoveTowards(
            _lungerRb.linearVelocity,
            targetVelocity,
            accelerationRate * Time.fixedDeltaTime
        );
    }

    bool FaceDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.0001f)
        {
            return true;
        }

        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float newAngle = Mathf.SmoothDampAngle(
            _lungerRb.rotation,
            targetAngle,
            ref _rotationVelocity,
            TurnSmoothTime,
            MaxTurnSpeed,
            Time.fixedDeltaTime
        );

        _lungerRb.MoveRotation(newAngle);

        return Mathf.Abs(Mathf.DeltaAngle(newAngle, targetAngle)) <= TurnTolerance;
    }
}
