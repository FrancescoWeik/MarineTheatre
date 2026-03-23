using System.Collections;
using UnityEngine;

public class SeaMovement : MonoBehaviour
{
    [SerializeField] private float maxXMovement = 5f;
    [SerializeField] private float speed = 5f;

    private void Start()
    {
        StartCoroutine(StartSeaMovement());
    }

    private IEnumerator StartSeaMovement()
    {
        float angle = 0f;
        Vector3 startPos = transform.localPosition;

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
