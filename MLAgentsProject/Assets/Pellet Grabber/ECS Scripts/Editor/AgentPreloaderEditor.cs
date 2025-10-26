using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AgentLinkManager))]
public class AgentLinkManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI(); // Draw default Inspector fields

        AgentLinkManager script = (AgentLinkManager)target;

        if (GUILayout.Button("Preload Agents in Editor"))
        {
            script.PreloadAgents(); // Call preloading method
        }
    }
}
