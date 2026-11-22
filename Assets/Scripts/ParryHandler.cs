using UnityEngine;
using UnityEngine.InputSystem;

public class ParryHandler : MonoBehaviour
{
    public float TimeLastParried;
    public float ParryWindow = 0.2f;
    public InputAction ParryAction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ParryAction.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        if (ParryAction.WasPressedThisFrame())
        {
            TimeLastParried = Time.time;
        }
    }
}
