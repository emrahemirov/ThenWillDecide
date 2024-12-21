using System;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float verticalMoveSpeed = 40f;
    [SerializeField] private float horizontalMoveSpeed = 20f;
    [SerializeField] private float escapeFromBorderDelta = 100f;
    [SerializeField] private float accelerationDelta = 50f;
    [SerializeField] private float decelerationDelta = 40f;
    [SerializeField] private float horizontalOffset = 0.5f;
    [SerializeField] private float verticalOffset = 2.5f;

    private FloatingJoystick _joystick;

    private Vector3 _input;
    public Vector3 CurrentVelocity { get; private set; }

    private float _leftBorder, _rightBorder, _topBorder, _bottomBorder;

    private bool
        _isOnTopBorder,
        _isOnBottomBorder,
        _isOnLeftBorder,
        _isOnRightBorder;

    private bool _isMovingForward,
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

    private void Update()
    {
        SetInputVector();
        SetIsOnBorders();
        SetIsMoving();
    }

    private void FixedUpdate()
    {
        MovePosition();
    }

    private void SetInputVector()
    {
        _input = new Vector3(_joystick.Horizontal, 0, _joystick.Vertical).normalized;
        if (_isOnTopBorder && _isMovingForward) _input.z = 0;
        if (_isOnBottomBorder && _isMovingBack) _input.z = 0;
        if (_isOnLeftBorder && _isMovingLeft) _input.x = 0;
        if (_isOnRightBorder && _isMovingRight) _input.x = 0;
    }

    private void MovePosition()
    {
        var targetVelocity = new Vector3(
            _input.x * horizontalMoveSpeed,
            0,
            _input.z * verticalMoveSpeed
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
        _isOnTopBorder = Math.Abs(transform.position.z - _topBorder) < 0.1f;
        _isOnBottomBorder = Math.Abs(transform.position.z - _bottomBorder) < 0.1f;
        _isOnLeftBorder = Math.Abs(transform.position.x - _leftBorder) < 0.1f;
        _isOnRightBorder = Math.Abs(transform.position.x - _rightBorder) < 0.1f;
    }

    private void SetIsMoving()
    {
        _isMovingForward = _input.z > 0.5f;
        _isMovingBack = _input.z < -0.5f;
        _isMovingLeft = _input.x < -0.5f;
        _isMovingRight = _input.x > 0.5f;
    }
}