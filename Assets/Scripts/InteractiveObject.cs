using UnityEngine;

public class InteractiveObject : MonoBehaviour
{
    public string objectName;
    public string objectDescription;

    public virtual void Interact(HandController controller)
    {
    }
}