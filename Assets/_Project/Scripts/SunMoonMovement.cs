using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// Shared between biomes: sprites and bestiary data come from the current biome (BiomeController > BiomeSunMoonData).
// The day state lives in DayManager, this only shows it.
public class SunMoonMovement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private float sunSpeed = 0.5f;
    private float halFmoonSpeed = 0.3f;
    private float fullMoonSpeed = 0.1f;

    private int currentActive = 0;

    [SerializeField] private Collider2D coll;
    [SerializeField] private Transform sunMoonContainer;
    // One per DayType (0 = Sunny, 1 = HalfMoon, 2 = FullMoon)
    [SerializeField] private List<GameObject> sunMoonObjects = new List<GameObject>();


    [SerializeField] private float yUpTarget = 11f;
    [SerializeField] private float yUpTime = 0.2f;

    [SerializeField] private float yDownTarget = 0f;
    [SerializeField] private float yDownTime = 0.3f;

    private Animator animator;
    private bool hovered = false;
    private Vector3 startPosition = Vector3.zero;

    private bool isAnimating = false;
    private bool biomeSwapPending = false;

    private BiomeSunMoonData CurrentData =>
        BiomeManager.Instance != null && BiomeManager.Instance.CurrentBiome != null ? BiomeManager.Instance.CurrentBiome.SunMoonData : null;

    private void OnEnable()
    {
        SubscribeToBiomeManager();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPosition = transform.localPosition;
        animator = GetComponent<Animator>();

        // Fallback: on scene load BiomeManager.Awake may run after this OnEnable
        SubscribeToBiomeManager();
        ApplyCurrentBiome();
    }

    private void OnDisable()
    {
        if (BiomeManager.Instance != null)
            BiomeManager.Instance.onBiomeChanged.RemoveListener(OnBiomeChanged);

        // Disabling kills the animation coroutine: reset its state or clicks would stay blocked forever
        if (isAnimating)
        {
            StopAllCoroutines();
            isAnimating = false;
            transform.localPosition = startPosition;
            animator.enabled = true;
            coll.enabled = true;
            if (biomeSwapPending)
                ApplyCurrentBiome();
        }
    }

    private void SubscribeToBiomeManager()
    {
        if (BiomeManager.Instance == null)
            return;

        // Remove first so calling this from both OnEnable and Start doesn't register it twice
        BiomeManager.Instance.onBiomeChanged.RemoveListener(OnBiomeChanged);
        BiomeManager.Instance.onBiomeChanged.AddListener(OnBiomeChanged);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isAnimating || (BiomeManager.Instance != null && BiomeManager.Instance.IsChanging))
            return;

        FishGeneralDataScriptable bestiaryData = GetBestiaryData(currentActive);
        if (bestiaryData != null)
        {
            bool wasClicked = SaveSystem.GetItemClicked(bestiaryData.title);
            if (!wasClicked)
            {
                SaveSystem.SetItemClicked(bestiaryData.title, true);

            }
        }

        //Debug.LogError("Pointer click");
        ChangeDay();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //Debug.LogError("Pointer enter");
        //sunMoonContainer.localScale = hoverScale;
        hovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //Debug.LogError("Pointer exit");
        //sunMoonContainer.localScale = Vector3.one;

        hovered = false;
    }

    public void ChangeDay()
    {
        StartCoroutine(DownAndUpAnimation(() =>
        {
            // A biome change arrived during the way down: it wins over the day change (every biome starts from the morning)
            if (biomeSwapPending)
                ApplyCurrentBiome();
            else
                NextDay();
        }));
    }

    private void OnBiomeChanged(int biomeIndex)
    {
        // Even if it's already morning it goes down and up: the new biome has different sprites
        biomeSwapPending = true;
        if (!isAnimating)
            StartCoroutine(DownAndUpAnimation(ApplyCurrentBiome));
    }

    private void NextDay()
    {
        int newDay = currentActive + 1;
        if (newDay >= sunMoonObjects.Count)
            newDay = 0;
        ShowDay(newDay);

        switch (newDay)
        {
            case 0:
                DayManager.Instance.ChangeToSunny();
                break;
            case 1:
                DayManager.Instance.ChangeToHalfMoon();
                break;
            case 2:
                DayManager.Instance.ChangeToFoolMoon();
                break;
            default:
                break;
        }
    }

    // Takes the sprites of the current biome and shows the current day (DayManager already reset it to the morning)
    private void ApplyCurrentBiome()
    {
        biomeSwapPending = false;

        BiomeSunMoonData data = CurrentData;
        if (data == null)
            Debug.LogWarning("[SunMoonMovement] The current biome has no BiomeSunMoonData: sprites unchanged.");

        for (int i = 0; i < sunMoonObjects.Count; i++)
        {
            if (data == null || i >= data.days.Count || data.days[i].sprite == null)
                continue;
            SpriteRenderer spriteRenderer = sunMoonObjects[i].GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
                spriteRenderer.sprite = data.days[i].sprite;
        }

        int day = DayManager.Instance != null ? (int)DayManager.Instance.currentDayType : 0;
        ShowDay(day);
    }

    private void ShowDay(int day)
    {
        for (int i = 0; i < sunMoonObjects.Count; i++)
            sunMoonObjects[i].SetActive(i == day);
        currentActive = day;
    }

    private FishGeneralDataScriptable GetBestiaryData(int day)
    {
        BiomeSunMoonData data = CurrentData;
        if (data == null || day >= data.days.Count)
            return null;
        return data.days[day].bestiaryData;
    }

    // Goes up, falls down out of the screen, runs atBottom, comes back to the start position
    private IEnumerator DownAndUpAnimation(Action atBottom)
    {
        isAnimating = true;
        animator.enabled = false;
        coll.enabled = false;

        float elapsed = 0f;
        Vector3 upPosition = new Vector3(startPosition.x, yUpTarget, startPosition.z);
        Vector3 downPosition = new Vector3(startPosition.x, yDownTarget, startPosition.z);
        Vector3 startAnimPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);

        //Place in start Position (TODO, based on the current speed and movement, place in the startPosition)


        //Go Up
        while (elapsed < yUpTime)
        {
            float t = elapsed / yUpTime;
            float easedT = Mathf.Sin(t * Mathf.PI * 0.5f);
            transform.localPosition = Vector3.Lerp(startAnimPosition, upPosition, easedT);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = upPosition;

        //Go down
        elapsed = 0f;
        while (elapsed < yDownTime)
        {
            float t = elapsed / yDownTime;
            // Ease In Formula: starts slow, ends fast
            float easedT = t * t;

            transform.localPosition = Vector3.Lerp(upPosition, downPosition, easedT);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = downPosition;

        //Change day element (or biome sprites)
        atBottom();

        //Go up
        elapsed = 0f;
        while (elapsed < yDownTime)
        {
            float t = elapsed / yDownTime;
            float easedT = Mathf.Sin(t * Mathf.PI * 0.5f);
            transform.localPosition = Vector3.Lerp(downPosition, upPosition, easedT);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = upPosition;

        //Go to standard position
        elapsed = 0f;
        while (elapsed < yUpTime)
        {
            float t = elapsed / yUpTime;
            // Ease In Formula: starts slow, ends fast
            float easedT = Mathf.Sin(t * Mathf.PI * 0.5f);

            transform.localPosition = Vector3.Lerp(upPosition, startPosition, easedT);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = startPosition;

        //Maybe ricochet

        // A biome change arrived on the way up: go down again to swap the sprites
        if (biomeSwapPending)
        {
            yield return DownAndUpAnimation(ApplyCurrentBiome);
            yield break;
        }

        isAnimating = false;
        animator.enabled = true;
        coll.enabled = true;
    }
}
