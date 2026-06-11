using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Vida del Jugador")]
    public Image healthBar;
    public Sprite[] healthSprites; // Ej: 0 = 0 vida, 3 = Vida llena

    [Header("Desbloqueos - Armadillo")]
    public GameObject armadilloKillPanel;
    public Image armadilloKillIcon;
    public Sprite[] armadilloKillSprites; // Tus 5 sprites
    private Coroutine armadilloFadeRoutine;

    [Header("Desbloqueos - Araña")]
    public GameObject spiderKillPanel;
    public Image spiderKillIcon;
    public Sprite[] spiderKillSprites; // Tus 5 sprites
    private Coroutine spiderFadeRoutine;

    [Header("Habilidades - UI")]
    public GameObject abilitiesPanel; // Se activa cuando cambias a una transformación desbloqueada
    public Image basicAbilityIcon;
    public Image superAbilityIcon;

    [Header("Habilidades - Sprites de Cooldown")]
    public Sprite[] armadilloBasicSprites;
    public Sprite[] armadilloSuperSprites;
    public Sprite[] spiderBasicSprites;
    public Sprite[] spiderSuperSprites;

    [Header("Audio")]
    public AudioSource uiAudioSource;
    public AudioClip unlockCompletedSound;

    // Controladores de Cooldown internos
    private float basicCooldownTimer;
    private float basicCooldownMax;
    private float superCooldownTimer;
    private float superCooldownMax;
    private char currentActiveAnimal = ' '; 

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        // Reducir los temporizadores de las habilidades
        if (basicCooldownTimer > 0) basicCooldownTimer -= Time.deltaTime;
        if (superCooldownTimer > 0) superCooldownTimer -= Time.deltaTime;

        // Si el panel de habilidades está visible, actualizamos los sprites en tiempo real
        if (abilitiesPanel != null && abilitiesPanel.activeSelf)
        {
            UpdateCooldownVisuals();
        }
    }

    // --- 1. LÓGICA DE VIDA ---
    public void UpdateHealth(int currentHp)
    {
        if (healthBar == null || healthSprites.Length == 0) return;
        // Asegura que no busquemos un sprite fuera del límite
        int index = Mathf.Clamp(currentHp, 0, healthSprites.Length - 1);
        healthBar.sprite = healthSprites[index];
    }

    // --- 2. LÓGICA DE CONTADOR DE MUERTES ---
    public void UpdateKillCounter(string type, int kills)
    {
        if (type == "Armadillo")
        {
            if (armadilloFadeRoutine != null) StopCoroutine(armadilloFadeRoutine);
            armadilloFadeRoutine = StartCoroutine(ShowKillCounter(armadilloKillPanel, armadilloKillIcon, armadilloKillSprites, kills));
        }
        else if (type == "Spider")
        {
            if (spiderFadeRoutine != null) StopCoroutine(spiderFadeRoutine);
            spiderFadeRoutine = StartCoroutine(ShowKillCounter(spiderKillPanel, spiderKillIcon, spiderKillSprites, kills));
        }
    }

    private IEnumerator ShowKillCounter(GameObject panel, Image icon, Sprite[] sprites, int kills)
    {
        // Asignar el sprite correspondiente (kills = 1 mostrará el sprite en la posición 0)
        int index = Mathf.Clamp(kills - 1, 0, sprites.Length - 1);
        icon.sprite = sprites[index];
        panel.SetActive(true);

        if (kills >= 5)
        {
            if (uiAudioSource && unlockCompletedSound) uiAudioSource.PlayOneShot(unlockCompletedSound);
            yield return new WaitForSeconds(3f); // Se queda un poco más de tiempo para celebrar
            panel.SetActive(false);
        }
        else
        {
            yield return new WaitForSeconds(2.5f); // Aparece solo por unos segundos
            panel.SetActive(false);
        }
    }

    // --- 3. LÓGICA DE HABILIDADES Y COOLDOWNS ---
    public void SwitchActiveTransformation(char animalChar)
    {
        currentActiveAnimal = char.ToLower(animalChar);
        
        // Solo activamos la UI de habilidades si tiene la Araña ('s') o Armadillo ('a')
        bool isTransformation = (currentActiveAnimal == 'a' || currentActiveAnimal == 's');
        if (abilitiesPanel != null) abilitiesPanel.SetActive(isTransformation);
        
        UpdateCooldownVisuals();
    }

    public void StartCooldown(bool isSuper, float duration)
    {
        if (!isSuper) 
        { 
            basicCooldownMax = duration; 
            basicCooldownTimer = duration; 
        }
        else 
        { 
            superCooldownMax = duration; 
            superCooldownTimer = duration; 
        }
    }

    private void UpdateCooldownVisuals()
    {
        if (currentActiveAnimal == 'a')
        {
            basicAbilityIcon.sprite = GetCooldownSprite(armadilloBasicSprites, basicCooldownTimer, basicCooldownMax);
            superAbilityIcon.sprite = GetCooldownSprite(armadilloSuperSprites, superCooldownTimer, superCooldownMax);
        }
        else if (currentActiveAnimal == 's')
        {
            basicAbilityIcon.sprite = GetCooldownSprite(spiderBasicSprites, basicCooldownTimer, basicCooldownMax);
            superAbilityIcon.sprite = GetCooldownSprite(spiderSuperSprites, superCooldownTimer, superCooldownMax);
        }
    }

    private Sprite GetCooldownSprite(Sprite[] array, float timer, float maxDuration)
    {
        if (array == null || array.Length == 0) return null;
        if (maxDuration <= 0f || timer <= 0f) return array[array.Length - 1]; // Sprite de "Habilidad Lista"

        // Calcular porcentaje: 1.0 = Listo, 0.0 = Recién usado
        float progress = 1f - (timer / maxDuration);
        int index = Mathf.Clamp(Mathf.FloorToInt(progress * array.Length), 0, array.Length - 1);
        
        return array[index];
    }
}