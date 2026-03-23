using UnityEngine;

[CreateAssetMenu(fileName = "FishMovementData", menuName = "ScriptableObjects/FishMovementData")]
public class FishMovementScriptable : ScriptableObject
{
    [Header("Height values")]
    [SerializeField] public float maxEndHeight; 
    [SerializeField] public float minEndHeight;
    [SerializeField] public float maxStartHeight;
    [SerializeField] public float minStartHeight;

    [Header("GeneralValues")]
    [SerializeField] public float maxSpeed;
    [SerializeField] public float minSpeed;
    //[Tooltip("8 is front, 6 is behind first wave, 4 is behind second wave, 2 is behind third wave")]
    [SerializeField] public FishSortingLayer sortingLayer = FishSortingLayer.Front;

    [Header("LayerHeights, do not modify")]
    [SerializeField] public float completelyFrontToBeSeend = -17.5f;
    [SerializeField] public float firstLayerHeightToBeSeen = -10.5f;
    [SerializeField] public float secondLayerHeightToBeSeen = -7.5f;
    [SerializeField] public float thirdLayerHeightToBeSeen = -4.5f;
}

public enum FishSortingLayer
{
    Front,
    Front_1,
    Front_2,
    Front_3,
}
