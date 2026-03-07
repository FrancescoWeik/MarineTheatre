using System.Collections;
using UnityEngine;

public class SpriteColorHandler : MonoBehaviour
{

    private SpriteRenderer spriteRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = DayManager.Instance.spriteDayColors[0];
        DayManager.Instance.onDayChanged.AddListener(OnDayChanged);
    }

    private void OnDayChanged(int day)
    {
        StartCoroutine(ChangeSpriteColor(day));
        //spriteRenderer.color = DayManager.Instance.spriteDayColors[day];
    }

    private IEnumerator ChangeSpriteColor(int day)
    {
        float elapsedTime = 0f;
        Color startColor = spriteRenderer.color;
        Color endColor = DayManager.Instance.spriteDayColors[day];
        while (elapsedTime < DayManager.Instance.dayTransitionTime)
        {
            spriteRenderer.color = Color.Lerp(startColor, endColor, elapsedTime / DayManager.Instance.dayTransitionTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        spriteRenderer.color = endColor; // Ensure the final color is set
    }
}
