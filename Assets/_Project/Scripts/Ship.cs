using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class Ship : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private FishGeneralDataScriptable shipData;
    [SerializeField]
    private SpriteRenderer spriteRenderer;


    private IEnumerator rotateRoutine;
    [SerializeField] private float angleRange = 15f;
    [SerializeField] private float rotationRoutineSpeed = 80f;
    private Quaternion standardSpriteRotation;

    private void Start()
    {
        standardSpriteRotation = spriteRenderer.transform.localRotation;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        bool wasClicked = SaveSystem.GetItemClicked(shipData.title);
        if (!wasClicked)
        {
            SaveSystem.SetItemClicked(shipData.title, true);

        }

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
