using UnityEngine;
using System.Collections;
using TMPro; // <-- ¡MUY IMPORTANTE! Necesario para el texto

public class RoundManager : MonoBehaviour
{
    [Header("Referencias de Escena")]
    public GameObject zombiePrefab; // El prefab de tu zombie
    public Transform[] spawnPoints; // Arrastra tus 5 spawn points aquí
    public Light directionalLight; // Arrastra tu luz direccional (el "Sol")
    public TextMeshProUGUI roundText; // Arrastra tu texto de UI aquí

    [Header("Configuración de Rondas")]
    public int baseZombiesPerRound = 5; // Zombies en la Ronda 1
    public int zombiesIncreasePerRound = 2; // Cuántos más cada ronda
    public float spawnDelay = 0.5f; // Tiempo entre cada spawn de zombie
    public float timeBetweenRounds = 10f; // Tiempo de descanso

    [Header("Configuración de Oscuridad")]
    public float nightIntensity = 0.05f; // Qué tan oscuro se vuelve el ambiente
    public float dayIntensity = 1.0f; // Intensidad de la luz de día
    public float sunRotationPerRound = 10f; // Cuántos grados baja el sol

    private int currentRound = 0;
    private int zombiesAlive = 0;
    private bool isSpawning = false;

    // Singleton (para que el ZombieAI nos encuentre fácil)
    public static RoundManager instance;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // Empezamos la primera ronda al iniciar el juego
        StartCoroutine(StartNextRound());
    }

    IEnumerator StartNextRound()
    {
        // Espera el tiempo de descanso (excepto en la ronda 1)
        if (currentRound > 0)
        {
            UpdateRoundText("¡Ronda " + currentRound + " completada!");
            yield return new WaitForSeconds(timeBetweenRounds);
        }

        currentRound++;
        Debug.Log("Iniciando Ronda: " + currentRound);

        // 1. Actualizar UI
        UpdateRoundText("Ronda " + currentRound);

        // 2. Hacerse más de noche
        UpdateDarkness();

        // 3. Empezar a spawnear
        isSpawning = true;
        int zombiesToSpawn = baseZombiesPerRound + (currentRound * zombiesIncreasePerRound);
        zombiesAlive = zombiesToSpawn; // Importante: Asignamos cuántos vivirán

        yield return StartCoroutine(SpawnWave(zombiesToSpawn));
        
        isSpawning = false;
    }

    IEnumerator SpawnWave(int amount)
    {
        Debug.Log("Spawneando " + amount + " zombies...");
        for (int i = 0; i < amount; i++)
        {
            // 1. Elige un spawn point aleatorio
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            // 2. Crea el zombie
            Instantiate(zombiePrefab, spawnPoint.position, spawnPoint.rotation);

            // 3. Espera un poco
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    void UpdateDarkness()
    {
        // 1. Gira el sol para que baje
        if (directionalLight != null)
        {
            directionalLight.transform.Rotate(sunRotationPerRound, 0, 0);
        }

        // 2. Oscurece la luz ambiental (¡MUY IMPORTANTE!)
        // Calcula qué tan "de noche" estamos (0.0 = día, 1.0 = noche total)
        float darknessFactor = Mathf.InverseLerp(1, 15, currentRound); // Será noche total en ronda 15
        
        // RenderSettings es la luz ambiental de toda la escena
        RenderSettings.ambientIntensity = Mathf.Lerp(dayIntensity, nightIntensity, darknessFactor);
    }

    void UpdateRoundText(string text)
    {
        if (roundText != null)
        {
            roundText.text = text;
        }
    }

    // --- ¡FUNCIÓN PÚBLICA QUE LLAMARÁ EL ZOMBIE! ---
    public void ZombieDied()
    {
        zombiesAlive--;
        Debug.Log("Zombie muerto. Quedan: " + zombiesAlive);

        // Si ya no quedan zombies Y no estamos spawneando...
        if (zombiesAlive <= 0 && !isSpawning)
        {
            Debug.Log("¡Ronda terminada! Preparando la siguiente...");
            StartCoroutine(StartNextRound());
        }
    }
}