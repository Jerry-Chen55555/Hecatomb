using UnityEngine;

public class AnimateAndDestroy : MonoBehaviour
{
    
    public GameObject Frame1;
    public GameObject Frame2;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Invoke("ChangeFrame", 0.17f);
        Invoke("DestroySelf", 0.25f);
    }

    // Update is called once per frame
    void Update()
    {
        
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
