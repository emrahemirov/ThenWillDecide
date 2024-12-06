using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private FloatingJoystick joystick;
    [SerializeField] private GameObject planeGameObject;
    [SerializeField] private float verticalMoveSpeed = 15f;
    [SerializeField] private float horizontalMoveSpeed = 10f;

    [SerializeField] private float horizontalOffset = 0.5f;
    [SerializeField] private float verticalOffset = 0.5f;

    private Vector3 _input;
    private Rigidbody _rb;
    private Collider _planeCollider;
    private Collider _playerCollider;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _planeCollider = planeGameObject.GetComponent<Collider>();
        _playerCollider = GetComponent<Collider>(); // Get the player's collider
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
        _input = new Vector3(joystick.Horizontal, 0, joystick.Vertical);
    }

    private void MovePosition()
    {
        var playerHalfWidth = _playerCollider.bounds.extents.x;
        var playerHalfHeight = _playerCollider.bounds.extents.z;

        var effectiveHorizontalOffset = horizontalOffset + playerHalfWidth;
        var effectiveVerticalOffset = verticalOffset + playerHalfHeight;

        var desiredPosition = transform.position +
                              transform.TransformDirection(new Vector3(
                                  _input.x * horizontalMoveSpeed * Time.fixedDeltaTime,
                                  0,
                                  _input.z * verticalMoveSpeed * Time.fixedDeltaTime
                              ));

        var bounds = _planeCollider.bounds;
        var clampedX = Mathf.Clamp(desiredPosition.x, bounds.min.x + effectiveHorizontalOffset, bounds.max.x - effectiveHorizontalOffset);
        var clampedZ = Mathf.Clamp(desiredPosition.z, bounds.min.z + effectiveVerticalOffset, bounds.max.z - effectiveVerticalOffset);

        _rb.MovePosition(new Vector3(clampedX, transform.position.y, clampedZ));
    }
}
