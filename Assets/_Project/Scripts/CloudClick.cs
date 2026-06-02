using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class CloudClick : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private FishGeneralDataScriptable cloudGeneralData;
    [SerializeField] private GameObject cloud;
    [SerializeField] private float destructionGetBigTime = 0.2f;
    [SerializeField] private float cloudDestructionTime = 0.4f;
    [SerializeField] private float waitTime = 2f;
    [SerializeField] private float cloudReAppearTime = 2f;
    [SerializeField] private float sizeMultiplier = 1.5f;
    private bool animating = false;

    public void OnPointerClick(PointerEventData eventData)
    {


        //Check not unlocked first....
        bool wasClicked = SaveSystem.GetItemClicked(cloudGeneralData.title);
        if (!wasClicked)
        {
            SaveSystem.SetItemClicked(cloudGeneralData.title, true);

        }

        if (!animating)
        {
            StartCoroutine(CloudPop());
        }
    }

    private IEnumerator CloudPop()
    {
        animating = true;   
        float elapsed = 0f;
        Vector3 startScale = cloud.transform.localScale;
        Vector3 endScale = startScale * sizeMultiplier;

        while (elapsed < destructionGetBigTime)
        {
            elapsed = elapsed + Time.deltaTime;
            cloud.transform.localScale = Vector3.Lerp(startScale, endScale, elapsed / destructionGetBigTime);
            yield return null;
        }
        cloud.transform.localScale = endScale;

        elapsed = 0f;
        startScale = endScale;
        endScale = Vector3.zero;
        //Scale to 0
        while (elapsed < cloudDestructionTime)
        {
            elapsed = elapsed + Time.deltaTime;
            cloud.transform.localScale = Vector3.Lerp(startScale, endScale, elapsed / cloudDestructionTime);
            yield return null;
        }
        cloud.transform.localScale = endScale;

        yield return new WaitForSeconds(waitTime);

        //Make cloud reAppear
        elapsed = 0f;
        startScale = Vector3.one * sizeMultiplier;
        while (elapsed < cloudReAppearTime)
        {
            elapsed = elapsed + Time.deltaTime;
            cloud.transform.localScale = Vector3.Lerp(endScale, startScale, elapsed / cloudReAppearTime);
            yield return null;
        }
        cloud.transform.localScale = startScale;

        //Make cloud reAppear
        elapsed = 0f;
        endScale = Vector3.one;
        while (elapsed < destructionGetBigTime)
        {
            elapsed = elapsed + Time.deltaTime;
            cloud.transform.localScale = Vector3.Lerp(startScale, endScale, elapsed / destructionGetBigTime);
            yield return null;
        }
        cloud.transform.localScale = endScale;

        animating = false;
    }
}
