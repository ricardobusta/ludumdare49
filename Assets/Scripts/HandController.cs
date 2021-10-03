using UnityEngine;

public class HandController : MonoBehaviour
{
    [SerializeField] private FpsInput input;
    [SerializeField] private Transform headTransform;
    [SerializeField] private LayerMask interactiveLayerMask;
    [SerializeField] private int interactiveLayer;
    [SerializeField] private int holdingLayer;
    [SerializeField] private Transform handTransform;
    [SerializeField] private MeshRenderer crosshair;

    private Item _holdingItem;
    private Material _crossHairMaterial;
    private bool _hadTargetSet;

    private void Start()
    {
        _crossHairMaterial = new Material(crosshair.material);
        crosshair.material = _crossHairMaterial;
    }

    public bool HasHoldingItem()
    {
        return _holdingItem != null;
    }

    public Item RemoveHoldingItem(Transform newParent)
    {
        var item = _holdingItem;
        _holdingItem = null;
        item.SetParent(newParent, interactiveLayer);
        return item;
    }

    public void SetHoldingItem(Item newItem)
    {
        _holdingItem = newItem;
        newItem.SetParent(handTransform, holdingLayer);
    }

    private void Update()
    {
        Debug.DrawRay(headTransform.position, headTransform.forward * 3);
        if (Physics.Raycast(headTransform.position, headTransform.forward, out var hit, 3, interactiveLayerMask,
            QueryTriggerInteraction.Ignore))
        {
            var interactive = hit.collider.GetComponent<InteractiveObject>();
            if (interactive)
            {
                _hadTargetSet = true;
                _crossHairMaterial.color = Color.yellow;
                if (input.GetInteract())
                {
                    interactive.Interact(this);
                }

                return;
            }
        }

        if (_hadTargetSet)
        {
            _hadTargetSet = false;
            _crossHairMaterial.color = Color.gray;
        }
    }
}