using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float sprintMultiplier = 1.5f; // <-- NUEVO: Multiplicador de velocidad
    private float currentMoveSpeed; // <-- NUEVO: Para guardar la velocidad actual

    public float groundDrag;
    [Header("Ground Check")]

    public float jumpForce ; 
    public float jumpCooldown ;
    public float airMultiplier ;
    bool readyToJump ;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.Tab; // <-- NUEVO: Tecla de Sprint

    public float playerHeight ; 
    public LayerMask whatIsGround;
    public bool grounded;

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
        currentMoveSpeed = moveSpeed; // <-- NUEVO: Inicializar velocidad
    }

    void Update()
    {
        grounded = Physics.Raycast(transform.position , Vector3.down , playerHeight * 0.5f + 0.2f , whatIsGround);

        MyInput();
        HandleSprinting(); // <-- NUEVO: Llama a la función de sprint
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

    // --- ¡NUEVA FUNCIÓN DE SPRINT! ---
    private void HandleSprinting()
    {
        // Si mantienes pulsada la tecla de sprint Y estás en el suelo
        if (Input.GetKey(sprintKey) && grounded)
        {
            currentMoveSpeed = moveSpeed * sprintMultiplier;
        }
        else
        {
            currentMoveSpeed = moveSpeed;
        }
    }

    // --- ¡FUNCIÓN DE MOVIMIENTO CORREGIDA! ---
    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // Lógica corregida: solo se aplica una fuerza,
        // ya sea la de suelo o la de aire.

        if (grounded)
        {
            // Aplicar fuerza en el suelo (usa currentMoveSpeed para el sprint)
            rb.AddForce(moveDirection.normalized * currentMoveSpeed * 10f, ForceMode.Force);
        }
        else if (!grounded)
        {
            // Aplicar fuerza en el aire (usa currentMoveSpeed y multiplicador)
            rb.AddForce(moveDirection.normalized * currentMoveSpeed * 10f * airMultiplier, ForceMode.Force);
        }
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x , 0 , rb.velocity.z);

        // --- CORREGIDO: Usa currentMoveSpeed para el límite ---
        if (flatVel.magnitude > currentMoveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * currentMoveSpeed;
            rb.velocity = new Vector3(limitedVel.x , rb.velocity.y , limitedVel.z);
        }
    }

    private void Jump()
    {
        Debug.Log("Jumped");
        rb.velocity = new Vector3(rb.velocity.x , 0 , rb.velocity.z);
        rb.AddForce(transform.up * jumpForce , ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true ; 
    }
}