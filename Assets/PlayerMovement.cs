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

    [SerializeField] private LineRenderer skidMark;
    [SerializeField] private Transform leftRearTireBottom;
    [SerializeField] private Transform rightRearTireBottom;
    private Transform roadSpawner;
    private Vector3[] originalVertices;

    private FloatingJoystick _joystick;

    private Vector3 _movementInput;
    private Vector3 _currentVelocity;

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
        roadSpawner = GameObject.FindGameObjectWithTag("RoadSpawner").transform;
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

        VelocityChange();
    }

    private Vector3 _prevVelocity;

    private void VelocityChange()
    {
        var isTurningRight = _currentVelocity.x > 0 && _prevVelocity.x <= 0;
        var isTurningLeft = _currentVelocity.x < 0 && _prevVelocity.x >= 0;
        var isTurning = isTurningRight || isTurningLeft;

        if (isTurning && Math.Abs(_movementInput.x) > 0.8f)
        {
            if (isTurningRight) IncreaseSkidMarkLength(leftRearTireBottom, _movementInput.z >= 0 ? 5 : -5);
            if (isTurningLeft) IncreaseSkidMarkLength(rightRearTireBottom, _movementInput.z >= 0 ? -5 : 5);
        }

        _prevVelocity = _currentVelocity;
    }


    private async void IncreaseSkidMarkLength(Transform tire, float rotateY)
    {
        const float targetScaleZ = -1.5f;
        const float duration = 0.1f;
        var lineRenderer = Instantiate(skidMark, tire.position,
            Quaternion.Euler(0, rotateY, 0), tire);
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(1, new Vector3(0, 0, 0));

        var timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            var newScaleZ = Mathf.Lerp(0, targetScaleZ, timeElapsed / duration);

            lineRenderer.SetPosition(1, new Vector3(0, 0, newScaleZ));

            timeElapsed += Time.deltaTime;
            await UniTask.Yield();
        }

        lineRenderer.transform.parent = roadSpawner;
    }


    private void FixedUpdate()
    {
        MovePosition();
        RotatePlayerCase();
    }

    private void RotatePlayerCase()
    {
        var targetYRotation = Mathf.Clamp((_movementInput.z >= 0 ? _movementInput.x : -_movementInput.x) * 6, -6, 6);
        var targetXRotation = Mathf.Clamp(-_movementInput.z * 3, -3, 3);
        var targetZRotation = Mathf.Clamp((_movementInput.z >= 0 ? _movementInput.x : -_movementInput.x) * 5, -5, 5);

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

        _movementInput = newInput;
    }

    private void MovePosition()
    {
        var targetVelocity = new Vector3(
            _movementInput.x * horizontalMoveSpeed,
            0,
            _movementInput.z * verticalMoveSpeed
        );

        var velocityDelta =
            IsEscapingFromBorders() ? escapeFromBorderDelta :
            IsMoving() ? accelerationDelta : decelerationDelta;

        _currentVelocity = Vector3.MoveTowards(_currentVelocity, targetVelocity, velocityDelta * Time.fixedDeltaTime);

        var desiredPosition = transform.position + _currentVelocity * Time.fixedDeltaTime;

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
        _isMovingForward = _movementInput.z > 0.5f;
        _isMovingBack = _movementInput.z < -0.5f;
        _isMovingLeft = _movementInput.x < 0;
        _isMovingRight = _movementInput.x > 0;
    }
}