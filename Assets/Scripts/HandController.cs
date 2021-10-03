using DefaultNamespace;
using UnityEngine;

public class HandController : MonoBehaviour
{
    [SerializeField] private FpsInput input;
    [SerializeField] private Transform headTransform;
    [SerializeField] private int interactiveLayer;
    [SerializeField] private int holdingLayer;
    [SerializeField] private Transform handTransform;

    private Item _holdingItem;

    public bool HasHoldingItem()
    {
        return _holdingItem != null;
    }

    public Item RemoveHoldingItem()
    {
        var item = _holdingItem;
        _holdingItem = null;
        item.SetParent(null, interactiveLayer);
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
        if (Physics.Raycast(headTransform.position, headTransform.forward, out var hit, 3, interactiveLayer,
            QueryTriggerInteraction.Ignore))
        {
            var interactive = hit.collider.GetComponent<InteractiveObject>();
            if (interactive)
            {
                if (input.GetInteract())
                {
                    interactive.Interact(this);
                    return;
                }
            }
        }
    }
}