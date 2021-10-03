using System;
using UnityEngine;

public class FpsController : MonoBehaviour
{
    [Header("References")] [SerializeField]
    private FpsInput input;

    [SerializeField] private Transform headTransform;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private AudioSource audioSource;

    [Header("Assets")] [SerializeField] private AudioClip stepSfx;
    [SerializeField] private AudioClip jumpSfx;
    [SerializeField] private AudioClip landSfx;

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
    [SerializeField] private float sprintModifier = 1.5f;
    [SerializeField] private float landSoundMinDelay = 0.3f;
    [SerializeField] private float walkStepDistance;

    [Header("Read Only")] [SerializeField] private Vector3 _velocity;
    [SerializeField] private float _verticalAngle;
    [SerializeField] private bool _grounded = true;
    [SerializeField] private float _timeSinceNotGrounded;
    [SerializeField] private Vector3 _groundNormal;
    [SerializeField] private float _lastTimeJumped;
    [SerializeField] private Vector3 _latestImpactSpeed;
    [SerializeField] private float _slopeFriction;
    [SerializeField] private float _distanceSinceLastStep;

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

                _slopeFriction = 0;
                if (IsSlopeGround(_groundNormal))
                {
                    _grounded = true;

                    if (hit.distance > characterController.skinWidth)
                    {
                        characterController.Move(Vector3.down * hit.distance); // snap to ground
                    }
                }
                else if (IsSlopeStep(bottom, top, radius, groundCheckDistance, out var stepHit))
                {
                    if (stepHit.distance > characterController.skinWidth)
                    {
                        _grounded = true;
                        var move = Vector3.up * (characterController.stepOffset - stepHit.distance);
                        characterController.Move(move); // snap to ground
                    }
                }
                else
                {
                    Debug.Log("Slope friction");
                    // Add slope friction
                    if (_velocity.y > 0)
                    {
                        _slopeFriction = 1 - Vector3.Dot(_groundNormal, Vector3.up);
                        _velocity.y -= _slopeFriction * (accelerationGround * Time.deltaTime);
                    }
                }
            }
        }

        if (!_grounded && wasGrounded)
        {
            _timeSinceNotGrounded = Time.time;
        }

        // Handle Landing
        if (_grounded && !wasGrounded)
        {
            if (Time.time > _timeSinceNotGrounded + landSoundMinDelay)
            {
                audioSource.PlayOneShot(landSfx);
            }
        }
    }

    private bool IsSlopeStep(Vector3 bottom, Vector3 top, float radius, float groundCheckDistance, out RaycastHit hit)
    {
        var offset = (Vector3) (transform.localToWorldMatrix * _velocity);
        offset.y = characterController.stepOffset;
        // Move capsule in the XZ movement direction, and up the max step size, then try hit test
        if (Physics.CapsuleCast(bottom + offset, top + offset, radius, Vector3.down, out hit,
            groundCheckDistance + offset.y, groundLayers, QueryTriggerInteraction.Ignore))
        {
            return IsSlopeGround(hit.normal);
        }

        return false;
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
        return angle < characterController.slopeLimit;
    }

    private void HandleMovement(Transform tr)
    {
        var worldMoveInput = tr.TransformVector(input.GetMove());
        var (beforeMoveBottom, beforeMoveTop, beforeMoveRadius) = GetCapsuleInfo(characterController, tr);

        if (_grounded)
        {
            var isSprinting = input.GetSprint();
            var speedModifier = isSprinting ? sprintModifier : 1f;

            // Handle movement on ground
            var targetVelocity = worldMoveInput * (maxSpeedGround * speedModifier);
            targetVelocity = ReorientOnSlope(targetVelocity, _groundNormal, tr);
            _velocity = Vector3.MoveTowards(_velocity, targetVelocity, accelerationGround * Time.deltaTime);

            if (input.GetJump() && !Physics.CapsuleCast(beforeMoveBottom, beforeMoveTop, beforeMoveRadius, Vector3.up,
                out var ceilHit, jumpSpeed * Time.deltaTime, groundLayers, QueryTriggerInteraction.Ignore))
            {
                _velocity = new Vector3(_velocity.x, jumpSpeed, _velocity.z);
                _lastTimeJumped = Time.time;
                _grounded = false;
                _groundNormal = Vector3.up;
                _timeSinceNotGrounded = Time.time;

                audioSource.PlayOneShot(jumpSfx);
            }

            if (_distanceSinceLastStep > walkStepDistance / speedModifier)
            {
                _distanceSinceLastStep = 0;
                audioSource.PlayOneShot(stepSfx);
            }

            _distanceSinceLastStep += _velocity.magnitude * Time.deltaTime;
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