using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;

public class SunMoonMovement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private float sunSpeed = 0.5f;
    private float halFmoonSpeed = 0.3f;
    private float fullMoonSpeed = 0.1f;

    private int currentActive = 0;

    [SerializeField] private Collider2D coll;
    [SerializeField] private Transform sunMoonContainer;
    [SerializeField] private List<GameObject> sunMoonObjects = new List<GameObject>();


    [SerializeField] private float yUpTarget = 11f;
    [SerializeField] private float yUpTime = 0.2f;

    [SerializeField] private float yDownTarget = 0f;
    [SerializeField] private float yDownTime = 0.3f;

    private Animator animator;
    private bool hovered = false;
    private Vector3 startPosition = Vector3.zero;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentActive = 0;
        startPosition = transform.localPosition;
        animator = GetComponent<Animator>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
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
        animator.enabled = false;
        coll.enabled = false;

        int prevDay = currentActive;
        currentActive++;
        if (currentActive >= sunMoonObjects.Count)
        {
            currentActive = 0;

        }

        StartCoroutine(ChangeDayAnimation(prevDay, currentActive));

    }

    private IEnumerator ChangeDayAnimation(int _prevDay, int newDay)
    {
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

            transform.position = Vector3.Lerp(upPosition, downPosition, easedT);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = downPosition;

        //Change day element
        sunMoonObjects[_prevDay].SetActive(false);
        if (currentActive >= sunMoonObjects.Count)
        {
            currentActive = 0;

        }
        sunMoonObjects[currentActive].SetActive(true);

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

        //TODO change background color and possible image


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



        animator.enabled = true;
        coll.enabled = true;
    }
}
