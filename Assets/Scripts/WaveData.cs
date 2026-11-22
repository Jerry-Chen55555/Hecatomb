using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Waves/Wave Data", fileName = "WaveData")]
public class WaveData : ScriptableObject
{
    public List<WaveSubwave> subwaves = new List<WaveSubwave>();
}

[Serializable]
public class WaveSubwave
{
    public GameObject enemyPrefab;
    [Min(1)] public int enemyCount = 1;
    [Min(0f)] public float spawnInterval = 0.5f;
    [Min(0f)] public float delayAfter = 2f;
}
