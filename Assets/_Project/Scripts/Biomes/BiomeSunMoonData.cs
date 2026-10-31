using System.Collections.Generic;
using UnityEngine;

// Sun/moon look of a biome: one entry per DayType (0 = Sunny, 1 = HalfMoon, 2 = FullMoon)
[CreateAssetMenu(fileName = "BiomeSunMoonData", menuName = "ScriptableObjects/BiomeSunMoonData")]
public class BiomeSunMoonData : ScriptableObject
{
    [System.Serializable]
    public class DayEntry
    {
        public Sprite sprite;
        public FishGeneralDataScriptable bestiaryData;
    }

    public List<DayEntry> days = new List<DayEntry>();
}
