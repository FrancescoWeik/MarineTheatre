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

    [SerializeField] public List<Color> bacgkroundDayColors = new List<Color>();
    [SerializeField] public List<Color> spriteDayColors = new List<Color>();


    [SerializeField] public float dayTransitionTime = 0.5f;

    public UnityEvent<int> onDayChanged;

    public DayType currentDayType = DayType.Sunny;
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

    public void ChangeToSunny()
    {
        currentDayType = DayType.Sunny;
        onDayChanged.Invoke(0);
        StartCoroutine(ChangeBackgroundColor(bacgkroundDayColors[0], spriteDayColors[0]));
    }

    public void ChangeToHalfMoon()
    {
        currentDayType = DayType.HalfMoon;
        onDayChanged.Invoke(1);
        StartCoroutine(ChangeBackgroundColor(bacgkroundDayColors[1], spriteDayColors[1]));
    }

    public void ChangeToFoolMoon()
    {
        currentDayType = DayType.FullMoon;
        onDayChanged.Invoke(2);
        StartCoroutine(ChangeBackgroundColor(bacgkroundDayColors[2], spriteDayColors[2]));
    }

    private IEnumerator ChangeBackgroundColor(Color endColor, Color seaNewColor)
    {
        //Debug.LogError("Change Background routine");
        float elapsedTime = 0f;
        Color startColor = background.color;
        Color seaStartColor = Color.white;
        if (seaBackgrounds.Count!=0)
            seaStartColor = seaBackgrounds[0].color;

        while (elapsedTime < dayTransitionTime)
        {
            background.color = Color.Lerp(startColor, endColor, elapsedTime / dayTransitionTime);


            foreach(var sea in seaBackgrounds)
            {
                sea.color = Color.Lerp(seaStartColor, seaNewColor, elapsedTime / dayTransitionTime);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        background.color = endColor;
    }
}
