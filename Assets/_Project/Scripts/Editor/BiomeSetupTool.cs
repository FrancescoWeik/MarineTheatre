using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

// One-shot tool (Tools > Biomes > Setup Biomes):
// 1. MarineLogic prefab: wraps each background layer in a container with BiomeTransitionElement and adds the BiomeController
// 2. Rebuilds AfricaLogic as a Prefab Variant of MarineLogic, copying the africa backgrounds from the old object
// 3. Adds the BiomeManager to the scene
// The old AfricaLogic is kept, inactive, as "AfricaLogic_OLD". This file can be deleted once used.
public static class BiomeSetupTool
{
    private const string MarinePrefabPath = "Assets/_Project/Prefabs/Biomes/MarineLogic.prefab";
    private const string AfricaPrefabPath = "Assets/_Project/Prefabs/Biomes/AfricaLogic.prefab";
    private const string BackgroundName = "BackgroundFull";
    private const float StepDelay = 0.15f;

    [MenuItem("Tools/Biomes/Setup Biomes (one-shot)")]
    private static void Setup()
    {
        // ---- Checks: nothing is modified until all of them pass ----
        GameObject marineAsset = AssetDatabase.LoadAssetAtPath<GameObject>(MarinePrefabPath);
        if (marineAsset == null) { Fail($"Non trovo {MarinePrefabPath}."); return; }
        if (marineAsset.GetComponent<BiomeController>() != null || AssetDatabase.LoadAssetAtPath<GameObject>(AfricaPrefabPath) != null)
        {
            Fail("Il setup sembra già fatto (MarineLogic ha già il BiomeController o AfricaLogic.prefab esiste già).");
            return;
        }

        var scene = EditorSceneManager.GetActiveScene();
        GameObject logic = scene.GetRootGameObjects().FirstOrDefault(g => g.name == "Logic");
        if (logic == null) { Fail("Non trovo il GameObject 'Logic' nella scena aperta."); return; }

        Transform marine = logic.transform.Find("MarineLogic");
        Transform oldAfrica = logic.transform.Find("AfricaLogic");
        if (marine == null || oldAfrica == null) { Fail("Non trovo 'Logic/MarineLogic' o 'Logic/AfricaLogic'."); return; }
        if (!PrefabUtility.IsOutermostPrefabInstanceRoot(marine.gameObject)
            || PrefabUtility.GetCorrespondingObjectFromSource(marine.gameObject) != marineAsset)
        {
            Fail("'Logic/MarineLogic' in scena non è un'istanza del prefab MarineLogic.");
            return;
        }

        // The layered africa background: the child that contains the SeaMovement layers
        Transform africaBackground = oldAfrica.Cast<Transform>().FirstOrDefault(c => c.GetComponentInChildren<SeaMovement>(true) != null);
        if (africaBackground == null) { Fail("Non trovo il background a livelli dentro 'AfricaLogic'."); return; }

        if (!EditorUtility.DisplayDialog("Setup Biomes",
            "Questo tool modifica il prefab MarineLogic, crea AfricaLogic.prefab (variant) e modifica la scena.\n\n" +
            "Ctrl+Z NON basta per annullarlo: assicurati di aver salvato la scena e fatto un commit prima di procedere.",
            "Procedi", "Annulla"))
            return;

        // ---- 1. MarineLogic prefab ----
        GameObject contents = PrefabUtility.LoadPrefabContents(MarinePrefabPath);
        try
        {
            SetupBiomeRoot(contents);
            PrefabUtility.SaveAsPrefabAsset(contents, MarinePrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contents);
        }
        marineAsset = AssetDatabase.LoadAssetAtPath<GameObject>(MarinePrefabPath);

        // ---- 2. AfricaLogic as a variant of MarineLogic ----
        GameObject africa = (GameObject)PrefabUtility.InstantiatePrefab(marineAsset, oldAfrica.parent);
        africa.name = "AfricaLogic";
        africa.transform.SetSiblingIndex(oldAfrica.GetSiblingIndex() + 1);
        africa.transform.localPosition = oldAfrica.localPosition;
        africa.transform.localRotation = oldAfrica.localRotation;
        africa.transform.localScale = oldAfrica.localScale;

        CopyBackground(africaBackground, africa.transform.Find(BackgroundName));

        // The old africa had no islands
        Transform islands = africa.transform.Find("IslandLogic");
        if (islands != null)
            islands.gameObject.SetActive(false);

        // Everything else of the old africa (e.g. the single BackgroundAfrica sprite) is moved as it is
        foreach (Transform child in oldAfrica.Cast<Transform>().ToList())
        {
            if (child != africaBackground)
                child.SetParent(africa.transform, true);
        }

        GameObject africaAsset = PrefabUtility.SaveAsPrefabAssetAndConnect(africa, AfricaPrefabPath, InteractionMode.AutomatedAction);
        if (africaAsset == null) { Fail("Salvataggio di AfricaLogic.prefab fallito, guarda la Console."); return; }
        if (PrefabUtility.GetPrefabAssetType(africaAsset) != PrefabAssetType.Variant)
            Debug.LogWarning("[BiomeSetup] AfricaLogic.prefab è stato creato ma NON risulta una Prefab Variant.");

        // ---- 3. Old africa kept for comparison ----
        oldAfrica.name = "AfricaLogic_OLD";
        oldAfrica.gameObject.SetActive(false);

        // ---- 4. BiomeManager ----
        GameObject managerObject = new GameObject("BiomeManager");
        managerObject.transform.SetParent(logic.transform, false);
        Transform dayManager = logic.transform.Find("DayManager");
        if (dayManager != null)
            managerObject.transform.SetSiblingIndex(dayManager.GetSiblingIndex() + 1);

        BiomeManager manager = managerObject.AddComponent<BiomeManager>();
        SerializedObject managerSerialized = new SerializedObject(manager);
        SerializedProperty biomes = managerSerialized.FindProperty("biomes");
        biomes.arraySize = 2;
        biomes.GetArrayElementAtIndex(0).objectReferenceValue = marine.GetComponent<BiomeController>();
        biomes.GetArrayElementAtIndex(1).objectReferenceValue = africa.GetComponent<BiomeController>();
        managerSerialized.ApplyModifiedPropertiesWithoutUndo();
        if (marine.GetComponent<BiomeController>() == null || africa.GetComponent<BiomeController>() == null)
            Debug.LogWarning("[BiomeSetup] Collega a mano i BiomeController di MarineLogic e AfricaLogic nella lista 'Biomes' del BiomeManager.");

        // Only the start biome visible in the editor too (BiomeManager does the same at runtime)
        africa.SetActive(false);

        EditorSceneManager.MarkSceneDirty(scene);
        Selection.activeGameObject = managerObject;
        Debug.Log("[BiomeSetup] Fatto: MarineLogic aggiornato, AfricaLogic.prefab creato come variant, BiomeManager aggiunto. Controlla e salva la scena.");
    }

    // Wraps Logic/Boat in a container moved by the biome transition (the Boat Animator animates the Boat
    // position itself) and makes it swap Ship <-> Jeep while out of the screen
    [MenuItem("Tools/Biomes/Setup Boat (one-shot)")]
    private static void SetupBoat()
    {
        var scene = EditorSceneManager.GetActiveScene();
        GameObject logic = scene.GetRootGameObjects().FirstOrDefault(g => g.name == "Logic");
        Transform boat = logic != null ? logic.transform.Find("Boat") : null;
        if (boat == null) { Fail("Non trovo 'Logic/Boat' nella scena aperta (il setup è già stato fatto?)."); return; }

        Transform ship = boat.Find("Ship");
        Transform jeep = boat.Find("Jeep");
        if (ship == null || jeep == null) { Fail("Non trovo 'Ship' e 'Jeep' dentro 'Logic/Boat'."); return; }

        GameObject container = new GameObject("BoatContainer");
        Undo.RegisterCreatedObjectUndo(container, "Setup Boat");
        container.transform.SetParent(logic.transform, false);
        container.transform.SetSiblingIndex(boat.GetSiblingIndex());
        // The container sits at zero, so the Boat keeps its local position
        Undo.SetTransformParent(boat, container.transform, "Setup Boat");

        BiomeTransitionElement element = Undo.AddComponent<BiomeTransitionElement>(container);
        SerializedObject elementSerialized = new SerializedObject(element);
        elementSerialized.FindProperty("bringToFrontOnExit").boolValue = false;
        elementSerialized.ApplyModifiedPropertiesWithoutUndo();

        // Same order as the BiomeManager biomes list: 0 = Marine, 1 = Africa
        BiomeObjectSwapper swapper = Undo.AddComponent<BiomeObjectSwapper>(container);
        SerializedObject swapperSerialized = new SerializedObject(swapper);
        SerializedProperty objects = swapperSerialized.FindProperty("objectPerBiome");
        objects.arraySize = 2;
        objects.GetArrayElementAtIndex(0).objectReferenceValue = ship.gameObject;
        objects.GetArrayElementAtIndex(1).objectReferenceValue = jeep.gameObject;
        swapperSerialized.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        Selection.activeGameObject = container;
        Debug.Log("[BiomeSetup] Boat pronta: 'Logic/BoatContainer' con BiomeTransitionElement e BiomeObjectSwapper (Ship = Marine, Jeep = Africa). Salva la scena.");
    }

    // Wraps each background layer in a moving container and adds the BiomeController to the biome root
    private static void SetupBiomeRoot(GameObject root)
    {
        Transform background = root.transform.Find(BackgroundName);

        // Layers = the children moved by SeaMovement, from the farthest (lowest sorting layer) to the nearest
        List<Transform> layers = background.Cast<Transform>()
            .Where(c => c.GetComponent<SeaMovement>() != null)
            .OrderBy(GetSortingDepth)
            .ToList();

        BiomeController controller = root.AddComponent<BiomeController>();
        SerializedObject controllerSerialized = new SerializedObject(controller);

        SerializedProperty steps = controllerSerialized.FindProperty("transitionSteps");
        steps.arraySize = layers.Count;
        for (int i = 0; i < layers.Count; i++)
        {
            Transform layer = layers[i];
            GameObject container = new GameObject($"Layer-{i + 1}");
            container.transform.SetParent(background, false);
            container.transform.SetSiblingIndex(layer.GetSiblingIndex());
            // The container sits at zero, so the layer keeps its local position
            layer.SetParent(container.transform, false);

            SerializedProperty step = steps.GetArrayElementAtIndex(i);
            step.FindPropertyRelative("element").objectReferenceValue = container.AddComponent<BiomeTransitionElement>();
            step.FindPropertyRelative("exitDelay").floatValue = i * StepDelay;
            step.FindPropertyRelative("enterDelay").floatValue = i * StepDelay;
        }

        List<Behaviour> logic = new List<Behaviour>();
        logic.AddRange(root.GetComponentsInChildren<SpawnFishManager>(true));
        logic.AddRange(root.GetComponentsInChildren<SpawnBirdManager>(true));
        SerializedProperty logicProperty = controllerSerialized.FindProperty("biomeLogic");
        logicProperty.arraySize = logic.Count;
        for (int i = 0; i < logic.Count; i++)
            logicProperty.GetArrayElementAtIndex(i).objectReferenceValue = logic[i];

        controllerSerialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static int GetSortingDepth(Transform layer)
    {
        SortingGroup sortingGroup = layer.GetComponentInChildren<SortingGroup>(true);
        return sortingGroup != null ? SortingLayer.GetLayerValueFromID(sortingGroup.sortingLayerID) : int.MaxValue;
    }

    // Copies the old africa background onto the variant's BackgroundFull (which now has the Layer-N containers)
    private static void CopyBackground(Transform source, Transform target)
    {
        target.localPosition = source.localPosition;
        target.localRotation = source.localRotation;
        target.localScale = source.localScale;

        List<Transform> containers = target.Cast<Transform>().Where(c => c.GetComponent<BiomeTransitionElement>() != null).ToList();
        List<Transform> layerTargets = containers.SelectMany(c => c.Cast<Transform>()).ToList();
        List<Transform> otherTargets = target.Cast<Transform>().Except(containers).ToList();

        var used = new HashSet<Transform>();
        foreach (Transform sourceChild in source.Cast<Transform>().ToList())
        {
            bool isLayer = sourceChild.GetComponent<SeaMovement>() != null;
            Transform match = (isLayer ? layerTargets : otherTargets).FirstOrDefault(t => t.name == sourceChild.name && !used.Contains(t));
            if (match != null)
            {
                used.Add(match);
                CopyTree(sourceChild, match);
            }
            else
            {
                Debug.LogWarning($"[BiomeSetup] '{sourceChild.name}' non esiste in MarineLogic: lo copio come oggetto aggiunto.");
                Object.Instantiate(sourceChild.gameObject, target).name = sourceChild.name;
            }
        }

        // Marine layers the africa background doesn't have: hide the whole container
        foreach (Transform unused in layerTargets.Where(t => !used.Contains(t)))
            unused.parent.gameObject.SetActive(false);
        foreach (Transform unused in otherTargets.Where(t => !used.Contains(t)))
            unused.gameObject.SetActive(false);
    }

    // Makes target look like source: transform, active state, components and children (matched by name)
    private static void CopyTree(Transform source, Transform target)
    {
        target.localPosition = source.localPosition;
        target.localRotation = source.localRotation;
        target.localScale = source.localScale;
        target.gameObject.SetActive(source.gameObject.activeSelf);
        CopyComponents(source.gameObject, target.gameObject);

        List<Transform> targetChildren = target.Cast<Transform>().ToList();
        var used = new HashSet<Transform>();
        foreach (Transform sourceChild in source.Cast<Transform>().ToList())
        {
            Transform match = targetChildren.FirstOrDefault(t => t.name == sourceChild.name && !used.Contains(t));
            if (match != null)
            {
                used.Add(match);
                CopyTree(sourceChild, match);
            }
            else
            {
                Object.Instantiate(sourceChild.gameObject, target).name = sourceChild.name;
            }
        }

        // Pieces of the marine background that the africa one doesn't have
        foreach (Transform unused in targetChildren.Where(t => !used.Contains(t)))
            unused.gameObject.SetActive(false);
    }

    private static void CopyComponents(GameObject source, GameObject target)
    {
        var used = new HashSet<Component>();
        foreach (Component sourceComponent in source.GetComponents<Component>())
        {
            if (sourceComponent == null || sourceComponent is Transform)
                continue;

            System.Type type = sourceComponent.GetType();
            Component match = target.GetComponents(type).FirstOrDefault(c => c.GetType() == type && !used.Contains(c));
            if (match == null)
                match = target.AddComponent(type);

            used.Add(match);
            EditorUtility.CopySerialized(sourceComponent, match);
        }

        // Components the africa piece doesn't have are disabled (removing them from a variant is not reversible)
        foreach (Component targetComponent in target.GetComponents<Component>())
        {
            if (targetComponent == null || targetComponent is Transform || used.Contains(targetComponent))
                continue;
            if (targetComponent is Behaviour behaviour)
                behaviour.enabled = false;
            else if (targetComponent is Renderer renderer)
                renderer.enabled = false;
        }
    }

    private static void Fail(string message)
    {
        Debug.LogError("[BiomeSetup] " + message);
        EditorUtility.DisplayDialog("Setup Biomes", message, "OK");
    }
}
