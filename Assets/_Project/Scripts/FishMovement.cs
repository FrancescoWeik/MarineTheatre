using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class FishMovement : MonoBehaviour
{
    [Header("Length values, Values randomized, same randoms for all fishes")]
    [Tooltip("The max curve length for a single curve should be: 7f")]
    [SerializeField] public float maxCurveLength = 20f;
    [Tooltip("The min curve length for a single curve should be: 2.5f")]
    [SerializeField] public float minCurveLength = 5f;
    

    [Header("Movement list scriptable, defined and specifics for each fish")]
    [SerializeField] private List<FishMovementScriptable> fishPossibleMovements = new List<FishMovementScriptable>();


    private SortingGroup sortingGroup;
    //lifeTime before destroying the object
    private float lifeTime = 45f;
    //Random found balues
    [SerializeField]  private FishMovementScriptable chosenFishMovement;
    [SerializeField]  private float minHeight = 0f;
    [SerializeField]  private float maxHeight = 0f;
    [SerializeField]  private float speed = 0f;
    [SerializeField]  private float curveLength;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    public void StartMoving(int direction)
    {
        sortingGroup = GetComponent<SortingGroup>();

        //Find direction
        transform.localScale = new Vector3(direction, 1f, 1f);

        //Find the random fish scriptable
        int fishChoice = Random.Range(0, fishPossibleMovements.Count);
        chosenFishMovement = fishPossibleMovements[fishChoice];

        //Find curve Height
        minHeight = Random.Range(chosenFishMovement.minStartHeight, chosenFishMovement.maxStartHeight);
        maxHeight = Random.Range(chosenFishMovement.minEndHeight, chosenFishMovement.maxEndHeight);

        //Find speed 
        speed = Random.Range(chosenFishMovement.minSpeed, chosenFishMovement.maxSpeed);

        //Set sorting layer
        sortingGroup.sortingLayerName = chosenFishMovement.sortingLayer.ToString();

        curveLength = Random.Range(minCurveLength, maxCurveLength);

        transform.position = new Vector3(transform.position.x, minHeight, transform.position.z);
        StartCoroutine(MovementRoutine(direction));
    }

    private IEnumerator MovementRoutine(int _direction)
    {
        Vector3 startPosition = transform.position;
        float elapsed = 0f;

        float amplitude = (maxHeight - minHeight) / 2f;
        float midPoint = (maxHeight + minHeight) / 2f;

        while (elapsed < lifeTime)
        {
            elapsed += Time.deltaTime;

            // 1. Calculate Horizontal Progress
            float xProgress = (elapsed * speed) * _direction;

            // 2. Calculate Vertical Sine/Cosine position
            // We use (2 * PI / length) to ensure the wave repeats exactly every 'curveLength' units
            float cycle = (2f * Mathf.PI * xProgress) / curveLength;

            // Mathf.Cos starts at 1 (max height). Use Mathf.Sin if you want to start at the midpoint.
            float yOffset = Mathf.Cos(cycle) * amplitude + midPoint;

            // 3. Apply the movement
            // This moves the object relative to where it started.
            //transform.localPosition = startPosition + new Vector3(xProgress, yOffset - (midPoint + amplitude), 0);ù
            transform.position = new Vector3(startPosition.x + xProgress, yOffset, startPosition.z);

            yield return null;
        }
        Destroy(this.gameObject);
    }
}
