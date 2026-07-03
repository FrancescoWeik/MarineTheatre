using UnityEngine;
using UnityEngine.UI;

public class FishUIDetails : MonoBehaviour
{
    [SerializeField] private Image fishImage;

    public void SetFishData(FishGeneralDataScriptable fishData) {
        fishImage.sprite = fishData.inPlaySprite;
    }
}
