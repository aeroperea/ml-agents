using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// converts selected materials to flatkit stylized surface in-place
public class ConvertToFlatKitStylizedSurface : EditorWindow
{
    [MenuItem("Tools/flatkit/convert selected materials to stylized surface")]
    public static void ShowWindow() => GetWindow<ConvertToFlatKitStylizedSurface>("flatkit converter");

    void OnGUI()
    {
        GUILayout.Label("Convert Selected Materials to FlatKit Stylized Surface", EditorStyles.boldLabel);
        if (GUILayout.Button("Convert Selected"))
            ConvertSelectedMaterials();
    }

    private static void ConvertSelectedMaterials()
    {
        // load the stylized surface shader
        var stylizedShader = AssetDatabase.LoadAssetAtPath<Shader>(
            "Assets/FlatKit/Shaders/StylizedSurface/StylizedSurface.shader");

        if (stylizedShader == null)
        {
            Debug.LogError("stylized surface shader not found at Assets/FlatKit/Shaders/StylizedSurface/StylizedSurface.shader");
            return;
        }

        // define semantic mapping: target property -> source name patterns
        var semanticMap = new Dictionary<string, string[]> {
            {"_BaseMap",     new[]{".*base.*",".*albedo.*",".*diffuse.*",".*maintex.*",".*color.*"}},
            {"_BumpMap",     new[]{".*normal.*",".*nm.*",".*nmap.*"}},
            {"_EmissionMap", new[]{".*emission.*",".*glow.*"}},
            {"_DetailMap",   new[]{".*detail.*"}},
            {"_BaseColor",   new[]{".*color.*",".*tint.*",".*albedo.*"}},
            {"_ColorDim",    new[]{".*shade.*",".*dim.*"}},
            // add more mappings here if needed
        };

        // process each selected material
        foreach (var obj in Selection.objects)
        {
            if (!(obj is Material mat))
                continue;

            Undo.RecordObject(mat, "convert to stylized surface");

            var srcShader = mat.shader;
            var srcNames = new List<string>();
            int propCount = ShaderUtil.GetPropertyCount(srcShader);
            for (int i = 0; i < propCount; i++)
                srcNames.Add(ShaderUtil.GetPropertyName(srcShader, i));

            // capture values from source
            var texValues = new Dictionary<string, Texture>();
            var colValues = new Dictionary<string, Color>();
            var floatValues = new Dictionary<string, float>();

            foreach (var kv in semanticMap)
            {
                var destProp = kv.Key;
                Texture texVal = null;
                Color colVal = default;
                float fVal = 0f;
                bool got = false;

                foreach (var pat in kv.Value)
                {
                    var rx = new Regex(pat, RegexOptions.IgnoreCase);
                    var match = srcNames.Find(n => rx.IsMatch(n));
                    if (match == null) continue;

                    int idx = srcNames.IndexOf(match);
                    var type = ShaderUtil.GetPropertyType(srcShader, idx);

                    switch (type)
                    {
                        case ShaderUtil.ShaderPropertyType.TexEnv:
                            texVal = mat.GetTexture(match);
                            got = texVal != null;
                            break;
                        case ShaderUtil.ShaderPropertyType.Color:
                            colVal = mat.GetColor(match);
                            got = true;
                            break;
                        case ShaderUtil.ShaderPropertyType.Float:
                        case ShaderUtil.ShaderPropertyType.Range:
                            fVal = mat.GetFloat(match);
                            got = true;
                            break;
                    }
                    if (got) break;
                }

                if (!got) continue;
                if (texVal != null) texValues[destProp] = texVal;
                else if (!colVal.Equals(default(Color))) colValues[destProp] = colVal;
                else floatValues[destProp] = fVal;
            }

            // switch shader
            mat.shader = stylizedShader;

            // apply captured values
            foreach (var kv2 in texValues) mat.SetTexture(kv2.Key, kv2.Value);
            foreach (var kv2 in colValues) mat.SetColor(kv2.Key, kv2.Value);
            foreach (var kv2 in floatValues) mat.SetFloat(kv2.Key, kv2.Value);

            EditorUtility.SetDirty(mat);
            Debug.Log($"converted {mat.name} to stylized surface: applied {texValues.Count + colValues.Count + floatValues.Count} properties");
        }

        AssetDatabase.SaveAssets();
    }
}
