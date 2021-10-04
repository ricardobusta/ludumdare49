using UnityEngine;

public class Item : InteractiveObject
{
    [SerializeField] private Material materialPrefab;
    [SerializeField] private new MeshRenderer renderer;
    public ItemType type;

    public CmykColor color;
    public InteractiveObject holder;

    private void Start()
    {
        renderer.material = new Material(materialPrefab);
        if (type == ItemType.Potion || type == ItemType.Flask)
        {
            SetPotion(color);
        }
        else
        {
            SetColor(color);
        }
    }

    public void SetParent(Transform parent, int layer)
    {
        var tr = transform;
        tr.SetParent(parent);
        tr.localPosition = Vector3.zero;
        tr.localRotation = Quaternion.identity;
        tr.localScale = Vector3.one;
        tr.gameObject.layer = layer;
        foreach (var r in GetComponentsInChildren<Renderer>(includeInactive: true))
        {
            r.gameObject.layer = layer;
        }
    }

    public void SetPotion(CmykColor newColor)
    {
        SetColor(newColor);
        if (newColor.a == 0)
        {
            type = ItemType.Flask;
            objectName = "Flask";
            objectDescription = "Empty bottle that can be filled with liquid.";
        }
        else
        {
            type = ItemType.Potion;
            objectName = "Potion";
            objectDescription = $"Color {newColor}";
        }
    }

    public void SetColor(CmykColor newColor)
    {
        this.color = newColor;
        renderer.material.color = newColor.ToRgba();
    }

    public enum ItemType
    {
        Ingredient,
        Potion,
        Flask
    }

    public override void Interact(HandController controller)
    {
        holder.Interact(controller);
    }
}