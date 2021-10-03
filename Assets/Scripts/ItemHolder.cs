using UnityEngine;

public class ItemHolder : InteractiveObject
{
    [SerializeField] private Transform holderPosition;

    private Item holdingItem;

    public override void Interact(HandController controller)
    {
        var isHoldingItem = holdingItem != null;
        var isHandItem = controller.HasHoldingItem();
        if (isHoldingItem && !isHandItem)
        {
            controller.SetHoldingItem(holdingItem);
            holdingItem = null;
        }
        else if (!isHoldingItem && isHandItem)
        {
            holdingItem = controller.RemoveHoldingItem(holderPosition);
        }
    }
}