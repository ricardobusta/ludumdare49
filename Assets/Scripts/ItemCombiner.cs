using DefaultNamespace;
using UnityEngine;

public class ItemCombiner : InteractiveObject
{
    [SerializeField] private ItemHolder combinerA;
    [SerializeField] private ItemHolder combinerB;
    [SerializeField] private Recipe recipe;

    [SerializeField] private Item potionPrefab;


    private enum Recipe
    {
        Extract,
        Combine,
        Dilute,
        Concentrate,
    }

    public override void Interact(HandController controller)
    {
        switch (recipe)
        {
            case Recipe.Extract:
                Extract();
                break;
            case Recipe.Combine:
                Combine();
                break;
            case Recipe.Dilute:
                Dilute();
                break;
            case Recipe.Concentrate:
                Concentrate();
                break;
        }
    }

    public void Extract()
    {
        if (combinerA.HasHoldingItem(Item.ItemType.Ingredient) && combinerB.HasHoldingItem(Item.ItemType.Flask))
        {
            combinerB.SpawnItem(potionPrefab).SetColor(combinerA.holdingItem.color);

            combinerA.ClearItem();
        }
    }

    public void Combine()
    {
        if (combinerA.HasHoldingItem(Item.ItemType.Potion) && combinerB.HasHoldingItem(Item.ItemType.Potion))
        {
            var c1 = combinerA.holdingItem.color;
            var c2 = combinerB.holdingItem.color;

            var c = new CmykColor
            {
                c = c1.c * c1.a + c2.c * c2.a,
                m = c1.m * c1.a + c2.m * c2.a,
                y = c1.y * c1.a + c2.y * c2.a,
                k = c1.k * c1.a + c2.k * c2.a,
                a = Mathf.Clamp01(c1.a + c2.a)
            };
            
            combinerA.holdingItem.SetColor(c);
            combinerB.holdingItem.SetColor(c);
        }
    }

    public void Dilute()
    {
        if (combinerA.HasHoldingItem(Item.ItemType.Potion))
        {
            var c = combinerA.holdingItem.color;
            c.a = 0.5f;
            
            combinerA.holdingItem.SetColor(c);
        }
    }

    public void Concentrate()
    {
        if (combinerA.HasHoldingItem(Item.ItemType.Potion))
        {
            var c = combinerA.holdingItem.color;
            c.a = 1;
            
            combinerA.holdingItem.SetColor(c);
        }
    }
}