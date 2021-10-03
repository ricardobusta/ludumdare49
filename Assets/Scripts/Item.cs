using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] private Material materialPrefab;
    [SerializeField] private MeshRenderer renderer;

    public float c;
    public float m;
    public float y;
    public float k;
    public float a;

    private void Start()
    {
        renderer.material = new Material(materialPrefab);
        SetColor(c, m, y, k, a);
    }

    public void SetParent(Transform parent, int layer)
    {
        var tr = transform;
        tr.SetParent(parent);
        tr.localPosition = Vector3.zero;
        tr.localRotation = Quaternion.identity;
        tr.localScale = Vector3.one;
        // tr.gameObject.layer = layer;
        // foreach (var r in GetComponentsInChildren<Renderer>(includeInactive: true))
        // {
        //     r.gameObject.layer = layer;
        // }
    }

    public Color GetColor()
    {
        var invK = 1 - k;
        return new Color((1 - c) * invK, (1 - m) * invK, (1 - y) * invK, a);
    }

    public void SetColor(float c, float m, float y, float k, float a)
    {
        this.c = c;
        this.m = m;
        this.y = y;
        this.k = k;
        this.a = a;

        renderer.material.color = GetColor();
    }
}