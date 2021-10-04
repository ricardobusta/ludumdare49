using DefaultNamespace;
using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] private Material materialPrefab;
    [SerializeField] private new MeshRenderer renderer;
    public ItemType type;

    public CmykColor color;

    private void Start()
    {
        renderer.material = new Material(materialPrefab);
        SetColor(color);
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

    public Color GetRgb()
    {
        var invK = 1 - color.k;
        return new Color((1 - color.c) * invK, (1 - color.m) * invK, (1 - color.y) * invK, color.a);
    }

    public void SetColor(CmykColor color)
    {
        this.color = color;
        renderer.material.color = GetRgb();
    }

    public enum ItemType
    {
        Ingredient,
        Potion,
        Flask
    }
}