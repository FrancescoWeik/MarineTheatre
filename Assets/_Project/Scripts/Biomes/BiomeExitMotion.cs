using System.Collections;
using UnityEngine;

// Exit used by the spawned animals when the biome changes: same feel as BiomeTransitionElement.Exit
// (small anticipation up, then fast fall down out of the screen), then the animal is destroyed.
public static class BiomeExitMotion
{
    private const float MaxStartDelay = 0.12f;
    private const float AnticipationHeight = 1f;
    private const float AnticipationTime = 0.15f;
    private const float FallTime = 0.4f;
    private const float FallDistance = 45f;

    public static IEnumerator ExitDownAndDestroy(Transform target)
    {
        // A small random delay so the animals don't all move as one block
        yield return new WaitForSeconds(Random.Range(0f, MaxStartDelay));

        Vector3 start = target.position;
        Vector3 up = start + Vector3.up * AnticipationHeight;
        Vector3 down = start + Vector3.down * FallDistance;

        // Anticipation up (ease out)
        yield return Move(target, start, up, AnticipationTime, t => Mathf.Sin(t * Mathf.PI * 0.5f));
        // Fall down out of the screen (ease in: starts slow, ends fast)
        yield return Move(target, up, down, FallTime, t => t * t);

        Object.Destroy(target.gameObject);
    }

    private static IEnumerator Move(Transform target, Vector3 from, Vector3 to, float duration, System.Func<float, float> ease)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            target.position = Vector3.LerpUnclamped(from, to, ease(elapsed / duration));
            elapsed += Time.deltaTime;
            yield return null;
        }
        target.position = to;
    }
}
