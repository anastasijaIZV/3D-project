using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public Camera playerCamera;
    public Transform head;

    [Header("Movement")]
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float jumpPower = 7f;
    public float gravity = 10f;
    public float lookSpeed = 2f;
    public float lookXLimit = 90f;
    public float defaultHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchSpeed = 3f;
    public float slideSpeed = 9f;
    public float itemPickupDistance = 5f;

    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private bool canMove = true;
    private bool isCrouching = false;

    private Transform attachedObject = null;
    private float attachedDistance = 0f;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMovement();
        HandleMouseLook();
        HandleJump();
        HandleCrouch();
        HandlePickup();
    }

    void HandleMovement()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;

        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);
        moveDirection.y = movementDirectionY;

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        characterController.Move(moveDirection * Time.deltaTime);
    }

    void HandleMouseLook()
    {
        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }

    void HandleJump()
    {
        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpPower;
        }
    }

    void HandleCrouch()
    {
        if (Input.GetKey(KeyCode.C) && canMove)
        {
            characterController.height = crouchHeight;
            walkSpeed = crouchSpeed;
            runSpeed = crouchSpeed;
            isCrouching = true;
        }
        else if (isCrouching)
        {
            characterController.height = defaultHeight;
            walkSpeed = 6f;
            runSpeed = 12f;
            isCrouching = false;
        }
    }

    void HandlePickup()
    {
        RaycastHit hit;
        bool cast = Physics.Raycast(head.position, head.forward, out hit, itemPickupDistance);

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (attachedObject != null)
            {
                attachedObject.SetParent(null);

                if (attachedObject.GetComponent<Rigidbody>() != null)
                    attachedObject.GetComponent<Rigidbody>().isKinematic = false;

                if (attachedObject.GetComponent<Collider>() != null)
                    attachedObject.GetComponent<Collider>().enabled = true;

                attachedObject = null;
            }
            else
            {
                if (cast && hit.transform.CompareTag("pickable"))
                {
                    attachedObject = hit.transform;
                    attachedObject.SetParent(transform);
                    attachedDistance = Vector3.Distance(attachedObject.position, head.position);

                    if (attachedObject.GetComponent<Rigidbody>() != null)
                        attachedObject.GetComponent<Rigidbody>().isKinematic = true;

                    if (attachedObject.GetComponent<Collider>() != null)
                        attachedObject.GetComponent<Collider>().enabled = false;
                }
            }
        }
    }
}
