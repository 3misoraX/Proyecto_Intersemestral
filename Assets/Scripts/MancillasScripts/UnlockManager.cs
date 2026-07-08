using UnityEngine;

public class UnlockManager : MonoBehaviour
{
    // Esto permite que cualquier script acceda al Manager fácilmente
    public static UnlockManager Instance;

    [Header("Referencias")]
    public GunScript playerGun;
    
    [Header("Tarjetas a Desbloquear")]
    public TransformationObject armadilloCard;
    public TransformationObject spiderCard;

    [Header("Progreso")]
    public int killsRequired = 5;
    private int armadilloKills = 0;
    private int spiderKills = 0;

    private bool armadilloUnlocked = false;
    private bool spiderUnlocked = false;

    void Awake()
    {
        // Configuración del Singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Los enemigos llamarán a esta función justo antes de morir
    public void RegisterKill(string enemyType)
    {
        // ESTO APARECERÁ EN TU CONSOLA. Si matas a una araña y dice "Armadillo",
        // significa que el script de la Araña está mandando la palabra equivocada.
        Debug.Log("El UnlockManager registró la muerte de: " + enemyType);

        if (enemyType == "Armadillo" && !armadilloUnlocked)
        {
            armadilloKills++;
            if (UIManager.Instance != null) UIManager.Instance.UpdateKillCounter("Armadillo", armadilloKills);
            
            if (armadilloKills >= killsRequired)
            {
                armadilloUnlocked = true;
                if (playerGun != null) playerGun.UnlockTransformation(armadilloCard);
            }
        }
        else if (enemyType == "Spider" && !spiderUnlocked)
        {
            spiderKills++;
            if (UIManager.Instance != null) UIManager.Instance.UpdateKillCounter("Spider", spiderKills);
            
            if (spiderKills >= killsRequired)
            {
                spiderUnlocked = true;
                if (playerGun != null) playerGun.UnlockTransformation(spiderCard);
            }
        }
    }
}