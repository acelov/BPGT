using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class Item : FillAndPreview
{
    public ItemTemplate itemTemplate;
    public Image backgroundColor;
    public Image underlayColor;
    public Image bottomColor;
    public Image topColor;
    public Image leftColor;
    public Image rightColor;
    public Image overlayColor;
    private Vector2Int position;
    public Bonus bonus;
    public BonusItemTemplate bonusItemTemplate;

    private void Awake()
    {
        bonus?.gameObject.SetActive(false);
        if (itemTemplate != null)
        {
            UpdateColor(itemTemplate);
        }
    }

    public void UpdateColor(ItemTemplate itemTemp)
    {
        this.itemTemplate = itemTemp;
        backgroundColor.color = itemTemp.backgroundColor;
        underlayColor.color = itemTemp.underlayColor;
        bottomColor.color = itemTemp.bottomColor;
        topColor.color = itemTemp.topColor;
        leftColor.color = itemTemp.leftColor;
        rightColor.color = itemTemp.rightColor;
        overlayColor.color = itemTemp.overlayColor;
    }

    public void SetBonus(BonusItemTemplate template)
    {
        bonusItemTemplate = template;
        bonus.gameObject.SetActive(true);
        bonus.FillIcon(template);
    }

    public override void FillIcon(ScriptableData iconScriptable)
    {
        UpdateColor((ItemTemplate)iconScriptable);
    }

    public void SetPosition(Vector2Int vector2Int)
    {
        position = vector2Int;
    }

    public Vector2Int GetPosition()
    {
        return position;
    }

    public bool HasBonusItem()
    {
        return bonusItemTemplate != null;
    }

    public void ClearBonus()
    {
        bonusItemTemplate = null;
        bonus.gameObject.SetActive(false);
    }

    public void SetTransparency(float alpha)
    {
        var color = backgroundColor.color;
        color.a = alpha;
        backgroundColor.color = color;

        color = underlayColor.color;
        color.a = alpha;
        underlayColor.color = color;

        color = bottomColor.color;
        color.a = alpha;
        bottomColor.color = color;

        color = topColor.color;
        color.a = alpha;
        topColor.color = color;

        color = leftColor.color;
        color.a = alpha;
        leftColor.color = color;

        color = rightColor.color;
        color.a = alpha;
        rightColor.color = color;

        color = overlayColor.color;
        color.a = alpha;
        overlayColor.color = color;

        bonus?.SetTransparency(alpha);
    }
}