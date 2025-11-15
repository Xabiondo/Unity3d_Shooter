using UnityEngine;
using UnityEngine.UI; // <-- ¡Necesario si quieres una barra de vida!

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    // --- Opcional: Para UI ---
    // Arrastra aquí tu Slider de vida desde el Canvas
    public Slider healthSlider; 
    // Arrastra aquí una imagen del Canvas para el efecto de "pantalla roja"
    public Image damageImage; 
    public Color flashColor = new Color(1f, 0f, 0f, 0.1f);
    public float flashSpeed = 5f;
    bool damaged = false;

    // --- Punto de Reaparición ---
    // Arrastra aquí un objeto (como un Empty) que marque dónde reaparecerá
    public Transform puntoDeReaparicion; // <--- NUEVO

    // --- Referencia a la lógica de muerte/Game Over ---
    // (Podríamos necesitar esto más tarde)
    // public PlayerDeathHandler deathHandler; 

    void Start()
    {
        currentHealth = maxHealth;
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        // Asegúrate de que la imagen de daño esté transparente al inicio
        if (damageImage != null)
        {
            damageImage.color = Color.clear;
        }
    }

    void Update()
    {
        // --- Lógica para el "flash" de daño ---
        if (damageImage == null) return;

        if (damaged)
        {
            damageImage.color = flashColor;
        }
        else
        {
            // Transiciona suavemente de vuelta a transparente
            damageImage.color = Color.Lerp(damageImage.color, Color.clear, flashSpeed * Time.deltaTime);
        }
        // Resetea el flag
        damaged = false;
    }


    /// <summary>
    /// Esta función será llamada por los zombies (y otras amenazas)
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return; // Ya está muerto (o en proceso de respawn)

        damaged = true; // Activa el flash de daño
        currentHealth -= amount;

        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        Debug.Log("¡El jugador recibió " + amount + " de daño! Vida: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("¡El jugador ha muerto!");
        
        // --- Lógica de Teletransporte / Reaparición ---
        // Comprueba si asignaste un punto de reaparición en el Inspector
        if (puntoDeReaparicion != null)
        {
            // Mueve al jugador a la posición del punto de reaparición
            transform.position = puntoDeReaparicion.position; // <--- NUEVO

            // Opcional: También puedes resetear la rotación
            // transform.rotation = puntoDeReaparicion.rotation; // <--- NUEVO (Opcional)
            
            // Restaura la salud
            currentHealth = maxHealth; // <--- NUEVO
            
            // Actualiza la barra de vida en la UI
            if (healthSlider != null) // <--- NUEVO
            {
                healthSlider.value = currentHealth; // <--- NUEVO
            }
            
            Debug.Log("¡Jugador reaparecido!");
        }
        else
        {
            // Si no hay punto de reaparición, simplemente registra la muerte
            Debug.LogWarning("¡El jugador murió pero no hay 'puntoDeReaparicion' asignado!");
            // Aquí podrías poner otra lógica, como destruir el objeto:
            // Destroy(gameObject);
        }

        // Aquí iría tu lógica de Game Over (si no quieres reaparecer)
        // Por ejemplo:
        // 1. Activar animación de muerte del jugador
        // 2. Mostrar menú de "Game Over"
        // 3. Pausar el juego
        // Time.timeScale = 0f; // Pausa simple
        
        // Si tienes un script que maneje esto:
        // if (deathHandler != null)
        // {
        //    deathHandler.HandleDeath();
        // }
    }
}