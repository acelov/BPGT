using UnityEditor;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(IconPreviewAttribute))]
public class IconDrawer : PropertyDrawer
{
    private Label m_Icon;
    private ScriptableData m_IconScriptable;

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        m_Icon = new Label
        {
            style =
            {
                width = 200,
                height = 200
            }
        };

        // get parent of the property
        m_IconScriptable = property.serializedObject.targetObject as ScriptableData;
        if (m_IconScriptable != null)
        {
            m_IconScriptable.OnChange += UpdatePreview;
        }

        UpdatePreview();
        return m_Icon;
    }

    private void UpdatePreview()
    {
        EditorApplication.delayCall += () =>
        {
            m_Icon.style.backgroundImage = EditorUtils.GetCanvasPreviewVisualElement(m_IconScriptable.prefab,
                obj => obj.FillIcon(m_IconScriptable));
        };
    }
}