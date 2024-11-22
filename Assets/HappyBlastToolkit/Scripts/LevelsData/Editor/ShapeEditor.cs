using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(ShapeTemplate))]
public class ShapeEditor : Editor
{
    private const int GridSize = 5;
    private VisualElement gridContainer;
    private ShapeTemplate[] shapes;
    private int currentIndex;
    private ShapeTemplate _target;

    public override VisualElement CreateInspectorGUI()
    {
        _target = (ShapeTemplate)target;

        var root = new VisualElement();

        // Load and apply USS
        var styleSheet =
            AssetDatabase.LoadAssetAtPath<StyleSheet>(PathConstants.ShapeEditorStyles);
        root.styleSheets.Add(styleSheet);

        root.Add(new Label(_target.name) { name = "title" });

        var navContainer = new VisualElement { name = "nav-container" };
        navContainer.Add(CreateButton("<<", () => NavigateShapes(-1)));
        navContainer.Add(CreateButton(">>", () => NavigateShapes(1)));
        navContainer.Add(CreateButton("+", AddShape));
        navContainer.Add(CreateButton("-", RemoveShape));
        root.Add(navContainer);

        var actionContainer = new VisualElement { name = "action-container" };
        actionContainer.Add(CreateButton("Clear All", ClearAll));
        actionContainer.Add(CreateButton("Save", Save));
        root.Add(actionContainer);

        root.Add(new Label("Click on a square to add/remove a block") { name = "instructions" });

        var classicModeContainer = new VisualElement { name = "classic-mode-container" };
        classicModeContainer.AddToClassList("classic-mode-box");
        classicModeContainer.Add(new Label("Classic mode parameters") { name = "classic-mode-title" });

        var scoreField = new IntegerField("Score for Spawn") { value = _target.scoreForSpawn };
        scoreField.AddToClassList("classic-mode-field");
        scoreField.RegisterValueChangedCallback(evt =>
        {
            _target.scoreForSpawn = evt.newValue;
            EditorUtility.SetDirty(_target);
        });
        classicModeContainer.Add(scoreField);


        gridContainer = new VisualElement { name = "grid-container" };
        root.Add(gridContainer);

        var sliderContainer = new VisualElement
        {
            name = "slider-container",
            style =
            {
                flexDirection = FlexDirection.Row
            }
        };
        var chanceField = new Slider("Chance for Spawn", 0, 1) { value = _target.chanceForSpawn };
        //value of slider
        var chanceValue = new FloatField
        {
            value = _target.chanceForSpawn,
            style =
            {
                marginLeft = 10
            }
        };
        chanceValue.RegisterValueChangedCallback(evt => { chanceField.value = evt.newValue; });
        chanceField.RegisterValueChangedCallback(evt =>
        {
            chanceValue.value = evt.newValue;
            _target.chanceForSpawn = evt.newValue;
        });
        chanceField.AddToClassList("classic-mode-field");
        chanceField.style.width = 200;
        chanceField.RegisterValueChangedCallback(evt =>
        {
            _target.chanceForSpawn = evt.newValue;
            EditorUtility.SetDirty(_target);
        });
        sliderContainer.Add(chanceField);
        sliderContainer.Add(chanceValue);
        root.Add(sliderContainer);

        root.Add(classicModeContainer);

        var adventureModeContainer = new VisualElement { name = "classic-mode-container" };
        adventureModeContainer.AddToClassList("classic-mode-box");
        adventureModeContainer.Add(new Label("Adventure mode parameters") { name = "adventure-mode-title" });

        var spawnFromLevel = new IntegerField("spawn from level") { value = _target.spawnFromLevel };
        spawnFromLevel.AddToClassList("adventure-mode-field");
        spawnFromLevel.RegisterValueChangedCallback(evt =>
        {
            _target.spawnFromLevel = evt.newValue;
            EditorUtility.SetDirty(_target);
        });
        adventureModeContainer.Add(spawnFromLevel);
        root.Add(adventureModeContainer);

        LoadShapes();
        CreateGrid();

        return root;
    }

    private void LoadShapes()
    {
        shapes = Resources.LoadAll<ShapeTemplate>($"Shapes");
        currentIndex = shapes.ToList().IndexOf(_target);
        if (currentIndex == -1 && shapes.Length > 0)
        {
            currentIndex = 0;
            Selection.activeObject = shapes[currentIndex];
        }
    }

    private void NavigateShapes(int direction)
    {
        Save();
        currentIndex = (currentIndex + direction + shapes.Length) % shapes.Length;
        Selection.activeObject = shapes[currentIndex];
        _target = shapes[currentIndex];
        CreateGrid();
    }

    private void AddShape()
    {
        Save();
        var path = AssetDatabase.GetAssetPath(_target);
        path = string.IsNullOrEmpty(path) ? PathConstants.Shapes : Path.GetDirectoryName(path);

        var newShapeTemplate = CreateInstance<ShapeTemplate>();
        for (var i = 0; i < GridSize; i++)
        {
            newShapeTemplate.rows[i] = new ShapeRow();
        }

        var assetPath = AssetDatabase.GenerateUniqueAssetPath($"{path}/NewShape.asset");
        AssetDatabase.CreateAsset(newShapeTemplate, assetPath);
        AssetDatabase.SaveAssets();

        LoadShapes();
        currentIndex = shapes.Length - 1;
        Selection.activeObject = newShapeTemplate;
        _target = newShapeTemplate;
        CreateGrid();
    }

    private void RemoveShape()
    {
        if (shapes.Length <= 1)
        {
            return;
        }

        var path = AssetDatabase.GetAssetPath(_target);
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.SaveAssets();

        LoadShapes();
        currentIndex = Mathf.Clamp(currentIndex, 0, shapes.Length - 1);
        Selection.activeObject = shapes[currentIndex];
        _target = shapes[currentIndex];
        CreateGrid();
    }

    private void CreateGrid()
    {
        gridContainer.Clear();
        for (var i = 0; i < GridSize; i++)
        {
            _target.rows[i] ??= new ShapeRow();

            var row = new VisualElement();
            row.AddToClassList("grid-row");
            for (var j = 0; j < GridSize; j++)
            {
                var cell = new Button();
                cell.AddToClassList("grid-cell");
                cell.AddToClassList(_target.rows[i].cells[j] ? "active" : "inactive");
                int x = i, y = j;
                cell.clicked += () =>
                {
                    _target.rows[x].cells[y] = !_target.rows[x].cells[y];
                    cell.ToggleInClassList("active");
                    cell.ToggleInClassList("inactive");
                    EditorUtility.SetDirty(_target);
                };

                row.Add(cell);
            }

            gridContainer.Add(row);
        }
    }

    private Button CreateButton(string text, Action clickEvent)
    {
        var button = new Button(clickEvent)
        {
            text = text,
            style =
            {
                flexGrow = 1
            }
        };
        return button;
    }

    private void ClearAll()
    {
        for (var i = 0; i < GridSize; i++)
        {
            for (var j = 0; j < GridSize; j++)
            {
                _target.rows[i].cells[j] = false;
            }
        }

        EditorUtility.SetDirty(_target);
        CreateGrid();
    }

    private void Save()
    {
        EditorUtility.SetDirty(_target);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}