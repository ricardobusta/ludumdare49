using UnityEngine;

namespace DefaultNamespace
{
    public class ItemDiscarder : InteractiveObject
    {
        public override void Interact(HandController controller)
        {
            if (!controller.HasHoldingItem())
            {
                return;
            }

            var item = controller.RemoveHoldingItem(null);
            Destroy(item.gameObject);
        }
    }
}