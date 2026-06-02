using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class FishMovement : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private FishGeneralDataScriptable fishGeneralData;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject unlockPrefab;

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
    private float rotationSpeed = 20f;
    private float rotationMaxAngle = 10f;
    private int currentRotationDirection = 1;

    private IEnumerator rotateRoutine;
    [SerializeField] private float angleRange = 15f;
    [SerializeField] private float rotationRoutineSpeed = 80f;
    [SerializeField] private float pingPongDuration = 0.8f;
    private Quaternion standardSpriteRotation;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rotationRoutineSpeed = 80f;
        pingPongDuration = 1.5f;
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

        //Set rotation randomness
        currentRotationDirection = Random.Range(1, 2);
        rotationSpeed = Random.Range(8f, 10f);
        rotationMaxAngle = 5f;

        standardSpriteRotation = spriteRenderer.transform.localRotation;
        StartCoroutine(MovementRoutine(direction));
    }

    private IEnumerator MovementRoutine(int _direction)
    {
        Vector3 startPosition = transform.position;
        float elapsed = 0f;

        float amplitude = (maxHeight - minHeight) / 2f;
        float midPoint = (maxHeight + minHeight) / 2f;

        float currentZ = transform.eulerAngles.z;

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

            //ROTATION
            /*currentZ += currentRotationDirection * rotationSpeed * Time.deltaTime;

            // Check if we hit the limit and flip the direction
            if (currentZ >= rotationMaxAngle)
            {
                currentZ = rotationMaxAngle;
                currentRotationDirection = -1;
            }
            else if (currentZ <= -rotationMaxAngle)
            {
                currentZ = -rotationMaxAngle;
                currentRotationDirection = 1;
            }

            transform.rotation = Quaternion.Euler(0, 0, currentZ);*/

            // 1. Calculate a 't' value that oscillates between -1 and 1
            // We use 'elapsed' to keep it moving forward over time
            float t = Mathf.Sin(elapsed * (rotationSpeed / 5f));

            // 2. Multiply by your max angle
            float zAngle = t * rotationMaxAngle;

            // 3. Apply
            transform.rotation = Quaternion.Euler(0, 0, zAngle);

            yield return null;
        }
        Destroy(this.gameObject);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.LogError("Fish clicked");

        //Check not unlocked first....

        //If not unlocked then spawn the effect for the unlock part
        /*GameObject unlockedGO = Instantiate(unlockPrefab);
        unlockedGO.transform.position = spriteRenderer.transform.position;
        unlockedGO.transform.rotation = spriteRenderer.transform.rotation;
        unlockedGO.GetComponent<UnlockFishEffect>().SetSprite(fishGeneralData.inPlaySprite);*/

        //Unlock fish, do animation for that fish
        if (rotateRoutine != null)
        {
            StopCoroutine(rotateRoutine);
        }
        rotateRoutine = RotateSpriteRoutine();
        StartCoroutine(rotateRoutine);
    }

    private IEnumerator RotateSpriteRoutine()
    {
        Quaternion startRot = standardSpriteRotation;

        float[] angles = new float[] { -15f, 15f, -7f, 7f, -4f, 0f };

        for (int i = 0; i < angles.Length; i++)
        {
            // Speed decays from full to near zero as we go through the bounces
            float decayFactor = 1f - (float)i / angles.Length; // 1.0 → 0.16
            float currentSpeed = Mathf.Max(rotationRoutineSpeed * decayFactor, 5f); // clamp to avoid 0

            Quaternion targetRot = startRot * Quaternion.Euler(0f, 0f, angles[i]);
            Quaternion fromRot = spriteRenderer.transform.localRotation;
            float duration = Quaternion.Angle(fromRot, targetRot) / currentSpeed;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                spriteRenderer.transform.localRotation = Quaternion.Lerp(fromRot, targetRot, t);
                yield return null;
            }

            spriteRenderer.transform.localRotation = targetRot;
        }

        spriteRenderer.transform.localRotation = startRot;
    }
}
