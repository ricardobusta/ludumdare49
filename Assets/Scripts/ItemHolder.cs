using UnityEngine;

public class ItemHolder : InteractiveObject
{
    [SerializeField] private Transform holderPosition;

    public Item holdingItem;

    public bool HasHoldingItem()
    {
        return holdingItem != null;
    }
    
    public bool HasHoldingItem(Item.ItemType type)
    {
        return HasHoldingItem() && holdingItem.type == type;
    }

    public void ClearItem()
    {
        if (holdingItem != null)
        {
            objectDescription = string.Empty;
            objectName = string.Empty;
            Destroy(holdingItem.gameObject);
        }
    }

    public void UpdateText()
    {
        if (holdingItem == null)
        {
            objectDescription = string.Empty;
            objectName = string.Empty;
        }
        else
        {
            objectDescription = holdingItem.objectDescription;
            objectName = holdingItem.objectName;
        }
    }

    public Item SpawnItem(Item prefab)
    {
        ClearItem();

        holdingItem = Instantiate(prefab, holderPosition);
        var tr = holdingItem.transform;
        tr.localPosition = Vector3.zero;
        tr.localScale = Vector3.one;
        tr.rotation = Quaternion.identity;
        return holdingItem;
    }

    public override void Interact(HandController controller)
    {
        var isHoldingItem = HasHoldingItem();
        var isHandItem = controller.HasHoldingItem();
        if (isHoldingItem && !isHandItem)
        {
            holdingItem.holder = null;
            controller.SetHoldingItem(holdingItem);
            holdingItem = null;
            controller.UpdateText(holdingItem);
            UpdateText();
        }
        else if (!isHoldingItem && isHandItem)
        {
            holdingItem = controller.RemoveHoldingItem(holderPosition);
            holdingItem.holder = this;
            controller.UpdateText(holdingItem);
            UpdateText();
        }
    }
}