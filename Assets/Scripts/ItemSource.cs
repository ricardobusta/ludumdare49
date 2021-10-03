using UnityEngine;

public class ItemSource : InteractiveObject
{
    [SerializeField] private Item itemPrefab;

    public override void Interact(HandController controller)
    {
        if (controller.HasHoldingItem())
        {
            return;
        }

        var newItem = Instantiate(itemPrefab);
        controller.SetHoldingItem(newItem);
    }
}