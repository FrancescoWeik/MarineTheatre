using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FishSpawnerDay", menuName = "ScriptableObjects/FishSpawnerDayData")]
public class FishSpawnerDayStructure : ScriptableObject
{
    public List<GameObject> highChanceFishSpawn = new List<GameObject>();
    public List<GameObject> mediumChanceFishSpawn = new List<GameObject>();
    public List<GameObject> lowChanceFishSpawn = new List<GameObject>();

    public float highChancePercentage = 70f;
    public float mediumChancePercentage = 20f;
    public float lowChancePercentage = 10f;
}
