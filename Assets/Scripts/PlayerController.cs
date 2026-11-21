using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    public InputAction MoveAction;
    public Vector2 MoveInput;
    public float Speed = 10.0f;

    void Start()
    {
        MoveAction.Enable();
    }

    void Update()
    {

        MoveInput = MoveAction.ReadValue<Vector2>();
        transform.Translate(Vector3.right * MoveInput.x * Time.deltaTime * Speed);
        transform.Translate(Vector3.up * MoveInput.y * Time.deltaTime * Speed);
    }
}
