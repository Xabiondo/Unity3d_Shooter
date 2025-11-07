using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float range = 100f;
    public Camera playerCamera;
    public ParticleSystem muzzleFlash; // Opcional
    public AudioSource shootSound;     // Opcional

    void Update()
    {
        if (Input.GetButtonDown("Fire1")) // Disparar con clic izquierdo
        {
            Shoot();
        }
    }

    void Shoot()
    {
        // Disparamos desde el centro de la pantalla
        Vector3 centerScreen = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        Ray ray = playerCamera.ScreenPointToRay(centerScreen);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, range))
        {
            Debug.Log("Hit: " + hit.collider.name);



            // Opcional: Crear efecto de impacto
            if (impactEffect != null)
            {
                Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            }

            // Dibujar rayo desde la cámara hasta el punto de impacto
            Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red, 2f);
        }
        else
        {
            // Dibujar rayo desde la cámara hasta el final del rango (si no hay impacto)
            Debug.DrawRay(ray.origin, ray.direction * range, Color.green, 2f);
        }

        // Opcional: Reproducir efectos visuales y sonoros
        if (muzzleFlash != null) muzzleFlash.Play();
        if (shootSound != null) shootSound.Play();
    }

    // Opcional: Prefab de efecto de impacto
    public GameObject impactEffect;
}