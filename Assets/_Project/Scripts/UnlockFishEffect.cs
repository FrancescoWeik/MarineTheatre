using System.Collections;
using UnityEngine;

public class UnlockFishEffect : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float moveUpDistance = 1f;
    [SerializeField] private float duration = 5f;
    public void SetSprite(Sprite sprite)
    {
        spriteRenderer.sprite = sprite;
    }

    public void OnEnable()
    {
        StartCoroutine(UpAndDisappear());
    }

    public IEnumerator UpAndDisappear()
    {
        float elapsed = 0f;
        while(elapsed< duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.position += Vector3.up * (moveUpDistance * Time.deltaTime / duration);
            spriteRenderer.color = new Color(1f, 1f, 1f, 1f - t);
            yield return null;
        }

        //Add item to inventory and then destroy
        Destroy(gameObject);
    }
}
