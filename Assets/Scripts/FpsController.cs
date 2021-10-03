using System;
using UnityEngine;

public class FpsController : MonoBehaviour
{
    [Header("References")] [SerializeField]
    private FpsInput input;

    [SerializeField] private Transform headTransform;
    [SerializeField] private CharacterController characterController;

    [Header("Parameters")] [SerializeField]
    private LayerMask groundLayers;

    [SerializeField] private float killHeight = -50f;
    [SerializeField] private float accelerationGround = 0.5f;
    [SerializeField] private float accelerationAir = 0.5f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float maxSpeedGround = 10f;
    [SerializeField] private float maxSpeedAir = 10f;
    [SerializeField] private float gravityAcceleration = 9.8f;
    [SerializeField] private float jumpSpeed = 10f;
    [SerializeField] private float groundCheckDistanceGround = 0.05f;
    [SerializeField] private float groundCheckDistanceAir = 0.05f;
    [SerializeField] private float verticalAngleLimit = 89f;
    [SerializeField] private float jumpGroundingDelay = 0.2f;
    [SerializeField] private float slopeAngleLimit = 45f;
    
    [Header("Debug")]
    [SerializeField] private Vector3 _velocity;
    private float _verticalAngle;
    [SerializeField] private bool _grounded = true;
    [SerializeField] private Vector3 _groundNormal;
    private float _lastTimeJumped;
    private Vector3 _latestImpactSpeed;

    private void Awake()
    {
        _grounded = true;
        _groundNormal = Vector3.up;
        _velocity = Vector3.zero;
        _verticalAngle = 0;
    }

    private void Update()
    {
        var tr = transform;
        if (tr.position.y < killHeight)
        {
            characterController.enabled = false; // Character controller do not like being moved around
            characterController.transform.position = Vector3.up;
            _velocity = Vector3.zero;
            // ReSharper disable once Unity.InefficientPropertyAccess
            characterController.enabled = true;
        }
        HandleGround(tr);
        HandleMovement(tr);
        HandleLook(tr);
    }

    private void HandleGround(Transform tr)
    {
        var wasGrounded = _grounded;

        // Actual Ground Check 
        var groundCheckDistance = _grounded
            ? characterController.skinWidth + groundCheckDistanceGround
            : groundCheckDistanceAir;

        _grounded = false;
        _groundNormal = Vector3.up;

        if (Time.time > _lastTimeJumped + jumpGroundingDelay)
        {
            var (bottom, top, radius) = GetCapsuleInfo(characterController, tr);
            if (Physics.CapsuleCast(bottom, top, radius, Vector3.down, out var hit,
                groundCheckDistance, groundLayers, QueryTriggerInteraction.Ignore))
            {
                _groundNormal = hit.normal;

                if (IsSlopeGround(_groundNormal))
                {
                    _grounded = true;

                    if (hit.distance > characterController.skinWidth)
                    {
                        characterController.Move(Vector3.down * hit.distance); // snap to ground
                    }
                }
            }
        }

        // Handle Landing
        if (_grounded && !wasGrounded)
        {
            // eg play sounds
        }
    }

    private bool IsSlopeGround(Vector3 groundNormal)
    {
        var characterUp = Vector3.up;
        var dot = Vector3.Dot(characterUp, groundNormal);
        if (dot <= 0)
        {
            return false; // it's on the ground or behind the ground
        }
        const double threshold = 1E-15;
        var num = Math.Sqrt(characterUp.sqrMagnitude * (double) groundNormal.sqrMagnitude);
        var angle = num < threshold
            ? 0.0f
            : (float) Math.Acos(Mathf.Clamp(dot / (float) num, -1f, 1f)) * Mathf.Rad2Deg;
        return angle < slopeAngleLimit;
    }

    private void HandleMovement(Transform tr)
    {
        var worldMoveInput = tr.TransformVector(input.GetMove());
        if (_grounded)
        {
            // Handle movement on ground
            var targetVelocity = worldMoveInput * maxSpeedGround;
            targetVelocity = ReorientOnSlope(targetVelocity, _groundNormal, tr);
            _velocity = Vector3.MoveTowards(_velocity, targetVelocity, accelerationGround * Time.deltaTime);

            if (input.GetJump())
            {
                _velocity = new Vector3(_velocity.x, jumpSpeed, _velocity.z);
                _lastTimeJumped = Time.time;
                _grounded = false;
                _groundNormal = Vector3.up;
            }
        }
        else
        {
            // Handle movement on air
            var verticalVelocity = Vector3.up * (_velocity.y - gravityAcceleration * Time.deltaTime);
            var horizontalVelocity = new Vector3(_velocity.x, 0, _velocity.z);
            horizontalVelocity += worldMoveInput * accelerationAir * Time.deltaTime;
            horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity, maxSpeedAir);
            _velocity = verticalVelocity + horizontalVelocity;
        }

        var (beforeMoveBottom, beforeMoveTop, beforeMoveRadius) = GetCapsuleInfo(characterController, tr);
        characterController.Move(_velocity * Time.deltaTime);

        // If movement caused a hit
        _latestImpactSpeed = Vector3.zero;
        if (Physics.CapsuleCast(beforeMoveBottom, beforeMoveTop, beforeMoveRadius, _velocity.normalized,
            out var hit, _velocity.magnitude * Time.deltaTime, groundLayers, QueryTriggerInteraction.Ignore))
        {
            _latestImpactSpeed = _velocity;
            
            _velocity = Vector3.ProjectOnPlane(_velocity, hit.normal);
        }
    }

    private (Vector3 bottom, Vector3 top, float radius) GetCapsuleInfo(CharacterController controller,
        Transform tr)
    {
        var position = tr.position;
        var up = tr.up;
        var radius = characterController.radius;
        var bottom = position + up * radius;
        var top = position + up * (characterController.height - radius);
        return (bottom, top, radius);
    }

    private void HandleLook(Transform tr)
    {
        // Look Horizontal
        tr.Rotate(new Vector3(0, input.GetHorizontal() * rotationSpeed, 0));

        // Look Vertical
        _verticalAngle = Mathf.Clamp(_verticalAngle - input.GetVertical() * rotationSpeed,
            -verticalAngleLimit, verticalAngleLimit);
        headTransform.localEulerAngles = new Vector3(_verticalAngle, 0, 0);
    }

    private Vector3 ReorientOnSlope(Vector3 vector, Vector3 slopeNormal, Transform tr)
    {
        var direction = vector.normalized;
        var directionRight = Vector3.Cross(direction, tr.up);
        return Vector3.Cross(slopeNormal, directionRight) * vector.magnitude;
    }
}