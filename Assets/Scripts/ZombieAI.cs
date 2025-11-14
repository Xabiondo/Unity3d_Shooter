using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class ZombieAI : MonoBehaviour
{
    public Transform player;            // Referencia al jugador
    private NavMeshAgent agent;         // Referencia al agente
    private Animator anim;              // Referencia al Animator

    public int health = 8;              // Vidas del zombie
    private bool isDead = false;        // Para saber si ya está muerto

    // --- Variables de Efecto Visual de Daño ---
    public SkinnedMeshRenderer[] meshRenderers;
    public Color damageColor = Color.red;
    public float damageFlashDuration = 0.1f;
    private Color[] originalColors;

    // --- ¡NUEVAS VARIABLES DE ATAQUE! ---
    public float attackRange = 2f;      // Distancia para empezar a atacar
    public float attackRate = 1.5f;     // Tiempo entre ataques (segundos)
    public int attackDamage = 10;       // Daño que hace el zombie
    private float nextAttackTime = 0f;  // Para controlar el cooldown
    private PlayerHealth playerHealth;  // <-- ¡NUEVO! Referencia al script de vida del jugador

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        // --- ¡NUEVO! Configurar el NavMeshAgent para el ataque ---
        // El agente parará automáticamente a esta distancia
        if (agent != null)
        {
            agent.stoppingDistance = attackRange;
        }

        // --- Búsqueda del Jugador (como antes) ---
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        // --- ¡NUEVO! Obtener la referencia al script de vida del jugador ---
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth == null)
            {
                Debug.LogError("¡El objeto del Jugador no tiene el script PlayerHealth!");
            }
        }

        // --- Guardar colores originales (como antes) ---
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

        // Asegurarse de que el jugador existe y el agente está listo
        if (player != null && agent != null && agent.isOnNavMesh)
        {
            // Calcular la distancia al jugador
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            // --- ¡LÓGICA DE ESTADO: ATACAR O PERSEGUIR! ---
            if (distanceToPlayer <= agent.stoppingDistance)
            {
                // Estamos EN RANGO: Detener movimiento y atacar

                // Forzamos la animación de Speed a 0, porque ya no nos movemos
               // anim.SetFloat("Speed", 0f);

                // Girar para mirar al jugador
                FacePlayer();

                // Comprobar si ha pasado el cooldown para el próximo ataque
                if (Time.time >= nextAttackTime)
                {
                    Attack(); // <-- ¡NUEVO! Llamar a la función de ataque
                    nextAttackTime = Time.time + attackRate; // Reiniciar cooldown
                }
            }
            else
            {
                // Estamos LEJOS: Perseguir
                agent.SetDestination(player.position);

                // Actualizar animación de movimiento (como antes)
                float currentSpeed = agent.velocity.magnitude;
              //  anim.SetFloat("Speed", currentSpeed);
            }
        }
    }

    // --- ¡NUEVAS FUNCIONES DE ATAQUE! ---

    /// <summary>
    /// Gira al zombie para que mire al jugador suavemente
    /// </summary>
    void FacePlayer()
    {
        // No queremos que el zombie se incline si el jugador salta
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        // Interpola suavemente a la nueva rotación (el 5f es la velocidad de giro)
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    /// <summary>
    /// Inicia la animación de ataque
    /// </summary>
    void Attack()
    {
        if (isDead) return;

        // Esto solo DISPARA la animación. 
        // El daño real se hará con un "Animation Event"
        // (Ver instrucciones en el siguiente paso)
        anim.SetTrigger("Attack"); // <-- ¡DEBES CREAR ESTE TRIGGER EN EL ANIMATOR!
        Debug.Log("Zombie inicia animación de ataque");
    }

    /// <summary>
    /// ¡FUNCIÓN MUY IMPORTANTE!
    /// Esta función es para ser llamada por un ANIMATION EVENT
    /// puesto en el frame exacto de GOLPE de tu animación de ataque.
    /// </summary>
    public void DealDamageEvent()
    {
        // Doble chequeo por si el jugador o el zombie mueren justo en ese frame
        if (isDead || playerHealth == null) return;

        // Comprobar si el jugador SIGUE en rango cuando la mano golpea
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Usamos un rango un pelín mayor (ej: attackRange + 0.5m) para ser generosos
        if (distanceToPlayer <= attackRange + 0.5f)
        {
            Debug.Log("¡Zombie GOLPEA al jugador!");
            playerHealth.TakeDamage(attackDamage);
        }
        else
        {
            Debug.Log("¡Zombie ataca pero el jugador esquivó!");
        }
    }

    // --- El resto de tus funciones (TakeDamage, Die, DamageFlash) ---
    // (No cambian, así que las omito aquí por brevedad,
    // pero deben seguir en tu script tal cual estaban)

public void TakeDamage(int damageAmount)
{
    if (isDead) return;

    health -= damageAmount;
    Debug.Log("¡Zombie golpeado! Vida restante: " + health);
    
    StartCoroutine(DamageFlash()); // Mueve el flash aquí arriba

    if (health <= 0)
    {
        Die();
    }
    else // <-- ¡NUEVO ELSE!
    {
        // Solo reproduce la animación de "hit" si el zombie NO va a morir
        anim.CrossFade("hitZombie", 0.5f); 
    }
}
void Die()
    {
        // 1. ¡Comprobación de seguridad PRIMERO!
        // Si ya estoy muerto, no hagas nada más.
        if (isDead) return; 
        isDead = true; // Ahora sí, estoy muerto.

        // 2. Notificar al Round Manager
        if (RoundManager.instance != null)
        {
            RoundManager.instance.ZombieDied();
        }

        // 3. Detener todo movimiento y física del agente
        // ESTO es lo que para el "movimiento loco"
        agent.velocity = Vector3.zero;
        agent.isStopped = true;
        agent.enabled = false; 

        // 4. Parar la animación de correr
        //anim.SetFloat("Speed", 0f);

        // 5. Desactivar el collider
        Collider zombieCollider = GetComponent<Collider>();
        if (zombieCollider != null)
        {
            zombieCollider.enabled = false;
        }

        // 6. Iniciar la animación de muerte
        anim.SetTrigger("Die");
        Debug.Log("¡El zombie ha muerto!");

        // 7. Destruir el objeto después de la animación
        Destroy(gameObject, 20f);
    }
    IEnumerator DamageFlash()
    {
        if (meshRenderers.Length == 0) yield break;

        foreach (SkinnedMeshRenderer renderer in meshRenderers)
        {
            if (renderer != null && renderer.material.HasProperty("_Color"))
            {
                renderer.material.color = damageColor;
            }
        }

        yield return new WaitForSeconds(damageFlashDuration);

        for (int i = 0; i < meshRenderers.Length; i++)
        {
            if (meshRenderers[i] != null && meshRenderers[i].material.HasProperty("_Color"))
            {
                meshRenderers[i].material.color = originalColors[i];
            }
        }
    }
}