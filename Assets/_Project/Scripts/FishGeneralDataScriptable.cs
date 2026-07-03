using UnityEngine;

[CreateAssetMenu(fileName = "FishGeneralData", menuName = "ScriptableObjects/FishGeneralData")]
public class FishGeneralDataScriptable : ScriptableObject
{
    public Sprite inPlaySprite;
    public Sprite marketSprite;
    public float stickYOffset = -288f;
    public string description;
    public string title;
}
