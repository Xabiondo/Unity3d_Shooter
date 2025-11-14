using UnityEngine;
using System.Collections; // <-- ¡MUY IMPORTANTE! Añade esto al principio

public class Weapon : MonoBehaviour
{
    public float range = 100f;
    public Camera playerCamera;
    public ParticleSystem muzzleFlash;
    public AudioSource shootSound;
    public GameObject impactEffect;

    private Animator anim;

    public Transform gunBarrelTip; // La punta del cañón (donde empieza la bala)
    private LineRenderer bulletTrail; // Referencia a nuestro Line Renderer

    // --- ¡NUEVAS VARIABLES PARA DISPARO AUTOMÁTICO! ---
    [Header("Cadencia de Disparo")]
    public float disparosPorSegundo = 10f; // Balas que dispara por segundo
    private float proximoDisparo = 0f;    // Para controlar el "cooldown"
    // --- FIN DE NUEVAS VARIABLES ---

    void Start() // <-- NECESITAMOS UN MÉTODO START()
    {
        // Obtenemos el componente Line Renderer al empezar
        bulletTrail = GetComponent<LineRenderer>();
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // --- ¡LÓGICA ACTUALIZADA! ---
        // 1. Cambiamos GetButtonDown por GetButton (para que detecte si está MANTENIDO)
        // 2. Añadimos una comprobación de tiempo (Time.time >= proximoDisparo)
        
        if (Input.GetButton("Fire1") && Time.time >= proximoDisparo)
        {
            // ¡Calculamos el tiempo del siguiente disparo!
            // Si disparamos 10 balas/seg, el tiempo entre balas es 1/10 = 0.1s
            proximoDisparo = Time.time + 1f / disparosPorSegundo;
            
            Shoot(); // Llamamos a la función de disparo
        }
    }

    void Shoot()
    {
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }
        
        if (shootSound != null)
        {
            shootSound.Play();
        }

        anim.SetTrigger("Shoot");

        Vector3 centerScreen = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        Ray ray = playerCamera.ScreenPointToRay(centerScreen);
        RaycastHit hit;

        // --- ¡LÓGICA DEL TRAZADOR! ---
        Vector3 trailEndPosition; // Dónde terminará el trazador

        if (Physics.Raycast(ray, out hit, range))
        {
            Debug.Log("Hit: " + hit.collider.name);
            trailEndPosition = hit.point; // El trazador termina donde golpea

            // (Aquí va tu lógica de daño al zombie...)
            if (hit.collider.CompareTag("Zombie"))
            {
                ZombieAI zombie = hit.collider.GetComponent<ZombieAI>();
                if (zombie != null)
                {
                    zombie.TakeDamage(1);
                }
            }
            
            if (impactEffect != null)
            {
                Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            }
        }
        else
        {
            // Si no golpea nada, el trazador se va hasta el rango máximo
            trailEndPosition = ray.origin + ray.direction * range;
        }

        // ¡Llama a la coroutine para dibujar el trazador!
        StartCoroutine(DrawBulletTrail(trailEndPosition));
    }

    // --- ¡NUEVA FUNCIÓN (COROUTINE)! ---
    // Esta función dibuja la línea y la borra poco después
    private IEnumerator DrawBulletTrail(Vector3 endPoint)
    {
        // Activa la línea
        bulletTrail.enabled = true;
        
        // Fija la posición de inicio (la punta del cañón)
        bulletTrail.SetPosition(0, gunBarrelTip.position);
        
        // Fija la posición final (donde golpeó)
        bulletTrail.SetPosition(1, endPoint);

        // Espera un tiempo muy corto
        yield return new WaitForSeconds(0.05f); // 0.05 segundos es un buen valor

        // Desactiva la línea
        bulletTrail.enabled = false;
    }
}