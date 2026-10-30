using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

// Moves an object out of / into the screen during a biome change.
// Put it on a container that only this script moves: the children can keep their own movement
// (SeaMovement, SimpleYMovement, Animator) and the two movements add up instead of overwriting each other.
public class BiomeTransitionElement : MonoBehaviour
{
    [Header("Exit: small anticipation up, then fast fall down")]
    [SerializeField] private float anticipationHeight = 1f;
    [SerializeField] private float anticipationTime = 0.15f;
    [SerializeField] private float exitTime = 0.35f;

    [Header("Enter: rise from below, small overshoot, settle")]
    [SerializeField] private float enterTime = 0.5f;
    [SerializeField] private float enterOvershoot = 0.5f;
    [SerializeField] private float settleTime = 0.15f;

    [Header("General")]
    [Tooltip("How far below the rest position the element goes to be out of the screen")]
    [SerializeField] private float offScreenDistance = 40f;
    [Tooltip("While exiting, draw this element in front of the entering one on the same sorting layer")]
    [SerializeField] private bool bringToFrontOnExit = true;

    private Vector3 restPosition;
    private bool initialized = false;

    private Vector3 HiddenPosition => restPosition + Vector3.down * offScreenDistance;

    private void Awake()
    {
        EnsureInitialized();
    }

    // The biome root can be inactive at load, so Awake may run later than the first call
    private void EnsureInitialized()
    {
        if (initialized)
            return;
        restPosition = transform.localPosition;
        initialized = true;
    }

    public void ShowInstant()
    {
        EnsureInitialized();
        transform.localPosition = restPosition;
    }

    public void HideInstant()
    {
        EnsureInitialized();
        transform.localPosition = HiddenPosition;
    }

    public IEnumerator Exit()
    {
        EnsureInitialized();
        SortingGroup[] sortingGroups = bringToFrontOnExit ? GetComponentsInChildren<SortingGroup>(true) : new SortingGroup[0];
        foreach (var sortingGroup in sortingGroups)
            sortingGroup.sortingOrder += 1;

        //Anticipation up (ease out)
        Vector3 start = transform.localPosition;
        yield return Move(start, restPosition + Vector3.up * anticipationHeight, anticipationTime, t => Mathf.Sin(t * Mathf.PI * 0.5f));

        //Fall down out of the screen (ease in: starts slow, ends fast)
        yield return Move(transform.localPosition, HiddenPosition, exitTime, t => t * t);

        foreach (var sortingGroup in sortingGroups)
            sortingGroup.sortingOrder -= 1;
    }

    public IEnumerator Enter()
    {
        EnsureInitialized();

        //Rise from below a bit over the rest position (ease out)
        Vector3 overshootPosition = restPosition + Vector3.up * enterOvershoot;
        yield return Move(HiddenPosition, overshootPosition, enterTime, t => Mathf.Sin(t * Mathf.PI * 0.5f));

        //Settle in the rest position
        yield return Move(overshootPosition, restPosition, settleTime, t => Mathf.SmoothStep(0f, 1f, t));
    }

    private IEnumerator Move(Vector3 from, Vector3 to, float duration, Func<float, float> ease)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            transform.localPosition = Vector3.LerpUnclamped(from, to, ease(elapsed / duration));
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = to;
    }
}
