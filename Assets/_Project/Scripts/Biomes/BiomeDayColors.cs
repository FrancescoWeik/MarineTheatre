using System.Collections.Generic;
using UnityEngine;

// Color palette of a biome, one color per DayType (0 = Sunny, 1 = HalfMoon, 2 = FullMoon)
[CreateAssetMenu(fileName = "BiomeDayColors", menuName = "ScriptableObjects/BiomeDayColors")]
public class BiomeDayColors : ScriptableObject
{
    public List<Color> backgroundDayColors = new List<Color>();
    public List<Color> spriteDayColors = new List<Color>();
}
