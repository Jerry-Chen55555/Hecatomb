using UnityEngine;
using UnityEngine.InputSystem;

public class FollowMouse : MonoBehaviour
{
    void Update()
    {
        Vector3 mousePos = new Vector3((Mouse.current.position.ReadValue().x - Screen.width/2)/500,
                                       (Mouse.current.position.ReadValue().y - Screen.height/2)/500,
                                       -10f);
        transform.localPosition = mousePos;
        // Debug.Log(Mouse.current.position.ReadValue());
    }
}
