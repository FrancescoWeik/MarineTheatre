using UnityEngine;

public class Bestiary : MonoBehaviour
{
    [SerializeField] private GameObject fishBestiaryPrefab;
    [SerializeField] private Transform fishGridContainer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FishGeneralDataScriptable[] allFishesData = Resources.LoadAll<FishGeneralDataScriptable>("FishGeneralData");

        foreach(FishGeneralDataScriptable fishData in allFishesData)
        {
            GameObject fishBestiaryEntry = Instantiate(fishBestiaryPrefab, fishGridContainer);
            FishContainer entryComponent = fishBestiaryEntry.GetComponent<FishContainer>();
            entryComponent.SetFishImage(fishData.marketSprite);
        }
    }

}
