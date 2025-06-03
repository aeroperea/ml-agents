using UnityEditor;
using UnityEngine;

public class EnvironmentSpawnerEditor : EditorWindow
{
    private GameObject environmentPrefab;
    private int rows = 3;
    private int columns = 3;
    private float xOffset = 10f;
    private float zOffset = 10f;
    private Vector3 bias = Vector3.zero;
    private Transform parentTransform;

    [MenuItem("Tools/ECS Environment Spawner")]
    public static void ShowWindow()
    {
        GetWindow<EnvironmentSpawnerEditor>("ECS Environment Spawner");
    }

    private void OnGUI()
    {
        GUILayout.Label("Environment Spawner", EditorStyles.boldLabel);

        environmentPrefab = (GameObject)EditorGUILayout.ObjectField("Environment Prefab", environmentPrefab, typeof(GameObject), false);
        parentTransform = (Transform)EditorGUILayout.ObjectField("Parent Transform", parentTransform, typeof(Transform), true);

        rows = EditorGUILayout.IntField("Rows", rows);
        columns = EditorGUILayout.IntField("Columns", columns);
        xOffset = EditorGUILayout.FloatField("X Offset", xOffset);
        zOffset = EditorGUILayout.FloatField("Z Offset", zOffset);
        bias = EditorGUILayout.Vector3Field("Bias", bias);

        if (GUILayout.Button("Spawn Environments"))
        {
            SpawnEnvironments();
        }

        if (GUILayout.Button("Clear Environments"))
        {
            ClearEnvironments();
        }
    }

    private void SpawnEnvironments()
    {
        if (environmentPrefab == null)
        {
            Debug.LogError("Environment Prefab is not assigned!");
            return;
        }

        GameObject parent = parentTransform != null ? parentTransform.gameObject : new GameObject("Spawned Environments");

        Undo.RegisterCreatedObjectUndo(parent, "Create Environment Parent");

        for (int x = 0; x < rows; x++)
        {
            for (int z = 0; z < columns; z++)
            {
                Vector3 position = new Vector3(
                    x * xOffset + bias.x,
                    bias.y,
                    z * zOffset + bias.z
                );

                GameObject newEnv = PrefabUtility.InstantiatePrefab(environmentPrefab) as GameObject;
                if (newEnv != null)
                {
                    Undo.RegisterCreatedObjectUndo(newEnv, "Spawn Environment");
                    newEnv.transform.position = position;
                    newEnv.transform.parent = parent.transform;
                }
            }
        }

        Debug.Log($"Spawned {rows * columns} environments.");
    }

    private void ClearEnvironments()
    {
        if (parentTransform == null)
        {
            Debug.LogWarning("No parent transform assigned! Select the correct parent before clearing.");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(parentTransform.gameObject, "Clear Environments");

        for (int i = parentTransform.childCount - 1; i >= 0; i--)
        {
            Undo.DestroyObjectImmediate(parentTransform.GetChild(i).gameObject);
        }

        Debug.Log("Cleared all spawned environments.");
    }
}
