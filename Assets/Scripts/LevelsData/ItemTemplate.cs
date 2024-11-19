using UnityEngine;

[CreateAssetMenu(fileName = "ItemTemplate", menuName = "BlockPuzzleGameToolkit/Items/ItemTemplate", order = 1)]
public class ItemTemplate : ScriptableData
{
    public Color backgroundColor;
    public Color underlayColor;
    public Color bottomColor;
    public Color topColor;
    public Color leftColor;
    public Color rightColor;
    public Color overlayColor;
}