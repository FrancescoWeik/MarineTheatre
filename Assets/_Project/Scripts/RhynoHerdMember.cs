using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

// One rhino of the herd. The herd root moves across the screen with FishMovement, this only moves the rhino inside the herd:
// calm rhinos bob up and down on their own, the agitated one runs left and right, then joins the herd and becomes a calm one.
public class RhynoHerdMember : MonoBehaviour, IPointerClickHandler
{
    private enum State { Calm, Agitated, Joining }

    [SerializeField] private FishGeneralDataScriptable rhynoData;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Calm: small independent up and down")]
    [SerializeField] private Vector2 bobAmplitudeRange = new Vector2(0.1f, 0.25f);
    [SerializeField] private Vector2 bobSpeedRange = new Vector2(1.5f, 2.5f);

    [Header("Agitated: runs left and right, then joins the herd")]
    [SerializeField] private bool startsAgitated = false;
    [Tooltip("Sprite used once it calms down")]
    [SerializeField] private Sprite calmSprite;
    [SerializeField] private Vector2 agitatedTimeRange = new Vector2(4f, 8f);
    [Tooltip("Local x limits of the run, relative to the herd")]
    [SerializeField] private float agitatedMinX = -6f;
    [SerializeField] private float agitatedMaxX = 8f;
    [SerializeField] private float agitatedSpeed = 9f;
    [Tooltip("Small hops while running")]
    [SerializeField] private float runHopHeight = 0.2f;
    [SerializeField] private float runHopSpeed = 14f;
    [Tooltip("Local position it takes in the herd once calm")]
    [SerializeField] private Vector2 herdPosition = new Vector2(3.6f, 0.3f);
    [SerializeField] private float joinSpeed = 4f;

    private State state;
    // Position the movement is computed from: the hop/bob is added on top of it
    private Vector3 basePosition;
    private float bobAmplitude;
    private float bobSpeed;
    private float bobPhase;
    private float agitatedTimeLeft;
    private int runDirection = 1;

    private IEnumerator rotateRoutine;
    [SerializeField] private float rotationRoutineSpeed = 80f;
    private Quaternion standardSpriteRotation;

    private void Awake()
    {
        basePosition = transform.localPosition;
        bobAmplitude = Random.Range(bobAmplitudeRange.x, bobAmplitudeRange.y);
        bobSpeed = Random.Range(bobSpeedRange.x, bobSpeedRange.y);
        bobPhase = Random.Range(0f, Mathf.PI * 2f);
        standardSpriteRotation = spriteRenderer.transform.localRotation;

        if (startsAgitated)
        {
            state = State.Agitated;
            agitatedTimeLeft = Random.Range(agitatedTimeRange.x, agitatedTimeRange.y);
            runDirection = Random.value < 0.5f ? -1 : 1;
        }
        else
        {
            state = State.Calm;
        }
    }

    private void Update()
    {
        switch (state)
        {
            case State.Calm:
                transform.localPosition = basePosition + Vector3.up * (Mathf.Sin(Time.time * bobSpeed + bobPhase) * bobAmplitude);
                break;

            case State.Agitated:
                basePosition.x += runDirection * agitatedSpeed * Time.deltaTime;
                if (basePosition.x >= agitatedMaxX)
                {
                    basePosition.x = agitatedMaxX;
                    runDirection = -1;
                }
                else if (basePosition.x <= agitatedMinX)
                {
                    basePosition.x = agitatedMinX;
                    runDirection = 1;
                }
                Face(runDirection);
                transform.localPosition = basePosition + Vector3.up * RunHop();

                agitatedTimeLeft -= Time.deltaTime;
                if (agitatedTimeLeft <= 0f)
                    state = State.Joining;
                break;

            case State.Joining:
                Vector3 target = new Vector3(herdPosition.x, herdPosition.y, basePosition.z);
                if (!Mathf.Approximately(target.x, basePosition.x))
                    Face(target.x > basePosition.x ? 1 : -1);
                basePosition = Vector3.MoveTowards(basePosition, target, joinSpeed * Time.deltaTime);
                transform.localPosition = basePosition + Vector3.up * RunHop();

                if (basePosition == target)
                    CalmDown();
                break;
        }
    }

    // Arrived in the herd: stops being agitated and from now on behaves like the others
    private void CalmDown()
    {
        state = State.Calm;
        Face(1);
        if (calmSprite != null)
            spriteRenderer.sprite = calmSprite;
    }

    private float RunHop()
    {
        return Mathf.Abs(Mathf.Sin(Time.time * runHopSpeed + bobPhase)) * runHopHeight;
    }

    // 1 = same direction as the herd (the sprites look right), -1 = turned back
    private void Face(int direction)
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * direction;
        transform.localScale = scale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //Check not unlocked first....
        if (rhynoData != null)
        {
            bool wasClicked = SaveSystem.GetItemClicked(rhynoData.title);
            if (!wasClicked)
            {
                SaveSystem.SetItemClicked(rhynoData.title, true);

            }
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
