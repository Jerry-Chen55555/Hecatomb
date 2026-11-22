using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayButton : MonoBehaviour
{

    public GameObject Fade;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPressed()
    {
        StartCoroutine("StartSequence");
    }

    IEnumerator StartSequence()
    {
        yield return (StartCoroutine("FadeToBlack"));
        yield return new WaitForSeconds(1f); 
        this.gameObject.SetActive(false);
        SceneManager.LoadScene("JerryScene");
    }

    IEnumerator FadeToBlack()
    {
        Image image = Fade.GetComponent<Image>();
        for(int i = 0; i <= 255; i++)
        {
            Color color = image.color;
            color.a = i/255f;
            image.color = color;
            yield return new WaitForSeconds(1f/255f);
        }
    }
}
