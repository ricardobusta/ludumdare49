using UnityEngine;

public class ItemCombiner : InteractiveObject
{
    [SerializeField] private ItemCombiner combinerA;
    [SerializeField] private ItemCombiner combinerB;
    [SerializeField] private GameObject trigger;

    public enum Recipe
    {
        Make,
        Mix,
        Dilution,
        Concentration,
    }

    public override void Interact(HandController controller)
    {
        throw new System.NotImplementedException();
    }
}
