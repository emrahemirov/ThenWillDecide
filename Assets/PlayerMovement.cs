using System;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float verticalMoveSpeed = 25;
    [SerializeField] private float horizontalMoveSpeed = 15f;
    [SerializeField] private float escapeFromBorderDelta = 60f;
    [SerializeField] private float accelerationDelta = 50f;
    [SerializeField] private float decelerationDelta = 40f;
    [SerializeField] private float horizontalOffset = 0.5f;
    [SerializeField] private float verticalOffset = 2.5f;

    [SerializeField] private GameObject playerCase;

    private FloatingJoystick _joystick;

    private Vector3 _input;
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
        var targetYRotation = Mathf.Clamp((_input.z >= 0 ? _input.x : -_input.x) * 5, -5, 5);
        var targetXRotation = Mathf.Clamp(-_input.z * 3, -3, 3);
        var targetZRotation = Mathf.Clamp((_input.z >= 0 ? _input.x : -_input.x) * 5, -5, 5);

        var targetRotation = Quaternion.Euler(targetXRotation, targetYRotation, targetZRotation);
        playerCase.transform.rotation =
            Quaternion.Slerp(playerCase.transform.rotation, targetRotation, Time.fixedDeltaTime * 5f);
    }

    private void SetInputVector()
    {
        var newInput = new Vector3(_joystick.Horizontal, 0, _joystick.Vertical);

        if (_isOnTopBorder && newInput.z > 0.5f) newInput.z = 0;
        if (_isOnBottomBorder && newInput.z < -0.5f) newInput.z = 0;
        if (_isOnLeftBorder && newInput.x < -0.5f) newInput.x = 0;
        if (_isOnRightBorder && newInput.x > 0.5f) newInput.x = 0;

        _input = newInput;
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
        _isOnTopBorder = Math.Abs(transform.position.z - _topBorder) < 1f;
        _isOnBottomBorder = Math.Abs(transform.position.z - _bottomBorder) < 1f;
        _isOnLeftBorder = Math.Abs(transform.position.x - _leftBorder) < 1f;
        _isOnRightBorder = Math.Abs(transform.position.x - _rightBorder) < 1f;
    }

    private void SetIsMoving()
    {
        _isMovingForward = _input.z > 0.5f;
        _isMovingBack = _input.z < -0.5f;
        _isMovingLeft = _input.x < -0.5f;
        _isMovingRight = _input.x > 0.5f;
    }
}