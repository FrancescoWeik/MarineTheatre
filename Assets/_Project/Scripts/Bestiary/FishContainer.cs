using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FishContainer : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image lockedSprite;
    [SerializeField] private Image unlockedSprite;
    [SerializeField] private RectTransform curtains;

    [SerializeField] private Vector2 startCurtainsPosition;
    [SerializeField] private Vector2 endCurtainsPosition;
    [SerializeField] private float curtainsSpeed = 15f;


    public string Title;
    private bool unlocked = false;
    private bool hidden = true;
    private FishGeneralDataScriptable FishData;
    public UnityEvent<FishGeneralDataScriptable> onFishDetails;

    public void OnPointerClick(PointerEventData eventData)
    {
        if(!hidden && unlocked)
        {
            onFishDetails?.Invoke(FishData);
        }
        if (hidden)
        {
            StopAllCoroutines();
            hidden = false;
            StartCoroutine(ShowHideCurtains(curtains.anchoredPosition, endCurtainsPosition));
        }
        else
        {
            StopAllCoroutines();
            hidden = true;
            StartCoroutine(ShowHideCurtains(curtains.anchoredPosition, startCurtainsPosition));
        }
    }

    public void SetFishData(FishGeneralDataScriptable fishData)
    {
        FishData = fishData;
    }

    public void SetFishImage(Sprite sprite)
    {
        unlockedSprite.sprite = sprite;
    }

    public void SetFishTitle(string title)
    {
        Title = title;
    }

    public void DisplayUnlocked()
    {
        lockedSprite.gameObject.SetActive(false);
        unlockedSprite.gameObject.SetActive(true);
        unlocked = true;
    }

    private IEnumerator ShowHideCurtains(Vector2 start, Vector2 to)
    {
        float distance = Vector3.Distance(start, to);
        float duration = distance / curtainsSpeed;        // derive time from speed
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            curtains.anchoredPosition = Vector3.Lerp(start, to, t);
            yield return null;
        }

        curtains.anchoredPosition = to; // snap to exact target

    }
}
