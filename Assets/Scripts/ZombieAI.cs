using UnityEngine;
using UnityEngine.AI;
using System.Collections; // <-- ¡NUEVO! Necesario para las Coroutines

public class ZombieAI : MonoBehaviour
{
    public Transform player;            // Referencia al jugador
    private NavMeshAgent agent;         // Referencia al agente
    private Animator anim;              // Referencia al Animator
    
    public int health = 8;              // Vidas del zombie
    private bool isDead = false;        // Para saber si ya está muerto

    // --- ¡NUEVAS VARIABLES PARA EL EFECTO VISUAL DE DAÑO! ---
    public SkinnedMeshRenderer[] meshRenderers; // <-- Arrastra los renderers de tu zombie aquí
    public Color damageColor = Color.red;      // Color cuando recibe daño
    public float damageFlashDuration = 0.1f;   // Duración del "parpadeo"

    private Color[] originalColors; // Para guardar los colores originales

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        // --- ¡NUEVO! Guardar los colores originales de los materiales ---
        if (meshRenderers.Length > 0)
        {
            originalColors = new Color[meshRenderers.Length];
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                if (meshRenderers[i] != null && meshRenderers[i].material.HasProperty("_Color"))
                {
                    originalColors[i] = meshRenderers[i].material.color;
                }
            }
        }
    }

    void Update()
    {
        if (isDead) return;

        if (player != null && agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(player.position);

            float currentSpeed = agent.velocity.magnitude;
            anim.SetFloat("Speed", currentSpeed);
        }
    }

    /// <summary>
    /// Esta función puede ser llamada desde otros scripts (como el de disparo)
    /// </summary>
    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        health -= damageAmount;
        Debug.Log("¡Zombie golpeado! Vida restante: " + health);

        // --- ¡NUEVO! Activar la animación de "Hit" y el efecto visual ---
        anim.CrossFade("hitZombie", 0.5f); // <-- Esta es la nueva línea
        StartCoroutine(DamageFlash()); // Inicia la coroutine del parpadeo

        if (health <= 0)
        {
            Die();
        }
    }

void Die()
    {
        if (isDead) return; // Asegurarse de que no se llame dos veces
        isDead = true;
        
        // --- ARREGLO PARA EL "SPRINT" ---
        // 1. Detenemos al agente y FORZAMOS su velocidad a CERO.
        agent.velocity = Vector3.zero;
        agent.isStopped = true;
        
        // 2. Desactivamos el componente NavMeshAgent. Ya no lo necesitamos.
        agent.enabled = false; 

        // 3. Forzamos al Animator a dejar de correr.
        anim.SetFloat("Speed", 0f);
        // ---------------------------------

        // Desactiva el collider para que no se pueda seguir disparando al cadáver
        Collider zombieCollider = GetComponent<Collider>();
        if (zombieCollider != null)
        {
            zombieCollider.enabled = false; 
        }

        anim.SetTrigger("Die"); // Ahora sí, reproduce la animación de muerte
        Debug.Log("¡El zombie ha muerto!");

        // --- ARREGLO PARA LA DESAPARICIÓN ---
        // AHORA DEBES CONFIGURAR ESTO.
        // Busca tu clip de animación de muerte (ej: "zombie_death") y mira cuánto dura.
        // Si dura 4.2 segundos, pon 4.2f aquí.
        // Yo le pondré 5 segundos para estar seguros, pero ajústalo tú.
        Destroy(gameObject, 20f); // <-- ¡AJUSTA ESTE TIEMPO A LA DURACIÓN DE TU ANIMACIÓN!
    }

    // --- ¡NUEVA FUNCIÓN! Coroutine para el efecto visual de "parpadeo" ---
    IEnumerator DamageFlash()
    {
        if (meshRenderers.Length == 0) yield break; // Si no hay renderers, sal de aquí

        // Cambiar a color de daño
        foreach (SkinnedMeshRenderer renderer in meshRenderers)
        {
            if (renderer != null && renderer.material.HasProperty("_Color"))
            {
                renderer.material.color = damageColor;
            }
        }

        // Esperar un corto tiempo
        yield return new WaitForSeconds(damageFlashDuration);

        // Volver a los colores originales
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            if (meshRenderers[i] != null && meshRenderers[i].material.HasProperty("_Color"))
            {
                meshRenderers[i].material.color = originalColors[i];
            }
        }
    }
}