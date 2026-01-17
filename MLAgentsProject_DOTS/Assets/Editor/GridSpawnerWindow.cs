// GridSpawnerWindow.cs
// place this script anywhere; unity will compile editor parts only inside the editor

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public static class GridSpawnerMath
{
    public static uint MakeSeed(uint baseSeed, int index)
    {
        uint s = (uint)index * 747796405u ^ (baseSeed * 196613u) ^ 0x9E3779B9u;
        return s == 0u ? 1u : s;
    }
}

[BurstCompile]
public struct BuildTransformsJob : IJobParallelFor
{
    public int countX, countY, countZ;
    public float3 spacing;
    public float3 origin;
    public bool centerToOrigin;

    public uint baseSeed;

    public float2 rotXDeg;
    public float2 rotYDeg;
    public float2 rotZDeg;
    public bool randomizeRotation;

    public bool randomizeScale;
    public bool uniformScale;
    public float2 uniformScaleRange;
    public float2 scaleXRange;
    public float2 scaleYRange;
    public float2 scaleZRange;

    [WriteOnly] public NativeArray<float3> outPositions;
    [WriteOnly] public NativeArray<quaternion> outRotations;
    [WriteOnly] public NativeArray<float3> outScales;

    public void Execute(int index)
    {
        int x = index % countX;
        int t = index / countX;
        int z = t % countZ;
        int y = t / countZ;

        float3 half = centerToOrigin
            ? new float3((countX - 1) * 0.5f, (countY - 1) * 0.5f, (countZ - 1) * 0.5f)
            : 0f;

        float3 p = origin + (new float3(x, y, z) - half) * spacing;
        outPositions[index] = p;

        var rng = new Unity.Mathematics.Random(GridSpawnerMath.MakeSeed(baseSeed, index));

        quaternion r;
        if (randomizeRotation)
        {
            float3 eDeg = new float3(
                math.lerp(rotXDeg.x, rotXDeg.y, rng.NextFloat()),
                math.lerp(rotYDeg.x, rotYDeg.y, rng.NextFloat()),
                math.lerp(rotZDeg.x, rotZDeg.y, rng.NextFloat())
            );
            r = quaternion.EulerXYZ(math.radians(eDeg));
        }
        else
        {
            r = quaternion.identity;
        }
        outRotations[index] = r;

        float3 s;
        if (randomizeScale)
        {
            if (uniformScale)
            {
                float u = math.lerp(uniformScaleRange.x, uniformScaleRange.y, rng.NextFloat());
                s = new float3(u, u, u);
            }
            else
            {
                s = new float3(
                    math.lerp(scaleXRange.x, scaleXRange.y, rng.NextFloat()),
                    math.lerp(scaleYRange.x, scaleYRange.y, rng.NextFloat()),
                    math.lerp(scaleZRange.x, scaleZRange.y, rng.NextFloat())
                );
            }
        }
        else
        {
            s = new float3(1f, 1f, 1f);
        }
        outScales[index] = s;
    }
}

#if UNITY_EDITOR
public class GridSpawnerWindow : EditorWindow
{
    GameObject prefab;
    Transform parent;

    bool useTargetSceneAsset = false;
    SceneAsset targetSceneAsset;
    bool saveTargetScene = true;
    bool closeIfOpenedByTool = true;
    bool createRootInTargetScene = true;
    string rootNameInTargetScene = "GridSpawner_Root";

    int countX = 10, countY = 1, countZ = 10;
    Vector3 spacing = Vector3.one;
    Vector3 origin = Vector3.zero;
    bool centerToOrigin = true;

    bool randomizeRotation = true;
    Vector2 rotXDeg = new Vector2(0f, 0f);
    Vector2 rotYDeg = new Vector2(0f, 360f);
    Vector2 rotZDeg = new Vector2(0f, 0f);

    bool randomizeScale = false;
    bool uniformScale = true;
    Vector2 uniformScaleRange = new Vector2(1f, 1f);
    Vector2 scaleXRange = new Vector2(1f, 1f);
    Vector2 scaleYRange = new Vector2(1f, 1f);
    Vector2 scaleZRange = new Vector2(1f, 1f);

    uint baseSeed = 12345;

    bool keepPrefabConnection = true;
    bool registerUndo = true;

    // anchor: scroll
    Vector2 scrollPos;

    const int HardCap = 500000;

    [MenuItem("Tools/Grid Spawner")]
    public static void Open()
    {
        GetWindow<GridSpawnerWindow>("Grid Spawner");
    }

    void OnGUI()
    {
        // anchor: scroll view begin
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        try
        {
            EditorGUILayout.LabelField("prefab", EditorStyles.boldLabel);
            prefab = (GameObject)EditorGUILayout.ObjectField(prefab, typeof(GameObject), false);
            parent = (Transform)EditorGUILayout.ObjectField("parent (optional)", parent, typeof(Transform), true);
            keepPrefabConnection = EditorGUILayout.Toggle("keep prefab connection", keepPrefabConnection);
            registerUndo = EditorGUILayout.Toggle("register undo", registerUndo);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("target scene (optional)", EditorStyles.boldLabel);
            useTargetSceneAsset = EditorGUILayout.Toggle("use scene asset (subscene)", useTargetSceneAsset);
            EditorGUI.BeginDisabledGroup(!useTargetSceneAsset);
            targetSceneAsset = (SceneAsset)EditorGUILayout.ObjectField("scene asset", targetSceneAsset, typeof(SceneAsset), false);
            saveTargetScene = EditorGUILayout.Toggle("save scene", saveTargetScene);
            closeIfOpenedByTool = EditorGUILayout.Toggle("close if opened by tool", closeIfOpenedByTool);
            createRootInTargetScene = EditorGUILayout.Toggle("create/find root in target scene", createRootInTargetScene);
            EditorGUI.BeginDisabledGroup(!createRootInTargetScene);
            rootNameInTargetScene = EditorGUILayout.TextField("root name", rootNameInTargetScene);
            EditorGUI.EndDisabledGroup();
            EditorGUI.EndDisabledGroup();

            if (useTargetSceneAsset && targetSceneAsset == null)
            {
                EditorGUILayout.HelpBox("assign a scene asset (.unity). for entities subscene, drag the subscene scene file here.", MessageType.Warning);
            }
            else if (useTargetSceneAsset && parent != null && targetSceneAsset != null)
            {
                string targetPath = AssetDatabase.GetAssetPath(targetSceneAsset);
                if (!string.IsNullOrEmpty(targetPath) && parent.gameObject.scene.path != targetPath)
                {
                    EditorGUILayout.HelpBox("parent is not in the target scene. it will be ignored and a root will be used (if enabled).", MessageType.Info);
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("grid", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                countX = EditorGUILayout.IntField("count x", Mathf.Max(1, countX));
                countY = EditorGUILayout.IntField("count y", Mathf.Max(1, countY));
                countZ = EditorGUILayout.IntField("count z", Mathf.Max(1, countZ));
            }
            spacing = EditorGUILayout.Vector3Field("spacing", spacing);
            origin = EditorGUILayout.Vector3Field("origin", origin);
            centerToOrigin = EditorGUILayout.Toggle("center to origin", centerToOrigin);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("rotation randomization", EditorStyles.boldLabel);
            randomizeRotation = EditorGUILayout.Toggle("enable", randomizeRotation);
            EditorGUI.BeginDisabledGroup(!randomizeRotation);
            rotXDeg = EditorGUILayout.Vector2Field("x deg min,max", rotXDeg);
            rotYDeg = EditorGUILayout.Vector2Field("y deg min,max", rotYDeg);
            rotZDeg = EditorGUILayout.Vector2Field("z deg min,max", rotZDeg);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("scale randomization", EditorStyles.boldLabel);
            randomizeScale = EditorGUILayout.Toggle("enable", randomizeScale);
            EditorGUI.BeginDisabledGroup(!randomizeScale);
            uniformScale = EditorGUILayout.Toggle("uniform", uniformScale);
            if (uniformScale)
            {
                uniformScaleRange = EditorGUILayout.Vector2Field("uniform min,max", uniformScaleRange);
            }
            else
            {
                scaleXRange = EditorGUILayout.Vector2Field("x min,max", scaleXRange);
                scaleYRange = EditorGUILayout.Vector2Field("y min,max", scaleYRange);
                scaleZRange = EditorGUILayout.Vector2Field("z min,max", scaleZRange);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
            baseSeed = (uint)Mathf.Max(1, EditorGUILayout.IntField("seed", (int)baseSeed));

            int total = SafeMul(SafeMul(countX, countY), countZ);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"total instances: {total}");

            using (new EditorGUILayout.HorizontalScope())
            {
                bool validTarget = !useTargetSceneAsset || targetSceneAsset != null;
                GUI.enabled = prefab != null && validTarget && total > 0 && total <= HardCap;
                if (GUILayout.Button("spawn"))
                {
                    Spawn(total);
                }
                GUI.enabled = true;

                if (parent != null)
                {
                    if (GUILayout.Button("delete children of parent"))
                    {
                        DeleteAllChildren(parent);
                    }
                }

                bool canDeleteRoot = useTargetSceneAsset && targetSceneAsset != null && createRootInTargetScene && !string.IsNullOrEmpty(rootNameInTargetScene);
                EditorGUI.BeginDisabledGroup(!canDeleteRoot);
                if (GUILayout.Button("delete root in target scene"))
                {
                    DeleteRootInTargetScene();
                }
                EditorGUI.EndDisabledGroup();
            }

            EditorGUILayout.HelpBox("spawns are computed with a burst job then instantiated on main thread. if you set a target scene, it will be opened additively (if needed), modified, then saved/closed.", MessageType.Info);
        }
        finally
        {
            // anchor: scroll view end
            EditorGUILayout.EndScrollView();
        }
    }

    static int SafeMul(int a, int b)
    {
        long v = (long)a * (long)b;
        return (int)Mathf.Clamp(v, 0, int.MaxValue);
    }

    void Spawn(int total)
    {
        if (prefab == null) return;

        Scene targetScene = ResolveTargetScene(out bool openedByTool);
        Transform actualParent = ResolveParentForTargetScene(targetScene);

        int workerCount = Unity.Jobs.LowLevel.Unsafe.JobsUtility.JobWorkerCount;
        int batchesPerWorker = 4;
        int innerBatch = math.max(1, total / math.max(1, workerCount * batchesPerWorker));

        var positions = new NativeArray<float3>(total, Allocator.TempJob);
        var rotations = new NativeArray<quaternion>(total, Allocator.TempJob);
        var scales = new NativeArray<float3>(total, Allocator.TempJob);

        var job = new BuildTransformsJob
        {
            countX = countX,
            countY = countY,
            countZ = countZ,
            spacing = spacing,
            origin = origin,
            centerToOrigin = centerToOrigin,

            baseSeed = baseSeed,

            rotXDeg = rotXDeg,
            rotYDeg = rotYDeg,
            rotZDeg = rotZDeg,
            randomizeRotation = randomizeRotation,

            randomizeScale = randomizeScale,
            uniformScale = uniformScale,
            uniformScaleRange = uniformScaleRange,
            scaleXRange = scaleXRange,
            scaleYRange = scaleYRange,
            scaleZRange = scaleZRange,

            outPositions = positions,
            outRotations = rotations,
            outScales = scales
        };

        bool canceled = false;
        int spawned = 0;

        try
        {
            var handle = job.Schedule(total, innerBatch);
            handle.Complete();

            string undoName = "Spawn Grid";
            if (registerUndo) Undo.IncrementCurrentGroup();

            try
            {
                for (int i = 0; i < total; i++)
                {
                    if (i % 1024 == 0)
                    {
                        float p = (float)i / Mathf.Max(1, total);
                        if (EditorUtility.DisplayCancelableProgressBar("spawning", $"{i}/{total}", p))
                        {
                            canceled = true;
                            break;
                        }
                    }

                    GameObject go = keepPrefabConnection
                        ? (GameObject)PrefabUtility.InstantiatePrefab(prefab)
                        : Instantiate(prefab);

                    if (go == null) continue;

                    if (go.scene != targetScene)
                        SceneManager.MoveGameObjectToScene(go, targetScene);

                    if (registerUndo) Undo.RegisterCreatedObjectUndo(go, undoName);

                    if (actualParent != null)
                        go.transform.SetParent(actualParent, true);

                    go.transform.SetPositionAndRotation((Vector3)positions[i], (Quaternion)rotations[i]);
                    go.transform.localScale = (Vector3)scales[i];

                    spawned++;
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();

                if (spawned > 0 && targetScene.IsValid() && targetScene.isLoaded)
                    EditorSceneManager.MarkSceneDirty(targetScene);
            }
        }
        finally
        {
            if (positions.IsCreated) positions.Dispose();
            if (rotations.IsCreated) rotations.Dispose();
            if (scales.IsCreated) scales.Dispose();

            FinalizeTargetScene(targetScene, openedByTool, canceled);
        }
    }

    Scene ResolveTargetScene(out bool openedByTool)
    {
        openedByTool = false;

        if (useTargetSceneAsset && targetSceneAsset != null)
        {
            string path = AssetDatabase.GetAssetPath(targetSceneAsset);
            if (!string.IsNullOrEmpty(path))
            {
                if (TryGetLoadedSceneByPath(path, out Scene loaded))
                    return loaded;

                openedByTool = true;
                return EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            }
        }

        if (parent != null)
            return parent.gameObject.scene;

        return SceneManager.GetActiveScene();
    }

    Transform ResolveParentForTargetScene(Scene targetScene)
    {
        Transform p = parent;

        if (p != null && p.gameObject.scene != targetScene)
            p = null;

        if (p == null && useTargetSceneAsset && targetSceneAsset != null && createRootInTargetScene && !string.IsNullOrEmpty(rootNameInTargetScene))
        {
            GameObject root = FindRootByName(targetScene, rootNameInTargetScene);
            if (root == null)
            {
                root = new GameObject(rootNameInTargetScene);
                SceneManager.MoveGameObjectToScene(root, targetScene);
                if (registerUndo) Undo.RegisterCreatedObjectUndo(root, "Create Grid Root");
            }
            p = root.transform;
        }

        return p;
    }

    void FinalizeTargetScene(Scene targetScene, bool openedByTool)
    {
        FinalizeTargetScene(targetScene, openedByTool, false);
    }

    void FinalizeTargetScene(Scene targetScene, bool openedByTool, bool canceled)
    {
        if (canceled) return;

        if (saveTargetScene && targetScene.IsValid() && targetScene.isLoaded && targetScene.isDirty)
        {
            EditorSceneManager.SaveScene(targetScene);
        }

        if (openedByTool && closeIfOpenedByTool && targetScene.IsValid() && targetScene.isLoaded)
        {
            EditorSceneManager.CloseScene(targetScene, true);
        }
    }

    static bool TryGetLoadedSceneByPath(string path, out Scene scene)
    {
        int n = SceneManager.sceneCount;
        for (int i = 0; i < n; i++)
        {
            var s = SceneManager.GetSceneAt(i);
            if (s.path == path)
            {
                scene = s;
                return true;
            }
        }

        scene = default;
        return false;
    }

    static GameObject FindRootByName(Scene scene, string name)
    {
        if (!scene.IsValid() || !scene.isLoaded) return null;
        var roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            if (roots[i] != null && roots[i].name == name)
                return roots[i];
        }
        return null;
    }

    void DeleteRootInTargetScene()
    {
        if (!useTargetSceneAsset || targetSceneAsset == null) return;
        if (string.IsNullOrEmpty(rootNameInTargetScene)) return;

        Scene targetScene = ResolveTargetScene(out bool openedByTool);

        try
        {
            GameObject root = FindRootByName(targetScene, rootNameInTargetScene);
            if (root == null) return;

            Undo.IncrementCurrentGroup();
            Undo.DestroyObjectImmediate(root);
            EditorSceneManager.MarkSceneDirty(targetScene);
        }
        finally
        {
            FinalizeTargetScene(targetScene, openedByTool, false);
        }
    }

    static void DeleteAllChildren(Transform p)
    {
        if (p == null) return;

        var toDelete = new System.Collections.Generic.List<GameObject>();
        foreach (Transform c in p)
            toDelete.Add(c.gameObject);

        if (toDelete.Count == 0) return;

        Undo.IncrementCurrentGroup();
        foreach (var go in toDelete)
            Undo.DestroyObjectImmediate(go);

        var scene = p.gameObject.scene;
        if (scene.IsValid() && scene.isLoaded)
            EditorSceneManager.MarkSceneDirty(scene);
    }
}
#endif
