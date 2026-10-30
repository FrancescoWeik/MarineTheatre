using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// For objects shared between biomes that only change look (e.g. Boat -> Jeep):
// on biome change it exits, swaps the active object while out of the screen, then enters again.
[RequireComponent(typeof(BiomeTransitionElement))]
public class BiomeObjectSwapper : MonoBehaviour
{
    [Tooltip("One object per biome, same order as the BiomeManager biomes list")]
    [SerializeField] private List<GameObject> objectPerBiome = new List<GameObject>();

    [Tooltip("Seconds after the biome change before starting to exit")]
    [SerializeField] private float exitDelay = 0f;
    [Tooltip("Seconds spent out of the screen before coming back")]
    [SerializeField] private float hiddenTime = 0.1f;

    private BiomeTransitionElement transitionElement;
    private int shownBiomeIndex = -1;
    private Coroutine swapRoutine;

    private void Awake()
    {
        transitionElement = GetComponent<BiomeTransitionElement>();
    }

    private void OnEnable()
    {
        SubscribeToBiomeManager();
    }

    private void Start()
    {
        // Fallback: on scene load BiomeManager.Awake may run after this OnEnable
        SubscribeToBiomeManager();
        if (BiomeManager.Instance != null)
            ShowObject(BiomeManager.Instance.CurrentBiomeIndex);
    }

    private void OnDisable()
    {
        if (BiomeManager.Instance != null)
            BiomeManager.Instance.onBiomeChanged.RemoveListener(OnBiomeChanged);

        // Disabling kills the coroutine: next time start from the rest position with the right object
        if (swapRoutine != null)
        {
            swapRoutine = null;
            transitionElement.ShowInstant();
            if (BiomeManager.Instance != null)
                ShowObject(BiomeManager.Instance.CurrentBiomeIndex);
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

    private void OnBiomeChanged(int biomeIndex)
    {
        // If it's already moving, the running routine picks up the latest biome
        if (swapRoutine == null)
            swapRoutine = StartCoroutine(SwapRoutine());
    }

    private IEnumerator SwapRoutine()
    {
        if (exitDelay > 0f)
            yield return new WaitForSeconds(exitDelay);

        // Loop in case the biome changed again while this was moving
        while (shownBiomeIndex != BiomeManager.Instance.CurrentBiomeIndex)
        {
            yield return transitionElement.Exit();

            if (hiddenTime > 0f)
                yield return new WaitForSeconds(hiddenTime);
            ShowObject(BiomeManager.Instance.CurrentBiomeIndex);

            yield return transitionElement.Enter();
        }

        swapRoutine = null;
    }

    private void ShowObject(int biomeIndex)
    {
        shownBiomeIndex = biomeIndex;
        // No object for this biome: keep the current one
        if (biomeIndex < 0 || biomeIndex >= objectPerBiome.Count || objectPerBiome[biomeIndex] == null)
            return;

        for (int i = 0; i < objectPerBiome.Count; i++)
        {
            if (objectPerBiome[i] != null)
                objectPerBiome[i].SetActive(i == biomeIndex);
        }
    }
}
