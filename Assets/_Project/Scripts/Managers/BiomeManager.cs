#if UNITY_EDITOR
using UnityEditor; // Required for editor scripting
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BiomeManager : MonoBehaviour
{
    public static BiomeManager Instance { get; private set; }

    [SerializeField] private List<BiomeController> biomes = new List<BiomeController>();
    [SerializeField] private int startBiomeIndex = 0;

    [Tooltip("Seconds after the old biome starts exiting before the new one starts entering (overlap)")]
    [SerializeField] private float enterDelay = 0.3f;

    public UnityEvent<int> onBiomeChanged;

    public int CurrentBiomeIndex { get; private set; }
    public BiomeController CurrentBiome => CurrentBiomeIndex >= 0 && CurrentBiomeIndex < biomes.Count ? biomes[CurrentBiomeIndex] : null;
    public bool IsChanging { get; private set; }

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

        // Only the start biome is visible
        CurrentBiomeIndex = startBiomeIndex;
        for (int i = 0; i < biomes.Count; i++)
        {
            if (i == startBiomeIndex)
                biomes[i].ShowInstant();
            else
                biomes[i].HideInstant();
        }
    }

    public void NextBiome()
    {
        ChangeBiome((CurrentBiomeIndex + 1) % biomes.Count);
    }

    public void ChangeBiome(int index)
    {
        if (IsChanging || index == CurrentBiomeIndex || index < 0 || index >= biomes.Count)
            return;

        StartCoroutine(ChangeBiomeRoutine(index));
    }

    private IEnumerator ChangeBiomeRoutine(int index)
    {
        IsChanging = true;
        BiomeController previousBiome = biomes[CurrentBiomeIndex];
        BiomeController nextBiome = biomes[index];
        CurrentBiomeIndex = index;
        onBiomeChanged.Invoke(index);

        // The routines run on this manager, so deactivating the old biome root doesn't stop them
        Coroutine exit = StartCoroutine(previousBiome.ExitRoutine());
        if (enterDelay > 0f)
            yield return new WaitForSeconds(enterDelay);
        Coroutine enter = StartCoroutine(nextBiome.EnterRoutine());

        yield return exit;
        yield return enter;
        IsChanging = false;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(BiomeManager))]
public class BiomeManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BiomeManager myScript = (BiomeManager)target;

        GUI.enabled = Application.isPlaying && !myScript.IsChanging;
        if (GUILayout.Button("Next Biome (Play Mode)"))
        {
            myScript.NextBiome();
        }
        GUI.enabled = true;
    }
}
#endif
