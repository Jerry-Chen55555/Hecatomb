using UnityEngine;

public class AnimateAndDestroy : MonoBehaviour
{
    
    public GameObject Frame1;
    public GameObject Frame2;
    
    void Start()
    {
        Invoke("ChangeFrame", 0.17f);
        Invoke("DestroySelf", 0.25f);
    }

    void ChangeFrame()
    {
        Frame1.SetActive(false);
        Frame2.SetActive(true);
    }

    void DestroySelf()
    {
        Destroy(this.gameObject);
    }
}
