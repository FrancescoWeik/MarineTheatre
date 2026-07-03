using UnityEngine;
using UnityEngine.UI;

public class FishUIDetails : MonoBehaviour
{
    [SerializeField] private Image fishImage;
    [SerializeField] private Image stickImage;

    public void SetFishData(FishGeneralDataScriptable fishData) {
        fishImage.sprite = fishData.inPlaySprite;
        RectTransform rectTransform = stickImage.GetComponent<RectTransform>();

        // Update the anchoredPosition using its current X and your new Y offset
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, fishData.stickYOffset);
    }
}
