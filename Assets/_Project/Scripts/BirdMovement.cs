using System.Collections;
using UnityEngine;

public class BirdMovement : MonoBehaviour
{

    //MinXSpawn = -11f, MaxXSpawn = 11f, YSpawn = 0f, 
    //X Range

    [Header("Length values, Values randomized, same randoms for all fishes")]
    [Tooltip("The max curve length for a single curve should be: 7f")]
    [SerializeField] public float maxCurveLength = 20f;
    [Tooltip("The min curve length for a single curve should be: 2.5f")]
    [SerializeField] public float minCurveLength = 5f;

    //lifeTime before destroying the object
    private float lifeTime = 15f;
    [SerializeField] private float minLifeTime = 5f;
    [SerializeField] private float maxLifeTime = 15f;
    //Random found balues
    [SerializeField] private float minHeight = 0f;
    [SerializeField] private float maxHeight = 0f;
    private float speed = 0f;
    [SerializeField] private float minSpeed = 2.2f;
    [SerializeField] private float maxSpeed = 3.3f;
    [SerializeField] private float exitSpeedMultiplier = 3f;
    [SerializeField] private float curveLength;
    private float rotationMaxAngle = 10f;

    private float dropTime = 0.5f;
    [SerializeField] private float minDropTime = 0.3f;
    [SerializeField] private float maxDropTime = 0.6f;
    [SerializeField] private float ricochetDropTime = 0.6f;
    [SerializeField] private float maxYDrop = -32f;
    [SerializeField] private float minYDrop = -19f;
    int direction = 1;
    private float rotationSpeed = 5f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        curveLength = Random.Range(minCurveLength, maxCurveLength);
        lifeTime = Random.Range(minLifeTime, maxLifeTime);
        speed = Random.Range(minSpeed, maxSpeed);
        dropTime = Random.Range(minDropTime, maxDropTime);
        float dropY = Random.Range(minYDrop, maxYDrop);
        StartCoroutine(DropBird(dropY));
    }

    private IEnumerator DropBird(float dropPosition)
    {

        float elapsedTime = 0f;
        float startY = transform.localPosition.y;
        float bottomY = dropPosition - 4f;

        // --- PHASE 1: THE ACCELERATED DROP ---
        while (elapsedTime < dropTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / dropTime;
            float newY = Mathf.Lerp(startY, bottomY, t * t);
            transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);
            yield return null;
        }



        // --- PHASE 3: THE BACK-AND-FORTH BOBBING ---
        Vector3 anchorLocalPos = transform.localPosition;
        float bobElapsed = 0f;
        float rotationEaseInDuration = 1.5f; // How long it takes to reach full tilt intensity

        float horizontalAmplitude = curveLength / 2f;
        float verticalAmplitude = (maxHeight - minHeight) / 2f;

        // RANDOMIZE START DIRECTION: 1 is Right, -1 is Left
        float startDirection = (Random.value > 0.5f) ? 1f : -1f;
        float scaleX = startDirection;
        transform.localScale = new Vector3(-scaleX * startDirection, 1f, 1f);

        while (bobElapsed < lifeTime)
        {
            bobElapsed += Time.deltaTime;

            // 1. Horizontal Oscillation
            float xOffset = Mathf.Sin(bobElapsed * speed) * horizontalAmplitude * startDirection;

            // 2. Vertical Oscillation
            float yOffset = Mathf.Sin(bobElapsed * speed * 2f) * verticalAmplitude;

            // 3. Apply Position
            transform.localPosition = new Vector3(
                anchorLocalPos.x + xOffset,
                anchorLocalPos.y + yOffset,
                anchorLocalPos.z
            );

            // 4. SEAMLESS ROTATION
            // Calculate the target tilt as before
            float horizontalVelocity = Mathf.Cos(bobElapsed * speed);
            float targetTilt = horizontalVelocity * rotationMaxAngle;

            // Calculate a multiplier that goes from 0 to 1 over 'rotationEaseInDuration'
            float rotationIntensity = Mathf.Min(bobElapsed / rotationEaseInDuration, 1f);

            // Apply the tilt scaled by the intensity
            float currentTilt = targetTilt * rotationIntensity;
            transform.localRotation = Quaternion.Euler(0, 0, -currentTilt);

            // 5. Visual Flip (Only flip if intensity is high enough to look natural)
            if (rotationIntensity > 0.5f)
            {
                scaleX = (horizontalVelocity >= 0) ? 1f : -1f;
                transform.localScale = new Vector3(-scaleX * startDirection, 1f, 1f);
            }

            yield return null;
        }

        // --- PHASE 4: PREPARE FOR EXIT (Wait for the bottom) ---
        // We wait until the Sin wave for Y (speed * 2) is near its lowest point (-1)
        // This ensures the "Up" launch starts from the very bottom of the bob
        while (Mathf.Sin(bobElapsed * speed * 2f) > -0.9f)
        {
            bobElapsed += Time.deltaTime;
            // Keep moving normally until we hit that bottom point
            float xOffset = Mathf.Sin(bobElapsed * speed) * horizontalAmplitude;
            float yOffset = Mathf.Sin(bobElapsed * speed * 2f) * verticalAmplitude;
            transform.localPosition = new Vector3(anchorLocalPos.x + xOffset, anchorLocalPos.y + yOffset, anchorLocalPos.z);
            yield return null;
        }

        // --- PHASE 5: THE FAST EXIT ---
        float exitDirX = Mathf.Cos(bobElapsed * speed) >= 0 ? 1f : -1f;

        // We create a velocity based on the direction it was already heading
        Vector3 exitVelocity = new Vector3(exitDirX * (speed * exitSpeedMultiplier), 25f, 0f);

        // 1. CAPTURE CURRENT STATE
        Quaternion startingRotation = transform.localRotation; // Where it's leaning right now
        float escapeAngle = exitDirX > 0 ? -45f : 45f;
        Quaternion targetRotation = Quaternion.Euler(0, 0, escapeAngle);

        float exitTimer = 0f;
        float rotationTurningDelay = 0.5f; // How long to wait before it starts looking up

        while (exitTimer < 3f)
        {
            exitTimer += Time.deltaTime;

            // Move upward and outward
            transform.localPosition += exitVelocity * Time.deltaTime;

            // 2. SEAMLESS ROTATION TRANSITION
            // We use a t value that stays 0 for a moment, then moves to 1
            float rotationT = Mathf.Clamp01((exitTimer - rotationTurningDelay) / 1.0f);

            // Slerp from the specific rotation it had when bobbing ended to the skyward rotation
            transform.localRotation = Quaternion.Slerp(startingRotation, targetRotation, rotationT);

            yield return null;
        }
    }

}
