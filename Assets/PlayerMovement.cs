using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    [Header("Movement")] 
    [SerializeField] private float verticalMoveSpeed = 40f;
    [SerializeField] private float horizontalMoveSpeed = 30f;
    [SerializeField] private float acceleration = 100f;
    [SerializeField] private float deceleration = 60f;
    [SerializeField] private float horizontalOffset = 0.5f;
    [SerializeField] private float verticalOffset = 0.5f;

    
    private FloatingJoystick _joystick;
    
    private Vector3 _input;
    private Vector3 _currentVelocity;
    private Rigidbody _rb;

    private float _minHorizontalMovable;
    private float _maxHorizontalMovable;
    private float _minVerticalMovable;
    private float _maxVerticalMovable;

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

        _minHorizontalMovable = planeColliderBounds.min.x + effectiveHorizontalOffset;
        _maxHorizontalMovable = planeColliderBounds.max.x - effectiveHorizontalOffset;
        _minVerticalMovable = planeColliderBounds.min.z + effectiveVerticalOffset;
        _maxVerticalMovable = planeColliderBounds.max.z - effectiveVerticalOffset;
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
        var targetVelocity = new Vector3(
            _input.x * horizontalMoveSpeed,
            0,
            _input.z * verticalMoveSpeed
        );

        _currentVelocity = Vector3.MoveTowards(
            _currentVelocity,
            targetVelocity,
            (_input.magnitude > 0 ? acceleration : deceleration) * Time.fixedDeltaTime
        );

        var desiredPosition = transform.position + transform.TransformDirection(_currentVelocity * Time.fixedDeltaTime);

        var clampedX = Mathf.Clamp(desiredPosition.x, _minHorizontalMovable, _maxHorizontalMovable);
        var clampedZ = Mathf.Clamp(desiredPosition.z, _minVerticalMovable, _maxVerticalMovable);

        _rb.MovePosition(new Vector3(clampedX, transform.position.y, clampedZ));
    }
}