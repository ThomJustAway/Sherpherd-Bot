using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5.0f;
    public float jumpForce = 5.0f;
    public float gravityScale = -9.81f;
    public float mouseSensitivity = 5.0f;
    [SerializeField] private float checkingDistance = 2f;
    private bool isGrounded;
    private Rigidbody rb;
    private Transform cameraTransform;
    private bool canJump;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.freezeRotation = true;
        isGrounded = true;
        cameraTransform = Camera.main.transform; // Assuming main camera follows player
        Cursor.lockState = CursorLockMode.Locked; // Lock cursor for smooth camera control
    }

    void FixedUpdate()
    {
        // Movement based on input
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 DirectionVector = new Vector3(horizontalInput, 0, verticalInput).normalized;
        Vector3 movement = (transform.rotation * DirectionVector) * speed;

        // Apply movement while considering physics
        rb.MovePosition(rb.position + movement * Time.fixedDeltaTime);

        

        // Adjust gravity 
        rb.AddForce(Vector3.up * gravityScale * Time.fixedDeltaTime);
    }

    void Update() // Update called every frame
    {
        // Look based on mouse movement
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);


        canJump = Input.GetKeyDown(KeyCode.Space);
        CheckGrounded();
        // Jumping logic
        if (isGrounded && canJump)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
        }


        // Keep camera looking horizontally (adjust based on your camera setup)
        cameraTransform.rotation = Quaternion.Euler(cameraTransform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0f);
        transform.rotation = cameraTransform.rotation;
    }

    private void CheckGrounded()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        isGrounded = Physics.Raycast(ray, checkingDistance);
        Debug.DrawRay(transform.position, Vector3.down * checkingDistance , Color.red);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Ground") // Check for ground tag
        {
            isGrounded = true;
        }
    }
}
