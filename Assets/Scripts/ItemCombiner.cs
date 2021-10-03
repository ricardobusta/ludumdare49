using DefaultNamespace;
using UnityEngine;

public class ItemCombiner : InteractiveObject
{
    [SerializeField] private ItemCombiner combinerA;
    [SerializeField] private ItemCombiner combinerB;
    [SerializeField] private GameObject trigger;

    public override void Interact(HandController controller)
    {
        throw new System.NotImplementedException();
    }
}
