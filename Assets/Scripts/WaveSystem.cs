using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WaveSystem : MonoBehaviour
{
    public List<WaveData> waves = new List<WaveData>();
    public TMP_Text waveCounter;
    public float timeBetweenWaves = 2f;
    public Vector2 spawnArea = new Vector2(8f, 4f);

    private IEnumerator Start()
    {
        for (int i = 0; i < waves.Count; i++)
        {
            if (waveCounter != null)
                waveCounter.text = $"Wave {i + 1}";

            if (waves[i] == null)
                continue;

            yield return StartCoroutine(SpawnWave(waves[i]));
            while (transform.childCount > 0)
                yield return null;

            if (i < waves.Count - 1)
                yield return new WaitForSeconds(timeBetweenWaves);
        }

        if (waveCounter != null)
            waveCounter.text = $"Wave {waves.Count} Complete";

        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
            player.WinGame();
    }

    private IEnumerator SpawnWave(WaveData wave)
    {
        if (wave.subwaves == null)
            yield break;

        foreach (WaveSubwave subwave in wave.subwaves)
        {
            if (subwave == null || subwave.enemyPrefab == null)
                continue;

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
