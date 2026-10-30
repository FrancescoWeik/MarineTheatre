using System.Collections;
using UnityEngine;

public class SeaMovement : MonoBehaviour
{
    [SerializeField] private float maxXMovement = 5f;
    [SerializeField] private float speed = 5f;

    // Saved once, so re-enabling doesn't use an already shifted position as the new center
    private Vector3 startPos;
    // Kept between enables, so the movement resumes from where it stopped
    private float angle = 0f;

    private void Awake()
    {
        startPos = transform.localPosition;
    }

    private void OnEnable()
    {
        StartCoroutine(StartSeaMovement());
    }

    private void OnDisable()
    {
        // Disabling only the component doesn't stop its coroutines: stop it so re-enabling doesn't run two
        StopAllCoroutines();
    }

    private IEnumerator StartSeaMovement()
    {
        while (true) // Forever loop
        {
            // Increase the angle based on time and speed
            angle += Time.deltaTime * speed;

            // Mathf.Sin returns -1 to 1.
            // Multiplying by maxDistance gives us -5 to 5.
            float xOffset = Mathf.Sin(angle) * maxXMovement;
            // Apply the position
            transform.localPosition = new Vector3(startPos.x + xOffset, startPos.y, startPos.z);

            yield return null;
        }
    }
}
