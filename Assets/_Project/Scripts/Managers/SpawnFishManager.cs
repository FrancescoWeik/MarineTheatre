#if UNITY_EDITOR
using UnityEditor; // Required for editor scripting
#endif
using System.Collections.Generic;
using UnityEngine;

public class SpawnFishManager : MonoBehaviour
{
    public static SpawnFishManager Instance { get; private set; }

    [Header("Fish Spawn Day Data")]
    [SerializeField] private List<FishSpawnerDayStructure> fishSpawners = new List<FishSpawnerDayStructure>();

    [Header("Transform where to spawn fishes")]
    [SerializeField] private Transform[] spawnContainers;

    [Header("General Data")]
    [SerializeField] private float minSpawnTime = 1.5f;
    [SerializeField] private float maxSpawnTimeDelay = 8f;
    [SerializeField] private int maxSpawnAtATime = 4;
    [SerializeField] private float maxPossibleRotation = 10f;

    [Header("FrenzyMode")]
    [SerializeField] private float minFrenzyModeSpawnTime = 0.1f;
    [SerializeField] private float maxFrenzyModeSpawnTime = 0.35f;
    private bool frenzyMode = false;

    private FishSpawnerDayStructure currentDaySpawner;

    private float currentSpawnWaitTime = 0f;
    private float spawnTimeElapsed = 0f;

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
        currentDaySpawner = fishSpawners[0];
    }

    private void Start()
    {
        DayManager.Instance.onDayChanged.AddListener(OnDayChanged);
    }

    private void OnDisable()
    {
        DayManager.Instance.onDayChanged.RemoveListener(OnDayChanged);
    }

    private void Update()
    {
        if(spawnTimeElapsed > currentSpawnWaitTime)
        {
            float numberToSpawn = Random.Range(0f, maxSpawnAtATime + 1);

            for(int i=0; i < numberToSpawn; i++)
            {
                //Get the spawn transform
                int spawnContainerIndex = Random.Range(0, 2);
                int direction = 1;
                if (spawnContainerIndex == 0)
                    direction = -1;

                GameObject fish;
                //Get the random fish prefab based on the day
                float randomValue = Random.Range(0f, 100f);
                if (randomValue < currentDaySpawner.lowChancePercentage)
                {
                    //Spawn low chance fish
                    int index = Random.Range(0, currentDaySpawner.lowChanceFishSpawn.Count);
                    fish = Instantiate(currentDaySpawner.lowChanceFishSpawn[index], spawnContainers[spawnContainerIndex]);
                    fish.GetComponent<FishMovement>().StartMoving(direction);
                }
                else if (randomValue < currentDaySpawner.mediumChancePercentage)
                {
                    //Spawn medium chance fish
                    //Spawn low chance fish
                    int index = Random.Range(0, currentDaySpawner.mediumChanceFishSpawn.Count);
                    fish = Instantiate(currentDaySpawner.mediumChanceFishSpawn[index], spawnContainers[spawnContainerIndex]);
                    fish.GetComponent<FishMovement>().StartMoving(direction);
                }
                else
                {
                    //Spawn high chance fish
                    //Spawn low chance fish
                    int index = Random.Range(0, currentDaySpawner.highChanceFishSpawn.Count);
                    fish = Instantiate(currentDaySpawner.highChanceFishSpawn[index], spawnContainers[spawnContainerIndex]);
                    fish.GetComponent<FishMovement>().StartMoving(direction);
                }

                fish.transform.localRotation = Quaternion.Euler(fish.transform.localRotation.x, fish.transform.localRotation.y, Random.Range(-maxPossibleRotation, maxPossibleRotation));
            }
          
            if (!frenzyMode)
                currentSpawnWaitTime = Random.Range(minSpawnTime, maxSpawnTimeDelay);
            else
                currentSpawnWaitTime = Random.Range(minFrenzyModeSpawnTime, maxFrenzyModeSpawnTime);
            spawnTimeElapsed = 0f;
        }
        spawnTimeElapsed = spawnTimeElapsed + Time.deltaTime;
    }

    private void OnDayChanged(int value)
    {
        currentDaySpawner = fishSpawners[value];
    }

    public void ChangeFrenzyMode()
    {
        frenzyMode = !frenzyMode;
        if (frenzyMode)
        {
            currentSpawnWaitTime = Random.Range(minFrenzyModeSpawnTime, maxFrenzyModeSpawnTime);
        }
        else
        {
            currentSpawnWaitTime = Random.Range(minSpawnTime, maxSpawnTimeDelay);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SpawnFishManager))] // Tells Unity this is for MyScript
public class SpawnFishManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector fields (variables like 'speed', etc.)
        DrawDefaultInspector();

        SpawnFishManager myScript = (SpawnFishManager)target;

        // Create the button
        if (GUILayout.Button("EnableDisable FrenzyMode"))
        {
            myScript.ChangeFrenzyMode();
        }
    }
}
#endif
