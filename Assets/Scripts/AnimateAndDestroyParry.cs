using UnityEngine;
using System.Collections;

public class AnimateAndDestroyParry : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine("Animate");
        Invoke("DestroySelf", 0.3f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator Animate()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.075f);
            child.gameObject.SetActive(false);
        }
    }

    void DestroySelf()
    {
        Destroy(this.gameObject);
    }
}
