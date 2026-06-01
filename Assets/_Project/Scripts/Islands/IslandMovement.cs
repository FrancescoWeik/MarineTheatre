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

    private void Start()
    {
        currentIslandIndex = 0;
        daysToWait = RandomizeDaysValue();
        DayManager.Instance.onDayChanged.AddListener(OnDayChanged);
    }

    private void OnDayChanged(int dayType) {
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
