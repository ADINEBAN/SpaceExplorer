using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 15f;

    [Header("Screen Boundary")]
    [SerializeField] private float screenPadding = 1.0f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private Camera mainCamera;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }
    }

    private void Update()
    {
        ReadInput();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void ReadInput()
    {
        movement = Vector2.zero;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.aKey.isPressed ||
            Keyboard.current.leftArrowKey.isPressed)
        {
            movement.x = -1f;
        }

        if (Keyboard.current.dKey.isPressed ||
            Keyboard.current.rightArrowKey.isPressed)
        {
            movement.x = 1f;
        }

        if (Keyboard.current.wKey.isPressed ||
            Keyboard.current.upArrowKey.isPressed)
        {
            movement.y = 1f;
        }

        if (Keyboard.current.sKey.isPressed ||
            Keyboard.current.downArrowKey.isPressed)
        {
            movement.y = -1f;
        }

        movement = movement.normalized;
    }

    private void MovePlayer()
    {
        if (mainCamera == null)
            return;

        Vector2 nextPosition =
            rb.position +
            movement * moveSpeed * Time.fixedDeltaTime;

        float cameraDistance =
            Mathf.Abs(mainCamera.transform.position.z);

        Vector3 bottomLeft =
            mainCamera.ViewportToWorldPoint(
                new Vector3(0f, 0f, cameraDistance)
            );

        Vector3 topRight =
            mainCamera.ViewportToWorldPoint(
                new Vector3(1f, 1f, cameraDistance)
            );

        nextPosition.x = Mathf.Clamp(
            nextPosition.x,
            bottomLeft.x + screenPadding,
            topRight.x - screenPadding
        );

        nextPosition.y = Mathf.Clamp(
            nextPosition.y,
            bottomLeft.y + screenPadding,
            topRight.y - screenPadding
        );

        rb.MovePosition(nextPosition);
    }
}