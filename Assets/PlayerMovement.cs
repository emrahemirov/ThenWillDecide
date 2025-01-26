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

    [SerializeField] private Transform playerCase;

    [SerializeField] private MeshFilter skidmarkMeshFilter;
    [SerializeField] private Transform leftBackTireBottom;
    private Transform roadSpawner;
    private Mesh mesh;
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

        var obj = Instantiate(skidmarkMeshFilter, leftBackTireBottom.position,
            Quaternion.Euler(0, 5, 0), roadSpawner);
        mesh = obj.mesh;
        // Store the original vertices for reference
        originalVertices = mesh.vertices;
        var vertices = mesh.vertices;
        for (var i = 0; i < vertices.Length; i++)
        {
            vertices[i].z = 0;
        }

        mesh.vertices = vertices;
        mesh.RecalculateBounds();
    }

    private void Update()
    {
        var vertices = mesh.vertices;


        for (var i = 0; i < vertices.Length; i++)
        {
            if (!(originalVertices[i].z < 0)) continue;
            vertices[i].z -= 0.7f;
            vertices[i].z = Mathf.Clamp(vertices[i].z, -5, 0);
        }

        mesh.vertices = vertices;
        mesh.RecalculateBounds();

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
        _isOnTopBorder = Math.Abs(transform.position.z - _topBorder) < 1f;
        _isOnBottomBorder = Math.Abs(transform.position.z - _bottomBorder) < 1f;
        _isOnLeftBorder = Math.Abs(transform.position.x - _leftBorder) < 1f;
        _isOnRightBorder = Math.Abs(transform.position.x - _rightBorder) < 1f;
    }

    private void SetIsMoving()
    {
        _isMovingForward = _movementInput.z > 0.5f;
        _isMovingBack = _movementInput.z < -0.5f;
        _isMovingLeft = _movementInput.x < 0;
        _isMovingRight = _movementInput.x > 0;
    }
}