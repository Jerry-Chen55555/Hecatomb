using System.Collections;
using UnityEngine;

public class BackgroundIntroAnimation : MonoBehaviour
{
    public Transform triangleMask1;
    public Transform triangleMask2;
    public Transform triangleMask3;
    public Transform triangleMask4;

    public Transform arrow1;
    public Transform arrow2;
    public Transform arrow3;
    public Transform arrow4;

    public float duration = 0.85f;
    public float distance = 3f;


    private void Start()
    {
        StartCoroutine(PlayIntroAnimation());
        SfxPlayer.Instance?.PlayEntrance();
    }

    private IEnumerator PlayIntroAnimation()
    {
        Transform[] objects =
        {
            triangleMask1, triangleMask2, triangleMask3, triangleMask4,
            arrow1, arrow2, arrow3, arrow4
        };

        Vector3[] startPositions = new Vector3[objects.Length];
        Vector3[] directions = new Vector3[objects.Length];

        for (int i = 0; i < objects.Length; i++)
        {
            startPositions[i] = objects[i].position;
            directions[i] = objects[i].up;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                objects[i].position = startPositions[i]
                    + directions[i] * (distance * elapsed / duration);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < objects.Length; i++)
        {
            Destroy(objects[i].gameObject);
        }
    }
}
