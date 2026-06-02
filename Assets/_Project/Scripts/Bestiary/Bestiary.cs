using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bestiary : MonoBehaviour
{
    [SerializeField] private GameObject fishBestiaryPrefab;
    [SerializeField] private Transform fishGridContainer;
    [SerializeField] private Button bestiaryButton;
    [SerializeField] private Button backButton;
    FishGeneralDataScriptable[] allFishesData;
    private List<FishContainer> fishContainers = new List<FishContainer>();
    private Canvas bestiaryCanvas;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bestiaryCanvas = GetComponent<Canvas>();
        allFishesData = Resources.LoadAll<FishGeneralDataScriptable>("FishGeneralData");

        foreach(FishGeneralDataScriptable fishData in allFishesData)
        {

            GameObject fishBestiaryEntry = Instantiate(fishBestiaryPrefab, fishGridContainer);
            FishContainer entryComponent = fishBestiaryEntry.GetComponent<FishContainer>();
            entryComponent.SetFishImage(fishData.marketSprite);
            entryComponent.SetFishTitle(fishData.title);

            bool unlocked = SaveSystem.GetItemClicked(fishData.title);
            if (unlocked)
            {
                entryComponent.DisplayUnlocked();
            }

            fishContainers.Add(entryComponent);
        }
    }

    public void OpenBestiary()
    {
        Debug.LogError("Open bestiary, now close curtains...");
        bestiaryButton.enabled = false;
        Curtains.Instance.onFinishClosing.AddListener(DisplayBestiary);
        Curtains.Instance.CloseCurtains();
        foreach (FishContainer fishContainer in fishContainers)
        {

            bool unlocked = SaveSystem.GetItemClicked(fishContainer.Title);
            if (unlocked)
            {
                fishContainer.DisplayUnlocked();
            }
        }
    }

    private void DisplayBestiary()
    {
        bestiaryButton.enabled = true;
        bestiaryCanvas.enabled = true;
        Curtains.Instance.onFinishClosing.RemoveListener(DisplayBestiary);
        Curtains.Instance.OpenCurtains();
    }


    public void CloseBestiaru()
    {
        Debug.LogError("Close bestiary, now close curtains...");
        backButton.enabled = false;
        Curtains.Instance.onFinishClosing.AddListener(HideBestiary);
        Curtains.Instance.CloseCurtains();
    }

    private void HideBestiary()
    {
        bestiaryCanvas.enabled = false;
        backButton.enabled = true;
        Curtains.Instance.onFinishClosing.RemoveListener(HideBestiary);
        Curtains.Instance.OpenCurtains();
    }
}
