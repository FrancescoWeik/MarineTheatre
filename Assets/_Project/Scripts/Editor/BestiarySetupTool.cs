using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// One-shot tool: makes the bestiary grid scrollable.
// Body -> ScrollView (ScrollRect + RectMask2D, ends above the back arrow) -> Grid (content, grows with the entries)
public static class BestiarySetupTool
{
    // Space between the top of the back arrow and the bottom of the scroll area
    private const float MarginOverBackButton = 20f;

    [MenuItem("Tools/UI/Setup Bestiary Scroll (one-shot)")]
    private static void SetupBestiaryScroll()
    {
        if (EditorApplication.isPlaying)
        {
            Fail("Esci dal Play mode prima di lanciare il setup.");
            return;
        }

        Bestiary bestiary = Object.FindFirstObjectByType<Bestiary>(FindObjectsInactive.Include);
        if (bestiary == null) { Fail("Nessun Bestiary nella scena aperta."); return; }

        SerializedObject bestiarySerialized = new SerializedObject(bestiary);
        RectTransform grid = bestiarySerialized.FindProperty("fishGridContainer").objectReferenceValue as RectTransform;
        Button backButton = bestiarySerialized.FindProperty("backButton").objectReferenceValue as Button;
        if (grid == null) { Fail("Bestiary non ha il Grid (fishGridContainer) assegnato."); return; }
        if (grid.GetComponentInParent<ScrollRect>(true) != null) { Fail("La collezione è già scorrevole: setup già fatto."); return; }

        RectTransform body = (RectTransform)grid.parent;

        // Bottom of the scroll area: just above the back arrow, so the entries never go under it
        float bottom = 180f;
        if (backButton != null)
        {
            RectTransform buttonRect = (RectTransform)backButton.transform;
            if (Mathf.Approximately(buttonRect.anchorMin.y, 0f) && Mathf.Approximately(buttonRect.anchorMax.y, 0f))
                bottom = buttonRect.anchoredPosition.y + (1f - buttonRect.pivot.y) * buttonRect.sizeDelta.y + MarginOverBackButton;
        }

        // Scroll view: same area as the old grid, minus the bottom strip with the arrow
        GameObject scrollView = new GameObject("ScrollView", typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(scrollView, "Setup Bestiary Scroll");
        scrollView.layer = grid.gameObject.layer;
        RectTransform scrollRectTransform = (RectTransform)scrollView.transform;
        scrollRectTransform.SetParent(body, false);
        scrollRectTransform.SetSiblingIndex(grid.GetSiblingIndex());
        scrollRectTransform.anchorMin = new Vector2(0f, 0f);
        scrollRectTransform.anchorMax = new Vector2(1f, 1f);
        scrollRectTransform.pivot = new Vector2(0.5f, 0.5f);
        scrollRectTransform.offsetMin = new Vector2(0f, bottom);
        scrollRectTransform.offsetMax = Vector2.zero;

        // Invisible image: lets the drag start also on the empty space between the entries
        Image dragArea = scrollView.AddComponent<Image>();
        dragArea.color = new Color(1f, 1f, 1f, 0f);
        dragArea.raycastTarget = true;
        // Cuts the entries at the edges of the scroll area
        scrollView.AddComponent<RectMask2D>();

        // Grid becomes the scrolled content: anchored on top, as tall as its entries
        Undo.SetTransformParent(grid, scrollRectTransform, "Setup Bestiary Scroll");
        Undo.RecordObject(grid, "Setup Bestiary Scroll");
        // The bestiary canvas has scale 0 in edit mode: keeping the world transform while reparenting would give the grid scale 0
        grid.localScale = Vector3.one;
        grid.localRotation = Quaternion.identity;
        grid.anchorMin = new Vector2(0f, 1f);
        grid.anchorMax = new Vector2(1f, 1f);
        grid.pivot = new Vector2(0.5f, 1f);
        grid.anchoredPosition = Vector2.zero;
        grid.sizeDelta = Vector2.zero;
        ContentSizeFitter fitter = Undo.AddComponent<ContentSizeFitter>(grid.gameObject);
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scroll = scrollView.AddComponent<ScrollRect>();
        scroll.content = grid;
        scroll.viewport = scrollRectTransform;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Elastic;
        scroll.elasticity = 0.1f;
        scroll.inertia = true;
        scroll.decelerationRate = 0.135f;
        scroll.scrollSensitivity = 30f;

        // The bestiary goes back to the top every time it opens
        Undo.RecordObject(bestiary, "Setup Bestiary Scroll");
        bestiarySerialized.Update();
        bestiarySerialized.FindProperty("scrollRect").objectReferenceValue = scroll;
        bestiarySerialized.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(bestiary.gameObject.scene);
        Selection.activeGameObject = scrollView;
        Debug.Log($"[BestiarySetup] Collezione scorrevole creata (area di scroll fino a {bottom:0.#} px dal fondo, sopra la freccia). Salva la scena.");
    }

    private static void Fail(string message)
    {
        Debug.LogError("[BestiarySetup] " + message);
        EditorUtility.DisplayDialog("Setup Bestiary", message, "OK");
    }
}
