using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandMovement : MonoBehaviour
{
    //Every  4 or 5 or 6 days passed after last movement, spawn an island and move it

    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private List<GameObject> islandList = new List<GameObject>();
    [SerializeField] private Vector2 dayRangeIslandSpawn;
    [SerializeField] private float timeToMove = 10f;

    private int currentDaysPassed = 0;
    private int daysToWait = 0;

    private int currentIslandIndex = 0;
    private bool isIslandMoving = false;

    private void OnEnable()
    {
        SubscribeToDayManager();
    }

    private void Start()
    {
        currentIslandIndex = 0;
        daysToWait = RandomizeDaysValue();
        // Fallback: on scene load DayManager.Awake may run after this OnEnable
        SubscribeToDayManager();
    }

    private void OnDisable()
    {
        if (DayManager.Instance != null)
            DayManager.Instance.onDayChanged.RemoveListener(OnDayChanged);

        // Disabling kills the coroutine: if an island was moving, reset it or no island would ever move again
        if (isIslandMoving)
        {
            StopAllCoroutines();
            FinishIslandMovement();
        }
    }

    private void SubscribeToDayManager()
    {
        if (DayManager.Instance == null)
            return;

        // Remove first so calling this from both OnEnable and Start doesn't register it twice
        DayManager.Instance.onDayChanged.RemoveListener(OnDayChanged);
        DayManager.Instance.onDayChanged.AddListener(OnDayChanged);
    }

    private void OnDayChanged(int dayType) {
        // The reset to the morning on a biome change is not a day passing
        if (BiomeManager.Instance != null && BiomeManager.Instance.IsChanging)
            return;

        if (!isIslandMoving)
        {
            currentDaysPassed++;
            if (currentDaysPassed >= daysToWait)
            {
                isIslandMoving = true;
                ShowIsland(currentIslandIndex);
                StartCoroutine(MoveIslandRoutine());
            }
        }
    }

    private IEnumerator MoveIslandRoutine()
    {
        float elapsed = 0f;
        while(elapsed < timeToMove)
        {
            float t = elapsed / timeToMove;
            Vector3 newPosition = Vector3.Lerp(startPoint.position, endPoint.position, t);
            transform.position = newPosition;
            elapsed += Time.deltaTime;
            yield return null;
        }

        FinishIslandMovement();
    }

    private void FinishIslandMovement()
    {
        //Reset the position
        transform.position = startPoint.position;
        currentDaysPassed = 0;
        isIslandMoving = false;
        daysToWait = RandomizeDaysValue();

        if (currentIslandIndex >= islandList.Count - 1)
        {
            currentIslandIndex = 0;
        }
        else
            currentIslandIndex++;
    }

    private int RandomizeDaysValue()
    {
        return Random.Range((int)dayRangeIslandSpawn.x, (int)dayRangeIslandSpawn.y + 1);
    }

    private void ShowIsland(int value) {

        foreach(GameObject island in islandList)
        {
            island.SetActive(false);
        }
        islandList[value].SetActive(true);

    }
   
}
