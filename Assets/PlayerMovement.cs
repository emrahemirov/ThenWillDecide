using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float verticalMoveSpeed = 25;
    [SerializeField] private float horizontalMoveSpeed = 15;
    [SerializeField] private float escapeFromBorderDelta = 60;
    [SerializeField] private float accelerationDelta = 50;
    [SerializeField] private float decelerationDelta = 40;
    [SerializeField] private float horizontalOffset = 0.5f;
    [SerializeField] private float verticalOffset = 2.5f;

    [SerializeField] private Transform playerCase;

    private FloatingJoystick _joystick;

    public Vector3 MovementInput { get; private set; }
    public Vector3 CurrentVelocity { get; private set; }

    private float _leftBorder, _rightBorder, _topBorder, _bottomBorder;

    private bool
        _isOnTopBorder,
        _isOnBottomBorder,
        _isOnLeftBorder,
        _isOnRightBorder;

    private bool
        _isMovingForward,
        _isMovingBack,
        _isMovingLeft,
        _isMovingRight;

    private void Start()
    {
        _joystick = FindFirstObjectByType<FloatingJoystick>();
        var planeCollider = GameObject.FindGameObjectWithTag("Ground").GetComponent<Collider>();
        var playerCollider = GetComponent<Collider>();

        var playerHalfWidth = playerCollider.bounds.extents.x;
        var playerHalfHeight = playerCollider.bounds.extents.z;
        var planeColliderBounds = planeCollider.bounds;

        var effectiveHorizontalOffset = horizontalOffset + playerHalfWidth;
        var effectiveVerticalOffset = verticalOffset + playerHalfHeight;

        _leftBorder = planeColliderBounds.min.x + effectiveHorizontalOffset;
        _rightBorder = planeColliderBounds.max.x - effectiveHorizontalOffset;
        _bottomBorder = planeColliderBounds.min.z + effectiveVerticalOffset;
        _topBorder = planeColliderBounds.max.z - effectiveVerticalOffset;
    }

    private float maxVel, minVel;

    private void Update()
    {
        SetInputVector();
        SetIsOnBorders();
        SetIsMoving();
    }


    private void FixedUpdate()
    {
        MovePosition();
        RotatePlayerCase();
    }

    private void RotatePlayerCase()
    {
        var targetYRotation = Mathf.Clamp((MovementInput.z >= 0 ? MovementInput.x : -MovementInput.x) * 6, -6, 6);
        var targetXRotation = Mathf.Clamp(-MovementInput.z * 3, -3, 3);
        var targetZRotation = Mathf.Clamp((MovementInput.z >= 0 ? MovementInput.x : -MovementInput.x) * 5, -5, 5);

        var targetRotation = Quaternion.Euler(targetXRotation, targetYRotation, targetZRotation);
        playerCase.rotation =
            Quaternion.Slerp(playerCase.rotation, targetRotation, Time.fixedDeltaTime * 5f);
    }

    private void SetInputVector()
    {
        var newInput = new Vector3(_joystick.Horizontal, 0, _joystick.Vertical);

        if (_isOnTopBorder && newInput.z > 0.5f) newInput.z = 0;
        if (_isOnBottomBorder && newInput.z < -0.5f) newInput.z = 0;
        if (_isOnLeftBorder && newInput.x < 0) newInput.x = 0;
        if (_isOnRightBorder && newInput.x > 0) newInput.x = 0;

        MovementInput = newInput;
    }

    private void MovePosition()
    {
        var targetVelocity = new Vector3(
            MovementInput.x * horizontalMoveSpeed,
            0,
            MovementInput.z * verticalMoveSpeed
        );

        var velocityDelta =
            IsEscapingFromBorders() ? escapeFromBorderDelta :
            IsMoving() ? accelerationDelta : decelerationDelta;

        CurrentVelocity = Vector3.MoveTowards(CurrentVelocity, targetVelocity, velocityDelta * Time.fixedDeltaTime);

        var desiredPosition = transform.position + CurrentVelocity * Time.fixedDeltaTime;

        var clampedX = Mathf.Clamp(desiredPosition.x, _leftBorder, _rightBorder);
        var clampedZ = Mathf.Clamp(desiredPosition.z, _bottomBorder, _topBorder);

        transform.position = new Vector3(clampedX, transform.position.y, clampedZ);
    }

    private bool IsMoving()
    {
        return _isMovingForward || _isMovingBack || _isMovingLeft || _isMovingRight;
    }

    private bool IsEscapingFromBorders()
    {
        return _isOnTopBorder && _isMovingBack || _isOnBottomBorder && _isMovingForward ||
               _isOnLeftBorder && _isMovingRight || _isOnRightBorder && _isMovingLeft;
    }

    private void SetIsOnBorders()
    {
        _isOnTopBorder = Math.Abs(transform.position.z - _topBorder) < 1;
        _isOnBottomBorder = Math.Abs(transform.position.z - _bottomBorder) < 1;
        _isOnLeftBorder = Math.Abs(transform.position.x - _leftBorder) < 1;
        _isOnRightBorder = Math.Abs(transform.position.x - _rightBorder) < 1;
    }

    private void SetIsMoving()
    {
        _isMovingForward = MovementInput.z > 0.5f;
        _isMovingBack = MovementInput.z < -0.5f;
        _isMovingLeft = MovementInput.x < 0;
        _isMovingRight = MovementInput.x > 0;
    }
}