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

    // --- ¡LA VARIABLE DE SEGURIDAD CLAVE! ---
    private bool isRoundInProgress = false; 

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
        // --- ¡BLOQUEO DE SEGURIDAD! ---
        if (isRoundInProgress)
        {
            Debug.LogWarning("Se intentó iniciar una ronda mientras otra ya estaba en progreso. Llamada ignorada.");
            yield break; // Detiene esta corrutina
        }
        isRoundInProgress = true; // ¡Bloqueamos!

        // Espera el tiempo de descanso (excepto en la ronda 1)
        if (currentRound > 0)
        {
            UpdateRoundText("¡Ronda " + currentRound + " completada!");
            yield return new WaitForSeconds(timeBetweenRounds);
        }

        currentRound++;

        // --- ¡¡NUEVA LÓGICA PARA LA RONDA 7!! ---

        // 1. Actualizar UI
        if (currentRound == 7)
        {
            Debug.Log("Iniciando Ronda: ¡APOCALIPSIS!");
            UpdateRoundText("¡Ya llegó el apocalipsis!");
        }
        else
        {
            Debug.Log("Iniciando Ronda: " + currentRound);
            UpdateRoundText("Ronda " + currentRound);
        }

        // 2. Hacerse más de noche (La función UpdateDarkness ha sido modificada)
        UpdateDarkness();

        // 3. Empezar a spawnear
        isSpawning = true;
        
        int zombiesToSpawn;

        // 4. Establecer número de zombies
        if (currentRound == 7)
        {
            zombiesToSpawn = 300; // ¡RONDA APOCALIPSIS!
        }
        else
        {
            // Fórmula normal
            zombiesToSpawn = baseZombiesPerRound + ((currentRound - 1) * zombiesIncreasePerRound);
        }
        
        zombiesAlive = zombiesToSpawn; // Importante: Asignamos cuántos vivirán
        
        // --- FIN DE LA LÓGICA DE RONDA 7 ---


        yield return StartCoroutine(SpawnWave(zombiesToSpawn));
        
        isSpawning = false;
        
        isRoundInProgress = false;
    }

    IEnumerator SpawnWave(int amount)
    {
        Debug.Log("Spawneando " + amount + " zombies...");
        for (int i = 0; i < amount; i++)
        {
            // Comprobación de seguridad
            if (spawnPoints.Length == 0)
            {
                Debug.LogError("¡No hay Spawn Points asignados en el RoundManager!");
                yield break;
            }

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
        if (directionalLight != null)
        {
            directionalLight.transform.Rotate(sunRotationPerRound, 0, 0);
        }

        float darknessFactor;
        
        // --- ¡NUEVA LÓGICA PARA LA RONDA 7! ---
        if (currentRound == 7)
        {
            // Forzamos el factor de oscuridad al máximo (1.0f)
            // para que use el 'nightIntensity' (0.05f)
            darknessFactor = 1.0f; 
        }
        else
        {
            // Lógica normal
            darknessFactor = Mathf.InverseLerp(1, 15, currentRound);
        }
        
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
        // Si la ronda ya ha terminado, no sigas restando
        if (zombiesAlive <= 0) return;

        zombiesAlive--;
        Debug.Log("Zombie muerto. Quedan: " + zombiesAlive);

        // --- ¡LA COMPROBACIÓN DE SEGURIDAD FINAL! ---
        if (zombiesAlive <= 0 && !isSpawning && !isRoundInProgress)
        {
            Debug.Log("¡Ronda terminada! Preparando la siguiente...");
            StartCoroutine(StartNextRound());
        }
    }
}