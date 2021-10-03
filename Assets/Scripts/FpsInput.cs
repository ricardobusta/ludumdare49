using System;
using UnityEngine;

public class FpsInput : MonoBehaviour
{
    [Header("References")] [SerializeField]
    private FpsController controller;

    [Header("Configs")] public float lookSensitivity = 1.0f;
    public bool invertXAxis = false;
    public bool invertYAxis = false;
    public bool touchControl;

    [Header("Parameters")] [SerializeField]
    private float webglSensitivityMultiplier = 0.25f;

    [SerializeField] private float maxMovementSize = 1.0f;

    public bool locked;

    private bool IsWebgl
    {
        get
        {
#if UNITY_WEBGL
            return true;
#endif
            return false;
        }
    }

    private void Awake()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public float GetHorizontal()
    {
        return GetAxis("Mouse X", invertXAxis);
    }

    public float GetVertical()
    {
        return GetAxis("Mouse Y", invertYAxis);
    }

    private float GetAxis(string mouseAxis, bool invertAxis)
    {
        if (locked)
        {
            return 0;
        }

        var input = 0f;
        if (touchControl)
        {
            throw new NotImplementedException();

            // Only if not mouse, since mouse already scales with frame time.
            input *= Time.deltaTime;
        }
        else
        {
            input = Input.GetAxisRaw(mouseAxis);

            if (IsWebgl)
            {
                // Higher sensitivity in Webgl mouse input.
                input *= webglSensitivityMultiplier;
            }
        }

        if (invertAxis)
        {
            input *= -1;
        }

        return input;
    }

    public bool GetJump()
    {
        if (locked)
        {
            return false;
        }

        return Input.GetButtonDown("Jump");
    }

    public Vector3 GetMove()
    {
        if (locked)
        {
            return Vector3.zero;
        }

        return Vector3.ClampMagnitude(new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")),
            maxMovementSize); // Clamp to avoid moving faster on diagonal
    }
}