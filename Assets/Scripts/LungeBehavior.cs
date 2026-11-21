using UnityEngine;

public class LungeBehavior : MonoBehaviour
{
    public float approachSpeed = 3f;
    public float aggroRange = 7f;
    public float acceleration = 18f;

    public float retreatSpeed = 2f;
    public float retreatDuration = 0.7f;

    public float chargeSpeed = 15f;
    public float chargeDuration = 0.5f;
    public float chargeAcceleration = 150f;

    public float restDuration = 1f;
    public float braking = 30f;

    public float turnSmoothTime = 0.25f;
    public float maxTurnSpeed = 360f;
    public float turnTolerance = 3f;

    private Rigidbody2D lungerRb;
    private GameObject player;

    private enum LungeState
    {
        Approach,
        Retreat,
        Charge,
        Rest,
        Turn
    }

    private LungeState currentState = LungeState.Approach;

    private float stateTimer;
    private float rotationVelocity;

    private Vector2 chargeDirection;

    void Start()
    {
        lungerRb = GetComponent<Rigidbody2D>();
        player = GameObject.Find("Player");
    }

    void FixedUpdate()
    {
        if (lungerRb == null) return;

        if (player == null)
        {
            MoveSmoothly(Vector2.zero, braking);
            return;
        }

        Vector2 direction =
            ((Vector2)player.transform.position - lungerRb.position).normalized;

        float distance = Vector2.Distance(
            lungerRb.position,
            player.transform.position
        );

        switch (currentState)
        {
            case LungeState.Approach:
                MoveSmoothly(direction * approachSpeed, acceleration);
                FaceDirection(direction);

                if (distance <= aggroRange)
                {
                    currentState = LungeState.Retreat;
                    stateTimer = retreatDuration;
                }

                break;

            case LungeState.Retreat:
                MoveSmoothly(-direction * retreatSpeed, acceleration);
                FaceDirection(direction);

                stateTimer -= Time.fixedDeltaTime;

                if (stateTimer <= 0)
                {
                    chargeDirection = direction;
                    currentState = LungeState.Charge;
                    stateTimer = chargeDuration;
                }

                break;

            case LungeState.Charge:
                MoveSmoothly(
                    chargeDirection * chargeSpeed,
                    chargeAcceleration
                );

                FaceDirection(chargeDirection);
                stateTimer -= Time.fixedDeltaTime;

                if (stateTimer <= 0)
                {
                    currentState = LungeState.Rest;
                    stateTimer = restDuration;
                }

                break;

            case LungeState.Rest:
                MoveSmoothly(Vector2.zero, braking);
                stateTimer -= Time.fixedDeltaTime;

                if (stateTimer <= 0)
                {
                    currentState = LungeState.Turn;
                }

                break;

            case LungeState.Turn:

                MoveSmoothly(Vector2.zero, braking);

                bool finishedTurning = FaceDirection(direction);

                if (finishedTurning)
                {
                    currentState = LungeState.Approach;
                }

                break;
        }
    }

    void MoveSmoothly(Vector2 targetVelocity, float accelerationRate)
    {
        lungerRb.linearVelocity = Vector2.MoveTowards(
            lungerRb.linearVelocity,
            targetVelocity,
            accelerationRate * Time.fixedDeltaTime
        );
    }

    bool FaceDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.0001f)
            return true;

        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        float newAngle = Mathf.SmoothDampAngle(
            lungerRb.rotation,
            targetAngle,
            ref rotationVelocity,
            turnSmoothTime,
            maxTurnSpeed,
            Time.fixedDeltaTime
        );

        lungerRb.MoveRotation(newAngle);

        return Mathf.Abs(Mathf.DeltaAngle(newAngle, targetAngle)) <= turnTolerance;
    }
}