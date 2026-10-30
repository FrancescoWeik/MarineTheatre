using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Ship : MonoBehaviour, IPointerClickHandler, ISerializationCallbackReceiver
{
    // One entry per look of the vehicle (Ship, Jeep...): the active one rotates and unlocks its own bestiary data
    [System.Serializable]
    private class Vehicle
    {
        public SpriteRenderer spriteRenderer;
        public FishGeneralDataScriptable data;
        [System.NonSerialized] public Quaternion standardRotation;
    }

    [SerializeField] private List<Vehicle> vehicles = new List<Vehicle>();

    // Old single-vehicle fields: only read to move them into the vehicles list
    [SerializeField, HideInInspector] private FishGeneralDataScriptable shipData;
    [SerializeField, HideInInspector] private SpriteRenderer spriteRenderer;


    private IEnumerator rotateRoutine;
    private Vehicle rotatingVehicle;
    [SerializeField] private float angleRange = 15f;
    [SerializeField] private float rotationRoutineSpeed = 80f;

    private void Start()
    {
        foreach (var vehicle in vehicles)
        {
            if (vehicle.spriteRenderer != null)
                vehicle.standardRotation = vehicle.spriteRenderer.transform.localRotation;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Vehicle vehicle = GetActiveVehicle();
        if (vehicle == null)
            return;

        if (vehicle.data != null)
        {
            bool wasClicked = SaveSystem.GetItemClicked(vehicle.data.title);
            if (!wasClicked)
            {
                SaveSystem.SetItemClicked(vehicle.data.title, true);

            }
        }

        if (rotateRoutine != null)
        {
            StopCoroutine(rotateRoutine);
            // The vehicle changed while the old one was still rotating: don't leave it tilted
            if (rotatingVehicle != vehicle)
                rotatingVehicle.spriteRenderer.transform.localRotation = rotatingVehicle.standardRotation;
        }
        rotatingVehicle = vehicle;
        rotateRoutine = RotateSpriteRoutine(vehicle);
        StartCoroutine(rotateRoutine);
    }

    private Vehicle GetActiveVehicle()
    {
        foreach (var vehicle in vehicles)
        {
            if (vehicle.spriteRenderer != null && vehicle.spriteRenderer.gameObject.activeInHierarchy)
                return vehicle;
        }
        return null;
    }

    private IEnumerator RotateSpriteRoutine(Vehicle vehicle)
    {
        Transform spriteTransform = vehicle.spriteRenderer.transform;
        Quaternion startRot = vehicle.standardRotation;

        float[] angles = new float[] { -15f, 15f, -7f, 7f, -4f, 0f };

        for (int i = 0; i < angles.Length; i++)
        {
            // Speed decays from full to near zero as we go through the bounces
            float decayFactor = 1f - (float)i / angles.Length; // 1.0 → 0.16
            float currentSpeed = Mathf.Max(rotationRoutineSpeed * decayFactor, 5f); // clamp to avoid 0

            Quaternion targetRot = startRot * Quaternion.Euler(0f, 0f, angles[i]);
            Quaternion fromRot = spriteTransform.localRotation;
            float duration = Quaternion.Angle(fromRot, targetRot) / currentSpeed;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                spriteTransform.localRotation = Quaternion.Lerp(fromRot, targetRot, t);
                yield return null;
            }

            spriteTransform.localRotation = targetRot;
        }

        spriteTransform.localRotation = startRot;
    }

    public void OnBeforeSerialize() { }

    public void OnAfterDeserialize()
    {
        // Migration from the single-vehicle version: the old Ship becomes the first vehicle
        if (vehicles == null)
            vehicles = new List<Vehicle>();
        if (vehicles.Count == 0 && spriteRenderer != null)
        {
            vehicles.Add(new Vehicle { spriteRenderer = spriteRenderer, data = shipData });
            spriteRenderer = null;
            shipData = null;
        }
    }
}
