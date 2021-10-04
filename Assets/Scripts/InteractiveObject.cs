using UnityEngine;

public abstract class InteractiveObject : MonoBehaviour
{
    public string objectName;
    public string objectDescription;
    
    public abstract void Interact(HandController controller);
}