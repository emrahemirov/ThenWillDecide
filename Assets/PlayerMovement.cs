using UnityEngine;
using UnityEngine.Serialization;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float verticalMoveSpeed = 15f;
    [SerializeField] private float horizontalMoveSpeed = 10f;
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float deceleration = 40f;
    [SerializeField] private float horizontalOffset = 0.5f;
    [SerializeField] private float verticalOffset = 2.5f;

    private FloatingJoystick _joystick;

    private Vector3 _input;
    public Vector3 CurrentVelocity { get; private set; }
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
        _bottomBoundary = planeColliderBounds.min.z + effectiveVerticalOffset;
        _topBoundary = planeColliderBounds.max.z - effectiveVerticalOffset;
    }

    private void Update()
    {
        SetInputVector();
    }
    
    private void FixedUpdate()
    {
        MovePosition();
    }

    private void SetInputVector()
    {
        _input = new Vector3(_joystick.Horizontal, 0, _joystick.Vertical).normalized;
    }

    private void MovePosition()
    {
        CurrentVelocity = _input.magnitude > 0.1f
            ? Vector3.MoveTowards(CurrentVelocity,
                new Vector3(_input.x * horizontalMoveSpeed, 0, _input.z * verticalMoveSpeed),
                acceleration * Time.fixedDeltaTime)
            : Vector3.MoveTowards(CurrentVelocity, Vector3.zero, deceleration * Time.fixedDeltaTime);
     
        _rb.linearVelocity = new Vector3(CurrentVelocity.x, _rb.linearVelocity.y, CurrentVelocity.z);

        var desiredPosition = transform.position + _rb.linearVelocity * Time.fixedDeltaTime;

        var clampedX = Mathf.Clamp(desiredPosition.x, _leftBoundary, _rightBoundary);
        var clampedZ = Mathf.Clamp(desiredPosition.z, _bottomBoundary, _topBoundary);

        _rb.MovePosition(new Vector3(clampedX, _rb.position.y, clampedZ));
    }
}