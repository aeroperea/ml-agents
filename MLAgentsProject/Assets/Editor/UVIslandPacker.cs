using UnityEngine;
using UnityEditor;

public class UVIslandPacker : EditorWindow
{
    GameObject targetGO;
    int submeshIndex;
    string[] submeshNames = new string[0];
    bool cloneMesh;
    int cellsX = 30, cellsY = 4, targetX = 1, targetY = 2;
    float cellSizeModifier = 0.5f, unwrapMargin = 0.001f;
    Texture2D paletteTexture;

    [MenuItem("Tools/uv island packer")]
    static void Open() => GetWindow<UVIslandPacker>("uv island packer");

    void OnGUI()
    {
        // pick the mesh object
        targetGO = (GameObject)EditorGUILayout.ObjectField("mesh object",
                                                           targetGO,
                                                           typeof(GameObject),
                                                           true);

        if (targetGO != null)
        {
            var renderer = targetGO.GetComponent<Renderer>();
            if (renderer != null && renderer.sharedMaterials != null)
            {
                var mats = renderer.sharedMaterials;
                if (submeshNames.Length != mats.Length)
                {
                    submeshNames = new string[mats.Length];
                    for (int i = 0; i < mats.Length; i++)
                        submeshNames[i] = mats[i] != null
                                         ? mats[i].name
                                         : $"<null {i}>";
                }
                submeshIndex = EditorGUILayout.Popup("submesh",
                                                     submeshIndex,
                                                     submeshNames);
            }
            // option to clone the mesh asset
            cloneMesh = EditorGUILayout.Toggle("clone mesh", cloneMesh);
        }

        cellsX = EditorGUILayout.IntField("cells x", cellsX);
        cellsY = EditorGUILayout.IntField("cells y", cellsY);
        targetX = EditorGUILayout.IntField("cell x", targetX);
        targetY = EditorGUILayout.IntField("cell y", targetY);
        cellSizeModifier = EditorGUILayout.FloatField("cell size mod", cellSizeModifier);
        unwrapMargin = EditorGUILayout.FloatField("unwrap margin", unwrapMargin);

        // palette texture picker
        paletteTexture = (Texture2D)EditorGUILayout.ObjectField("palette texture",
                                                                paletteTexture,
                                                                typeof(Texture2D),
                                                                false);

        // preview section
        EditorGUILayout.LabelField("palette preview", EditorStyles.boldLabel);
        if (paletteTexture == null)
        {
            EditorGUILayout.HelpBox("assign a palette texture to see preview", MessageType.Info);
        }
        else
        {
            string path = AssetDatabase.GetAssetPath(paletteTexture);
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti != null && !ti.isReadable)
            {
                EditorGUILayout.HelpBox("enable read/write in texture import settings", MessageType.Warning);
            }
            else
            {
                int px = Mathf.Clamp((int)((targetX + 0.5f) * paletteTexture.width / cellsX),
                                     0, paletteTexture.width - 1);
                int py = Mathf.Clamp((int)((targetY + 0.5f) * paletteTexture.height / cellsY),
                                     0, paletteTexture.height - 1);
                Color sample = paletteTexture.GetPixel(px, py);
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ColorField("preview color", sample);
                EditorGUI.EndDisabledGroup();
            }
        }

        if (GUILayout.Button("pack uv"))
            PackUV();
    }

    void PackUV()
    {
        if (targetGO == null)
        {
            Debug.LogError("please assign mesh object");
            return;
        }

        var mf = targetGO.GetComponent<MeshFilter>();
        var smr = targetGO.GetComponent<SkinnedMeshRenderer>();
        if (mf == null && smr == null)
        {
            Debug.LogError("no meshfilter or skinnedmeshrenderer on object");
            return;
        }

        Mesh mesh = mf != null ? mf.sharedMesh : smr.sharedMesh;
        if (mesh == null)
        {
            Debug.LogError("mesh is null");
            return;
        }

        // clone and save as new asset if requested
        if (cloneMesh)
        {
            Mesh original = mesh;
            Mesh newMesh = Instantiate(original);

            string savePath = EditorUtility.SaveFilePanelInProject(
                "save cloned mesh",
                original.name + "_packed",
                "asset",
                "choose save location for cloned mesh");

            if (string.IsNullOrEmpty(savePath))
            {
                Debug.LogError("save path invalid");
                return;
            }

            AssetDatabase.CreateAsset(newMesh, savePath);
            AssetDatabase.SaveAssets();

            mesh = newMesh;
            if (mf != null) mf.sharedMesh = mesh;
            if (smr != null) smr.sharedMesh = mesh;
        }

        Vector2[] uv = mesh.uv;
        if (uv == null || uv.Length == 0)
        {
            var p = new UnwrapParam { packMargin = unwrapMargin };
            Unwrapping.GenerateSecondaryUVSet(mesh, p);
            uv = mesh.uv2;
        }

        int[] tris = mesh.GetTriangles(submeshIndex);
        if (tris == null || tris.Length == 0)
        {
            Debug.LogError("no triangles in submesh");
            return;
        }

        float minU = 1, maxU = 0, minV = 1, maxV = 0;
        foreach (int idx in tris)
        {
            var w = uv[idx];
            minU = Mathf.Min(minU, w.x);
            maxU = Mathf.Max(maxU, w.x);
            minV = Mathf.Min(minV, w.y);
            maxV = Mathf.Max(maxV, w.y);
        }

        float wdt = maxU - minU, hgt = maxV - minV;
        if (wdt == 0 || hgt == 0)
        {
            Debug.LogError("uv island has zero area");
            return;
        }

        float cellW = 1f / cellsX, cellH = 1f / cellsY;
        float sU = cellW * cellSizeModifier / wdt;
        float sV = cellH * cellSizeModifier / hgt;
        float oU = targetX * cellW - minU * sU;
        float oV = targetY * cellH - minV * sV;

        for (int i = 0; i < tris.Length; i++)
        {
            int idx = tris[i];
            var w = uv[idx];
            w.x = w.x * sU + oU;
            w.y = w.y * sV + oV;
            uv[idx] = w;
        }

        mesh.uv = uv;

        if (!cloneMesh)
        {
            EditorUtility.SetDirty(mesh);
            AssetDatabase.SaveAssets();
        }

        Debug.Log($"packed submesh {submeshNames[submeshIndex]} into cell ({targetX},{targetY})");
    }
}
