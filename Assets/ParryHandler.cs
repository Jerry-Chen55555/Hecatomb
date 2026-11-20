using UnityEngine;
using UnityEngine.InputSystem;

public class ParryHandler : MonoBehaviour
{

    public InputAction parryAction;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        parryAction.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        if (parryAction.WasPressedThisFrame())
        {
            print("Parried " + Time.time);
        }
    }
}
