using UnityEngine;

public class EfectoCabeceo : MonoBehaviour
{
    [Header("Dependencias (Arrastra tu Jugador)")]
    [Tooltip("El Rigidbody de tu jugador")]
    [SerializeField] private Rigidbody rb;
    [Tooltip("El script PlayerMovement de tu jugador")]
    [SerializeField] private PlayerMovement playerMovement; 

    [Header("Configuración del Cabeceo")]
    [Tooltip("Velocidad del movimiento de cabeceo")]
    [SerializeField] private float velocidadCabeceo = 12.0f;
    [Tooltip("Amplitud (qué tanto se mueve) del cabeceo")]
    [SerializeField] private float magnitudCabeceo = 0.04f;
    [Tooltip("Velocidad para suavizar la vuelta a la calma")]
    [SerializeField] private float velocidadSuavizado = 10.0f;

    // Variables internas
    private Vector3 posOriginalCamara;
    private float timer = 0.0f;

    void Start()
    {
        // Guardamos la posición local inicial de la cámara
        posOriginalCamara = transform.localPosition;
    }

    void Update()
    {
        // 1. Comprobar si el jugador se está moviendo en el suelo
        
        // Obtenemos la velocidad horizontal (X, Z) del Rigidbody
        float velocidadHorizontal = new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude;
        
        // Usamos el 'bool' público que creamos en PlayerMovement
        bool estaEnSuelo = playerMovement.grounded;

        if (velocidadHorizontal > 0.1f && estaEnSuelo)
        {
            // 2. El jugador se está moviendo: calculamos el cabeceo

            // Incrementamos el timer con el tiempo y la velocidad
            timer += Time.deltaTime * velocidadCabeceo;

            // 3. Calculamos la posición usando Ondas Sinusoidales
            // Movimiento vertical (arriba y abajo)
            float movimientoY = Mathf.Sin(timer) * magnitudCabeceo;
            
            // Movimiento horizontal (vaivén lateral), más lento
            float movimientoX = Mathf.Cos(timer / 2) * magnitudCabeceo * 0.5f;

            // 4. Aplicamos la nueva posición a la cámara
            // Es IMPORTANTE usar localPosition
            transform.localPosition = new Vector3(
                posOriginalCamara.x + movimientoX,
                posOriginalCamara.y + movimientoY,
                posOriginalCamara.z
            );
        }
        else
        {
            // 5. El jugador está quieto: volvemos suavemente a la posición original
            timer = 0; // Reseteamos el timer
            
            transform.localPosition = Vector3.Lerp(
                transform.localPosition, 
                posOriginalCamara, 
                Time.deltaTime * velocidadSuavizado
            );
        }
    }
}