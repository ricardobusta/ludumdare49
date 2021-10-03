using UnityEngine;

public abstract class InteractiveObject : MonoBehaviour
{
    public abstract void Interact(HandController controller);
}