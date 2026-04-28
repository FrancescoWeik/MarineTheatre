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
    private float lifeTime = 45f;
    //Random found balues
    [SerializeField] private float minHeight = 0f;
    [SerializeField] private float maxHeight = 0f;
    [SerializeField] private float speed = 0f;
    [SerializeField] private float curveLength;
    private float rotationMaxAngle = 10f;

    [SerializeField] private float dropTime = 0.5f;
    [SerializeField] private float ricochetDropTime = 0.6f;
    [SerializeField] private float maxYDrop = -32f;
    [SerializeField] private float minYDrop = -19f;
    int direction = 1;
    private float rotationSpeed = 5f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        float dropY = Random.Range(minYDrop, maxYDrop);
        StartCoroutine(DropBird(dropY));
    }

    private IEnumerator DropBird(float dropPosition)
    {
        /*float elapsedTime = 0f;
        float minDrop = dropPosition - 4f;

        while(elapsedTime< dropTime)
        {
            float newY = Mathf.Lerp(0f, minDrop, elapsedTime / dropTime);
            transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = new Vector3(transform.localPosition.x, minDrop, transform.localPosition.z);

        //Now ricochet
        float startY = transform.localPosition.y;
        elapsedTime = 0f;
        while (elapsedTime < ricochetDropTime)
        {
            elapsedTime += Time.deltaTime;
            float newY = Mathf.Lerp(startY, dropPosition, elapsedTime / ricochetDropTime);
            transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);
            yield return null;
        }

        transform.localPosition = new Vector3(transform.localPosition.x, dropPosition, transform.localPosition.z);*/

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

        // --- PHASE 2: THE RISE ---
        /*elapsedTime = 0f;
        float riseDuration = Mathf.Abs(dropPosition - bottomY) / speed;

        while (elapsedTime < riseDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / riseDuration;
            float tEaseOut = t * (2f - t);
            float newY = Mathf.Lerp(bottomY, dropPosition, tEaseOut);
            transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);
            yield return null;
        }
        */
        // --- PHASE 3: THE BACK-AND-FORTH BOBBING ---
        Vector3 centerPosition = transform.position;
        float bobElapsed = 0f;

        // Adjust these to change the 'size' of the patrol area
        float horizontalAmplitude = curveLength / 2f;
        float verticalAmplitude = (maxHeight - minHeight) / 2f;

        while (true)
        {
            bobElapsed += Time.deltaTime;

            // 1. Horizontal Oscillation
            // We use Sin so it starts at 0 (the center) and moves outward
            float xOffset = Mathf.Sin(bobElapsed * speed) * horizontalAmplitude;

            // 2. Vertical Oscillation
            // We use -Mathf.Cos and +1 so the wave starts at 0 and goes UP first
            float yOffset = (-Mathf.Cos(bobElapsed * speed * 2f) + 1f) * verticalAmplitude;

            // 3. Apply Position
            transform.position = new Vector3(
                centerPosition.x + xOffset,
                centerPosition.y + yOffset,
                centerPosition.z
            );

            // 4. Procedural Rotation (Leaning)
            // horizontalVelocity is the derivative of Sin (which is Cos)
            float horizontalVelocity = Mathf.Cos(bobElapsed * speed);
            float tilt = horizontalVelocity * rotationMaxAngle;
            transform.rotation = Quaternion.Euler(0, 0, -tilt);

            // 5. Flip Visuals
            // Flips the sprite scale based on which way it's currently moving
            float scaleX = (horizontalVelocity >= 0) ? 1f : -1f;
            //transform.localScale = new Vector3(scaleX, 1f, 1f);

            yield return null;
        }
    }

}
