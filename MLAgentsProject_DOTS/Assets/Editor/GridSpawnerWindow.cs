// GridSpawnerWindow.cs
// place this script anywhere; unity will compile editor parts only inside the editor

using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public static class GridSpawnerMath
{
    // simple per-index seed scrambler; avoids zero seed
    public static uint MakeSeed(uint baseSeed, int index)
    {
        uint s = (uint)index * 747796405u ^ (baseSeed * 196613u) ^ 0x9E3779B9u;
        return s == 0u ? 1u : s;
    }
}

[BurstCompile]
public struct BuildTransformsJob : IJobParallelFor
{
    // grid
    public int countX, countY, countZ;
    public float3 spacing;
    public float3 origin;
    public bool centerToOrigin;

    // random
    public uint baseSeed;

    // rotation ranges in degrees
    public float2 rotXDeg; // min,max
    public float2 rotYDeg;
    public float2 rotZDeg;
    public bool randomizeRotation;

    // scale ranges
    public bool randomizeScale;
    public bool uniformScale;
    public float2 uniformScaleRange;       // min,max
    public float2 scaleXRange;             // min,max
    public float2 scaleYRange;
    public float2 scaleZRange;

    // outputs
    [WriteOnly] public NativeArray<float3> outPositions;
    [WriteOnly] public NativeArray<quaternion> outRotations;
    [WriteOnly] public NativeArray<float3> outScales;

    public void Execute(int index)
    {
        // decode 3d index (x fastest, then z, then y)
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
            float3 eRad = math.radians(eDeg);
            r = quaternion.EulerXYZ(eRad);
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
    // input
    GameObject prefab;
    Transform parent;

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

    const int HardCap = 500000; // safety guard

    [MenuItem("Tools/Grid Spawner")]
    public static void Open()
    {
        GetWindow<GridSpawnerWindow>("Grid Spawner");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("prefab", EditorStyles.boldLabel);
        prefab = (GameObject)EditorGUILayout.ObjectField(prefab, typeof(GameObject), false);
        parent = (Transform)EditorGUILayout.ObjectField("parent (optional)", parent, typeof(Transform), true);
        keepPrefabConnection = EditorGUILayout.Toggle("keep prefab connection", keepPrefabConnection);
        registerUndo = EditorGUILayout.Toggle("register undo", registerUndo);

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
            GUI.enabled = prefab != null && total > 0 && total <= HardCap;
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
        }

        EditorGUILayout.HelpBox("spawns are computed with a burst job then instantiated on main thread. use a parent to keep your hierarchy clean.", MessageType.Info);
    }

    static int SafeMul(int a, int b)
    {
        long v = (long)a * (long)b;
        return (int)Mathf.Clamp(v, 0, int.MaxValue);
    }

    void Spawn(int total)
    {
        if (prefab == null) return;

        int workerCount = Unity.Jobs.LowLevel.Unsafe.JobsUtility.JobWorkerCount;
        int batchesPerWorker = 4; // small number keeps scheduling overhead low
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
                            break;
                        }
                    }

                    GameObject go = keepPrefabConnection
                        ? (GameObject)PrefabUtility.InstantiatePrefab(prefab)
                        : Instantiate(prefab);

                    if (go == null) continue;

                    if (registerUndo) Undo.RegisterCreatedObjectUndo(go, undoName);

                    if (parent != null) go.transform.SetParent(parent, true);

                    go.transform.SetPositionAndRotation((Vector3)positions[i], (Quaternion)rotations[i]);
                    go.transform.localScale = (Vector3)scales[i];
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                EditorSceneManager.MarkAllScenesDirty();
            }
        }
        finally
        {
            if (positions.IsCreated) positions.Dispose();
            if (rotations.IsCreated) rotations.Dispose();
            if (scales.IsCreated) scales.Dispose();
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
        EditorSceneManager.MarkAllScenesDirty();
    }
}
#endif
