using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")] [SerializeField] private float verticalMoveSpeed = 20f;
    [SerializeField] private float horizontalMoveSpeed = 15f;
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float deceleration = 40f;
    [SerializeField] private float horizontalOffset = 0.5f;
    [SerializeField] private float verticalOffset = 0.5f;


    private FloatingJoystick _joystick;

    private Vector3 _input;
    private Vector3 _currentVelocity;
    private Rigidbody _rb;

    private float _leftBoundary;
    private float _rightBoundary;
    private float _topBoundary;
    private float _bottomBoundary;


    private void Start()
    {
        _joystick = FindFirstObjectByType<FloatingJoystick>();
        _rb = GetComponent<Rigidbody>();
        var planeCollider = GameObject.FindGameObjectWithTag("Ground").GetComponent<Collider>();
        var playerCollider = GetComponent<Collider>();

        var playerHalfWidth = playerCollider.bounds.extents.x;
        var playerHalfHeight = playerCollider.bounds.extents.z;
        var planeColliderBounds = planeCollider.bounds;

        var effectiveHorizontalOffset = horizontalOffset + playerHalfWidth;
        var effectiveVerticalOffset = verticalOffset + playerHalfHeight;

        _leftBoundary = planeColliderBounds.min.x + effectiveHorizontalOffset;
        _rightBoundary = planeColliderBounds.max.x - effectiveHorizontalOffset;
        _topBoundary = planeColliderBounds.min.z + effectiveVerticalOffset;
        _bottomBoundary = planeColliderBounds.max.z - effectiveVerticalOffset;
    }

    private void Update()
    {
        SetInputVector();
        MovePosition();
    }

    private void SetInputVector()
    {
        _input = new Vector3(_joystick.Horizontal, 0, _joystick.Vertical).normalized;
    }

    private void MovePosition()
    {
        _currentVelocity = _input.magnitude > 0.1f
            ? Vector3.MoveTowards(_currentVelocity,
                new Vector3(_input.x * horizontalMoveSpeed, 0, _input.z * verticalMoveSpeed),
                acceleration * Time.deltaTime)
            : Vector3.MoveTowards(_currentVelocity, Vector3.zero, deceleration * Time.deltaTime);

        _rb.linearVelocity = new Vector3(_currentVelocity.x, _rb.linearVelocity.y, _currentVelocity.z);

        var desiredPosition = transform.position + _rb.linearVelocity * Time.deltaTime;

        var clampedX = Mathf.Clamp(desiredPosition.x, _leftBoundary, _rightBoundary);
        var clampedZ = Mathf.Clamp(desiredPosition.z, _topBoundary, _bottomBoundary);

        _rb.MovePosition(new Vector3(clampedX, _rb.position.y, clampedZ));
    }
}