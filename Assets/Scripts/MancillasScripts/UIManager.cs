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

    [Header("Desbloqueos - Pez")]
    public GameObject fishKillPanel;
    public Image fishKillIcon;
    public Sprite[] fishKillSprites;
    private Coroutine fishFadeRoutine;

    [Header("Habilidades - Pez (Sprites de Cooldown)")]
    public Sprite[] fishBasicSprites;
    public Sprite[] fishSuperSprites;

    // Relojes del pez
    private float fishBasicTimer, fishBasicMax;
    private float fishSuperTimer, fishSuperMax;

    [Header("Desbloqueos - Pinguino")]
    public GameObject penguinKillPanel;
    public Image penguinKillIcon;
    public Sprite[] penguinKillSprites;

    [Header("Habilidades - Pingüino")]
    public Sprite[] penguinBasicSprites;
    public Sprite[] penguinSuperSprites;
    private Coroutine penguinFadeRoutine;

    [Header("Desbloqueos - Armiño")]
    public GameObject ermineKillPanel;
    public Image ermineKillIcon;
    public Sprite[] ermineKillSprites;

    [Header("Habilidades - Armiño")]
    public Sprite[] ermineBasicSprites;
    public Sprite[] ermineSuperSprites;
    private Coroutine ermineFadeRoutine;

    [Header("Audio")]
    public AudioSource uiAudioSource;
    public AudioClip unlockCompletedSound;

    // Controladores de Cooldown internos
    private float armaBasicTimer, armaBasicMax;
    private float armaSuperTimer, armaSuperMax;
    private float spiderBasicTimer, spiderBasicMax;
    private float spiderSuperTimer, spiderSuperMax;
    private float penguinBasicTimer, penguinBasicMax;
    private float penguinSuperTimer, penguinSuperMax;
    private float ermineBasicTimer, ermineBasicMax;
    private float ermineSuperTimer, ermineSuperMax;
    private char currentActiveAnimal = ' '; 

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        // Reducir todos los temporizadores en segundo plano
        if (armaBasicTimer > 0) armaBasicTimer -= Time.deltaTime;
        if (armaSuperTimer > 0) armaSuperTimer -= Time.deltaTime;
        if (spiderBasicTimer > 0) spiderBasicTimer -= Time.deltaTime;
        if (spiderSuperTimer > 0) spiderSuperTimer -= Time.deltaTime;
        if (fishBasicTimer > 0) fishBasicTimer -= Time.deltaTime;
        if (fishSuperTimer > 0) fishSuperTimer -= Time.deltaTime;
        if (penguinBasicTimer > 0) penguinBasicTimer -= Time.deltaTime;
        if (penguinSuperTimer > 0) penguinSuperTimer -= Time.deltaTime;
        if (ermineBasicTimer > 0) ermineBasicTimer -= Time.deltaTime;
        if (ermineSuperTimer > 0) ermineSuperTimer -= Time.deltaTime;
        

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
        else if (type == "Fish")
        {
            if (fishFadeRoutine != null) StopCoroutine(fishFadeRoutine);
            fishFadeRoutine = StartCoroutine(ShowKillCounter(fishKillPanel, fishKillIcon, fishKillSprites, kills));
        }
        else if (type == "Penguin")
        {
            if (penguinFadeRoutine != null) StopCoroutine(penguinFadeRoutine);
            penguinFadeRoutine = StartCoroutine(ShowKillCounter(penguinKillPanel, penguinKillIcon, penguinKillSprites, kills));
        }
        else if (type == "Ermine")
        {
            if (ermineFadeRoutine != null) StopCoroutine(ermineFadeRoutine);
            ermineFadeRoutine = StartCoroutine(ShowKillCounter(ermineKillPanel, ermineKillIcon, ermineKillSprites, kills));
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
            SfxPlayer.Play(uiAudioSource, unlockCompletedSound);
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

    // Cambiamos 'void' por 'bool' para que el UI le avise al jugador si la habilidad estaba lista
    public bool StartCooldown(char animalChar, bool isSuper, float duration)
    {
        char a = char.ToLower(animalChar);

        if (a == 'a') // Relojes del Armadillo
        {
            if (!isSuper)
            {
                if (armaBasicTimer > 0) return false; 
                armaBasicMax = duration; 
                armaBasicTimer = duration; 
                return true; 
            }
            else
            {
                if (armaSuperTimer > 0) return false; 
                armaSuperMax = duration; 
                armaSuperTimer = duration; 
                return true; 
            }
        }
        else if (a == 's') // Relojes de la Araña
        {
            if (!isSuper)
            {
                if (spiderBasicTimer > 0) return false; 
                spiderBasicMax = duration; 
                spiderBasicTimer = duration; 
                return true; 
            }
            else
            {
                if (spiderSuperTimer > 0) return false; 
                spiderSuperMax = duration; 
                spiderSuperTimer = duration; 
                return true; 
            }
        }
        else if (a == 'f') // Relojes del Pez
        {
            if (!isSuper)
            {
                if (fishBasicTimer > 0) return false; 
                fishBasicMax = duration; 
                fishBasicTimer = duration; 
                return true; 
            }
            else
            {
                if (fishSuperTimer > 0) return false; 
                fishSuperMax = duration; 
                fishSuperTimer = duration; 
                return true; 
            }
        }
        else if (a == 'p') // Relojes del Pingüino
        {
            if (!isSuper)
            {
                if (penguinBasicTimer > 0) return false; 
                penguinBasicMax = duration; 
                penguinBasicTimer = duration; 
                return true; 
            }
            else
            {
                if (penguinSuperTimer > 0) return false; 
                penguinSuperMax = duration; 
                penguinSuperTimer = duration; 
                return true; 
            }
        }
        else if (a == 'e') // Relojes del Armiño
        {
            if (!isSuper)
            {
                if (ermineBasicTimer > 0) return false; 
                ermineBasicMax = duration; 
                ermineBasicTimer = duration; 
                return true; 
            }
            else
            {
                if (ermineSuperTimer > 0) return false; 
                ermineSuperMax = duration; 
                ermineSuperTimer = duration; 
                return true; 
            }
        }
        return false;
    }

    private void UpdateCooldownVisuals()
    {
        if (currentActiveAnimal == 'a')
        {
            basicAbilityIcon.sprite = GetCooldownSprite(armadilloBasicSprites, armaBasicTimer, armaBasicMax);
            superAbilityIcon.sprite = GetCooldownSprite(armadilloSuperSprites, armaSuperTimer, armaSuperMax);
        }
        else if (currentActiveAnimal == 's')
        {
            basicAbilityIcon.sprite = GetCooldownSprite(spiderBasicSprites, spiderBasicTimer, spiderBasicMax);
            superAbilityIcon.sprite = GetCooldownSprite(spiderSuperSprites, spiderSuperTimer, spiderSuperMax);
        }
        else if (currentActiveAnimal == 'f')
        {
            basicAbilityIcon.sprite = GetCooldownSprite(fishBasicSprites, fishBasicTimer, fishBasicMax);
            superAbilityIcon.sprite = GetCooldownSprite(fishSuperSprites, fishSuperTimer, fishSuperMax);
        }
        else if (currentActiveAnimal == 'p')
        {
            basicAbilityIcon.sprite = GetCooldownSprite(penguinBasicSprites, penguinBasicTimer, penguinBasicMax);
            superAbilityIcon.sprite = GetCooldownSprite(penguinSuperSprites, penguinSuperTimer, penguinSuperMax);
        }
        else if (currentActiveAnimal == 'e')
        {
            basicAbilityIcon.sprite = GetCooldownSprite(ermineBasicSprites, ermineBasicTimer, ermineBasicMax);
            superAbilityIcon.sprite = GetCooldownSprite(ermineSuperSprites, ermineSuperTimer, ermineSuperMax);
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