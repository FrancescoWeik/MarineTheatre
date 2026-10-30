using System.Collections;
using UnityEngine;

public class SimpleYMovement : MonoBehaviour
{
    [SerializeField] private float maxYMovement = -1f;
    [SerializeField] private float time = 0.5f;
    private float startYPosition;
    private float passedTime = 0f;


    private void Awake()
    {
        startYPosition = transform.localPosition.y;
    }

    private void Update()
    {
        passedTime = passedTime + Time.deltaTime;
        if (passedTime < time)
        {
            float yOffset = Mathf.Lerp(startYPosition, startYPosition + maxYMovement, passedTime / time);
            transform.localPosition = new Vector3(transform.localPosition.x, yOffset, transform.localPosition.z);
        }else if (passedTime >= time && passedTime<time*2f)
        {
            float yOffset = Mathf.Lerp(startYPosition + maxYMovement, startYPosition, (passedTime - time) / time);
            transform.localPosition = new Vector3(transform.localPosition.x, yOffset, transform.localPosition.z);
        }
        else
        {
            passedTime = 0f;
        }
    }
}
