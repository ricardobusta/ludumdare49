using TMPro;
using UnityEngine;

public class HandController : MonoBehaviour
{
    [SerializeField] private FpsInput input;
    [SerializeField] private Transform headTransform;
    [SerializeField] private LayerMask interactiveLayerMask;
    [SerializeField] private int interactiveLayer;
    [SerializeField] private int holdingLayer;
    [SerializeField] private Transform handTransform;
    [SerializeField] private MeshRenderer crosshair;
    [SerializeField] private TMP_Text nameDisplay;
    [SerializeField] private TMP_Text descriptionDisplay;

    private Item _holdingItem;
    private Material _crossHairMaterial;
    private bool _hadTargetSet;
    private InteractiveObject _lastInteractedObject;

    private void Start()
    {
        _crossHairMaterial = new Material(crosshair.material);
        crosshair.material = _crossHairMaterial;
        nameDisplay.text = string.Empty;
        descriptionDisplay.text = string.Empty;
    }

    public bool HasHoldingItem()
    {
        return _holdingItem != null;
    }

    public bool HasHoldingItem(Item.ItemType type)
    {
        return HasHoldingItem() && _holdingItem.type == type;
    }

    public Item RemoveHoldingItem(Transform newParent)
    {
        var item = _holdingItem;
        _holdingItem = null;
        item.SetParent(newParent, interactiveLayer);
        return item;
    }

    public void SetHoldingItem(Item newItem)
    {
        _holdingItem = newItem;
        newItem.SetParent(handTransform, holdingLayer);
    }

    public void UpdateText(InteractiveObject interactive)
    {
        if (interactive == null)
        {
            descriptionDisplay.text = string.Empty;
            nameDisplay.text = string.Empty;
        }
        else
        {
            descriptionDisplay.text = interactive.objectDescription;
            nameDisplay.text = interactive.objectName;
        }
    }

    private void Update()
    {
        Debug.DrawRay(headTransform.position, headTransform.forward * 3);
        if (Physics.Raycast(headTransform.position, headTransform.forward, out var hit, 3, interactiveLayerMask,
            QueryTriggerInteraction.Ignore))
        {
            var interactive = hit.collider.GetComponent<InteractiveObject>();
            if (interactive)
            {
                _hadTargetSet = true;
                _crossHairMaterial.color = Color.yellow;

                if (interactive != _lastInteractedObject)
                {
                    _lastInteractedObject = interactive;
                    UpdateText(interactive);
                }

                if (input.GetInteract())
                {
                    interactive.Interact(this);
                }

                return;
            }
        }

        if (_hadTargetSet)
        {
            _hadTargetSet = false;
            UpdateText(null);
            _crossHairMaterial.color = Color.gray;
            _lastInteractedObject = null;
        }
    }
}