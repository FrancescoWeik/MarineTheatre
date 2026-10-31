using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

public enum DayType
{
    Sunny = 0,
    HalfMoon = 1,
    FullMoon = 2
}

public class DayManager : MonoBehaviour
{
    public static DayManager Instance { get; private set; }

    [SerializeField] private SpriteRenderer background;
    [SerializeField] private List<SpriteRenderer> seaBackgrounds = new List<SpriteRenderer>();

    // The day colors now come from the current biome (BiomeController > BiomeDayColors)
    [SerializeField] public float dayTransitionTime = 0.5f;
    [Tooltip("Duration of the color change when the biome changes")]
    [SerializeField] private float biomeColorTransitionTime = 1f;

    public UnityEvent<int> onDayChanged;

    public DayType currentDayType = DayType.Sunny;

    private Coroutine colorRoutine;

    private BiomeDayColors CurrentColors =>
        BiomeManager.Instance != null && BiomeManager.Instance.CurrentBiome != null ? BiomeManager.Instance.CurrentBiome.DayColors : null;
    private void Awake()
    {
        // If an instance already exists and it's not this one, destroy this one
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Set the instance to this object
        Instance = this;

        // Optional: Keep this object alive when switching scenes
        // DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (BiomeManager.Instance != null)
            BiomeManager.Instance.onBiomeChanged.AddListener(OnBiomeChanged);

        // Start with the colors of the start biome
        int day = (int)currentDayType;
        background.color = GetBackgroundColor(day);
        foreach (var sea in seaBackgrounds)
            sea.color = GetSpriteColor(day);
    }

    public void ChangeToSunny()
    {
        SetDay(DayType.Sunny);
    }

    public void ChangeToHalfMoon()
    {
        SetDay(DayType.HalfMoon);
    }

    public void ChangeToFoolMoon()
    {
        SetDay(DayType.FullMoon);
    }

    public Color GetBackgroundColor(int day)
    {
        BiomeDayColors colors = CurrentColors;
        if (colors == null || day >= colors.backgroundDayColors.Count)
        {
            Debug.LogWarning("[DayManager] The current biome has no BiomeDayColors (or not enough colors): background color unchanged.");
            return background.color;
        }
        return colors.backgroundDayColors[day];
    }

    public Color GetSpriteColor(int day)
    {
        BiomeDayColors colors = CurrentColors;
        if (colors == null || day >= colors.spriteDayColors.Count)
            return Color.white;
        return colors.spriteDayColors[day];
    }

    private void SetDay(DayType dayType)
    {
        currentDayType = dayType;
        onDayChanged.Invoke((int)dayType);
        LerpToCurrentColors(dayTransitionTime);
    }

    private void OnBiomeChanged(int biomeIndex)
    {
        // Every biome starts from the morning. BiomeManager.IsChanging is true here,
        // so listeners can tell this reset from a real day passing
        if (currentDayType != DayType.Sunny)
        {
            currentDayType = DayType.Sunny;
            onDayChanged.Invoke((int)DayType.Sunny);
        }
        LerpToCurrentColors(biomeColorTransitionTime);
    }

    private void LerpToCurrentColors(float duration)
    {
        // A new change replaces the running one, so two routines don't write the same colors
        if (colorRoutine != null)
            StopCoroutine(colorRoutine);

        int day = (int)currentDayType;
        colorRoutine = StartCoroutine(ChangeBackgroundColor(GetBackgroundColor(day), GetSpriteColor(day), duration));
    }

    private IEnumerator ChangeBackgroundColor(Color endColor, Color seaNewColor, float duration)
    {
        //Debug.LogError("Change Background routine");
        float elapsedTime = 0f;
        Color startColor = background.color;
        Color seaStartColor = Color.white;
        if (seaBackgrounds.Count!=0)
            seaStartColor = seaBackgrounds[0].color;

        while (elapsedTime < duration)
        {
            background.color = Color.Lerp(startColor, endColor, elapsedTime / duration);


            foreach(var sea in seaBackgrounds)
            {
                sea.color = Color.Lerp(seaStartColor, seaNewColor, elapsedTime / duration);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        background.color = endColor;
        foreach (var sea in seaBackgrounds)
            sea.color = seaNewColor;
        colorRoutine = null;
    }
}
