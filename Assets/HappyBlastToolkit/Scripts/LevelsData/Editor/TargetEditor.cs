using UnityEditor;
using UnityEngine.UIElements;

[CanEditMultipleObjects]
public class TargetEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        return root;
    }

    private void MarkDirtyAndSave(TargetScriptable targetScriptable)
    {
        EditorUtility.SetDirty(targetScriptable);
        AssetDatabase.SaveAssets();
    }
}