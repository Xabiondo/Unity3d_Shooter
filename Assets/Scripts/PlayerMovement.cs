using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;

    public float groundDrag;
    [Header("Ground Check")]

    public float jumpForce ; 
    public float jumpCooldown ;
    public float airMultiplier ;
    bool readyToJump ;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;

    public float playerHeight ; 
    public LayerMask whatIsGround;
    bool grounded;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;
    Rigidbody rb; 

    void Start()
    {
        rb = GetComponent<Rigidbody>(); 
        rb.freezeRotation = true;
        readyToJump = true ;
    }

    void Update()
    {
        grounded = Physics.Raycast(transform.position , Vector3.down , playerHeight * 0.5f + 0.2f , whatIsGround);
        if(grounded){
             Debug.Log("Grounded");
        }
       
        MyInput();
        SpeedControl();

        if (grounded)
        {
            rb.drag = groundDrag; 
        }
    
        else
        {
            rb.drag = 0 ; 
            
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force); // ❌ "addForce" → ✅ "AddForce"
        if (!grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }else if(grounded){
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f , ForceMode.Force);
        }
        
    }
    private void SpeedControl(){
        Vector3 flatVel = new Vector3(rb.velocity.x , 0 , rb.velocity.z);

        if (flatVel.magnitude > moveSpeed){
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x , rb.velocity.y , limitedVel.z);
        }
    }
    private void Jump(){
        Debug.Log("Jumped");
        rb.velocity = new Vector3(rb.velocity.x , 0 , rb.velocity.z);
        rb.AddForce(transform.up * jumpForce , ForceMode.Impulse);
    }
    private void ResetJump(){
        readyToJump = true ; 
    }
}