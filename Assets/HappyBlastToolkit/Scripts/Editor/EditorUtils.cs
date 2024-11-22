using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

public static class EditorUtils
{
    public static Texture2D GetCanvasPreviewVisualElement<T>(T prefab, Action<T> action) where T : FillAndPreview
    {
        // Create a new preview render utility
        var previewRender = new PreviewRenderUtility
        {
            camera =
            {
                backgroundColor = Color.black,
                clearFlags = CameraClearFlags.SolidColor,
                cameraType = CameraType.Game,
                farClipPlane = 1000f,
                nearClipPlane = 0.1f
            }
        };


        var obj = previewRender.InstantiatePrefabInScene(prefab.gameObject);
        action.Invoke(obj.GetComponent<T>());
        var rect = obj.GetComponent<RectTransform>().rect;
        previewRender.BeginStaticPreview(new Rect(0.0f, 0.0f, rect.width, rect.height));
        var canvasInstance = obj.AddComponent<Canvas>();
        canvasInstance.renderMode = RenderMode.ScreenSpaceCamera;
        canvasInstance.worldCamera = previewRender.camera;

        var canvasScaler = obj.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        // Set the reference resolution
        canvasScaler.referenceResolution = new Vector2(1920, 1080); // Set your reference resolution here

        var scaleFactorX = Screen.width / canvasScaler.referenceResolution.x;
        var scaleFactorY = Screen.height / canvasScaler.referenceResolution.y;
        canvasScaler.scaleFactor = Mathf.Min(scaleFactorX, scaleFactorY) * 7;

        previewRender.Render();

        var texture = previewRender.EndStaticPreview();

        previewRender.camera.targetTexture = null;
        previewRender.Cleanup();
        return texture;
    }

    public static SerializedProperty GetPropertyFromValue(Object targetObject)
    {
        var serializedObject = new SerializedObject(targetObject);
        var property = serializedObject.GetIterator();

        // Go through each property in the object
        while (property.Next(true))
        {
            // Skip properties with child properties (e.g., arrays, structs)
            if (property.hasVisibleChildren)
            {
                continue;
            }

            // Check if the property value matches the desired field value
            // if (fieldValue.Equals(GetFieldValue(targetObject, property.name)))
            {
                // Create a copy of the property
                var copiedProperty = property.Copy();
                // Make sure the serializedObject is up to date
                copiedProperty.serializedObject.Update();
                // Apply the modified properties
                copiedProperty.serializedObject.ApplyModifiedProperties();
                return copiedProperty;
            }
        }

        return null; // Field value not found
    }

    public static object GetFieldValue(Object targetObject, string fieldName)
    {
        var field = targetObject.GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field != null)
        {
            return field.GetValue(targetObject);
        }

        Debug.LogError($"Field {fieldName} not found in object {targetObject.GetType().Name}");
        return null;
    }

    public static VisualElement GetObjectFields(SerializedObject serializedObject,
        Action<SerializedProperty> onChange = null)
    {
        var visualElement = new VisualElement();
        // Iterate through the fields of the Icon class
        var iterator = serializedObject.GetIterator();
        var enterChildren = true;
        while (iterator.NextVisible(enterChildren))
        {
            // Exclude the "m_Script" field
            if (iterator.name == "m_Script")
            {
                continue;
            }

            // Create a PropertyField for each field
            var propertyField = new PropertyField(iterator.Copy());
            propertyField.Bind(serializedObject);
            propertyField.style.flexShrink = 0;
            propertyField.style.flexGrow = 0;
            propertyField.style.width = 400;
            propertyField.RegisterValueChangeCallback(evt => { onChange?.Invoke(evt.changedProperty); });

            visualElement.Add(propertyField);
            enterChildren = false;
        }

        return visualElement;
    }

    public static Vector2 GetAbsolutePosition(List<VisualElement> elements, VisualElement parent)
    {
        var position = Vector2.zero;
        foreach (var element in elements)
        {
            position += element.LocalToWorld(element.layout.position);
        }

        return parent.WorldToLocal(position / elements.Count);
    }
}