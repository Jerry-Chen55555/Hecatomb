using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSystem : MonoBehaviour
{
    public List<WaveData> waves = new List<WaveData>();
    public float timeBetweenWaves = 2f;
    public Vector2 spawnArea = new Vector2(8f, 4f);

    private IEnumerator Start()
    {
        for (int i = 0; i < waves.Count; i++)
        {
            yield return StartCoroutine(SpawnWave(waves[i]));
            while (transform.childCount > 0)
                yield return null;

            if (i < waves.Count - 1)
                yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    private IEnumerator SpawnWave(WaveData wave)
    {
        foreach (WaveSubwave subwave in wave.subwaves)
        {
            for (int i = 0; i < subwave.enemyCount; i++)
            {
                Vector2 offset = new Vector2(
                    Random.Range(-spawnArea.x, spawnArea.x),
                    Random.Range(-spawnArea.y, spawnArea.y));
                Instantiate(subwave.enemyPrefab, transform.position + (Vector3)offset,
                    Quaternion.identity, transform);

                if (i < subwave.enemyCount - 1 && subwave.spawnInterval > 0f)
                    yield return new WaitForSeconds(subwave.spawnInterval);
            }

            if (subwave.delayAfter > 0f)
                yield return new WaitForSeconds(subwave.delayAfter);
        }
    }
}
