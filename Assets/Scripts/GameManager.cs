using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Debug")]
    public bool enableDebugLogs = false;

    [Header("Player Referenzen")]
    public PlayerController player;
    public CharacterStats playerStats;

    [Header("Story System")]
    public GameObject storyPanel;
    public TextMeshProUGUI storyText;

    [Header("Anzeigen (UI)")]
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI highscoreText;

    [Header("Buttons")]
    public GameObject choiceButtons;
    public Button shopButton;
    public Button potionButton;

    [Header("Battle UI")]
    public GameObject battlePanel;
    public TextMeshProUGUI battleText;
    public TextMeshProUGUI enemyNameText;
    public Slider enemyHPBar;
    public Slider attackBar;
    public GameObject hitButton;

    [Header("Player UI")]
    public Slider levelBar;
    public TextMeshProUGUI playerHPText;
    public TextMeshProUGUI potionCountText;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hitSound;
    public AudioClip criticalHitSound;
    public AudioClip winSound;
    public AudioClip fleeSound;
    public AudioClip failSound;
    public AudioClip coinSound;
    public AudioSource musicSource;

    [Header("Kampf-Einstellungen")]
    public float barSpeed = 2f;
    public float defaultHitQuality = 50f;

    [Header("Level Up Effect")]
    public GameObject levelUpEffect;
    public AudioClip levelUpSound;

    [Header("Shop Items Referenz")]
    public ShopItem defaultHealingPotion;

    [Header("Enemy Animations")]
    public bool enableEnemyAnimations = true;

    private bool isBattling = false;
    private CharacterStats currentEnemy;
    public int highscore = 1;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Performance Settings
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (musicSource != null && !musicSource.isPlaying) musicSource.Play();

        // Player finden
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.GetComponent<PlayerController>();
                if (enableDebugLogs) Debug.Log("✅ Player gefunden:  " + playerObj.name);
            }
            else
            {
                Debug.LogError("❌ KEIN PLAYER GEFUNDEN!");
            }
        }

        // PlayerStats finden
        if (playerStats == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerStats = playerObj.GetComponent<CharacterStats>();
            }
        }

        highscore = PlayerPrefs.GetInt("Highscore", 1);

        if (playerStats != null)
        {
            playerStats.gold = PlayerPrefs.GetInt("Gold", 0);
        }

        // UI initialisieren
        if (storyPanel != null) storyPanel.SetActive(false);
        if (hitButton != null) hitButton.SetActive(false);
        if (attackBar != null) attackBar.gameObject.SetActive(false);
        if (battlePanel != null) battlePanel.SetActive(false);
        if (choiceButtons != null) choiceButtons.SetActive(false);

        if (potionButton != null)
        {
            potionButton.onClick.AddListener(UsePotionInBattle);
            potionButton.gameObject.SetActive(false);
        }

        // UI Validierung
        ValidateUIConnections();

        UpdateUI();
        UpdatePotionUI();

        if (playerStats != null)
        {
            UpdateLevelUI(playerStats.level, playerStats.currentXP, playerStats.maxXP);
        }

        Invoke("UpdatePotionUI", 0.5f);
    }

    void ValidateUIConnections()
    {
        if (enableDebugLogs) Debug.Log("=== UI CONNECTION VALIDATION ===");

        if (playerHPText == null) Debug.LogWarning("❌ playerHPText is not connected!");
        if (levelBar == null) Debug.LogWarning("❌ levelBar is not connected!");
        if (battlePanel == null) Debug.LogWarning("❌ battlePanel is not connected!");
        if (battleText == null) Debug.LogWarning("❌ battleText is not connected!");
        if (criticalHitSound == null) Debug.LogWarning("⚠️ criticalHitSound is not assigned (will use hitSound)!");

        if (enableDebugLogs) Debug.Log("=== END OF VALIDATION ===");
    }

    void Update()
    {
        if (isBattling && attackBar != null && attackBar.gameObject.activeSelf)
        {
            attackBar.value = Mathf.PingPong(Time.time * barSpeed * 100, 100);
        }
    }

    public void AddGold(int amount)
    {
        if (playerStats != null)
        {
            playerStats.gold += amount;
            PlayerPrefs.SetInt("Gold", playerStats.gold);
            PlayerPrefs.Save();
            UpdateUI();

            if (audioSource != null && coinSound != null)
            {
                audioSource.PlayOneShot(coinSound);
            }
        }
    }

    public void UpdateUI()
    {
        if (playerStats != null && goldText != null)
        {
            goldText.text = "Gold: " + playerStats.gold;
        }

        if (highscoreText != null)
        {
            highscoreText.text = "Highscore:  " + highscore;
        }
    }

    public void UpdatePlayerHPText(int current, int max)
    {
        if (playerHPText != null)
        {
            playerHPText.text = "HP: " + current + " / " + max;
        }

        if (hpText != null)
        {
            hpText.text = "HP: " + current + " / " + max;
        }
    }

    public void UpdateLevelUI(int level, int currentXP, int maxXP)
    {
        if (levelText != null)
        {
            levelText.text = "Level " + level;
        }

        if (levelBar != null)
        {
            levelBar.maxValue = maxXP;
            levelBar.value = currentXP;
        }
    }

    public void ShowLevelUpEffect()
    {
        if (levelUpEffect != null && player != null)
        {
            GameObject effect = Instantiate(levelUpEffect, player.transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }

        if (audioSource != null && levelUpSound != null)
        {
            audioSource.PlayOneShot(levelUpSound);
        }
    }

    public void StartBattle(CharacterStats enemy)
    {
        isBattling = true;
        currentEnemy = enemy;

        if (player != null)
        {
            player.StopRunning();
        }

        if (battlePanel != null)
        {
            battlePanel.SetActive(true);
        }

        if (enemyNameText != null)
        {
            enemyNameText.text = enemy.characterName + " Lvl " + enemy.level + " (HP: " + enemy.currentHP + ")";
        }

        if (enemyHPBar != null)
        {
            enemyHPBar.maxValue = enemy.maxHP;
            enemyHPBar.value = enemy.currentHP;
        }

        ShowBattleText(enemy.characterName + " blockiert den Weg!");

        if (choiceButtons == null)
        {
            choiceButtons = GameObject.Find("ChoiceButtons");
        }

        if (choiceButtons != null)
        {
            choiceButtons.SetActive(true);

            for (int i = 0; i < choiceButtons.transform.childCount; i++)
            {
                choiceButtons.transform.GetChild(i).gameObject.SetActive(true);
            }

            if (enableDebugLogs) Debug.Log("✅ ChoiceButtons aktiviert!");
        }
        else
        {
            Debug.LogError("❌ ChoiceButtons NICHT GEFUNDEN!");
        }

        if (hitButton != null)
        {
            hitButton.SetActive(true);
        }

        if (attackBar != null)
        {
            attackBar.gameObject.SetActive(true);
            attackBar.value = 0;
        }

        if (potionButton != null)
        {
            potionButton.gameObject.SetActive(true);
            UpdatePotionUI();
        }
    }

    public void OnAttackButtonPressed()
    {
        if (currentEnemy == null || playerStats == null) return;

        if (hitButton != null)
        {
            hitButton.SetActive(false);
        }

        // WICHTIG: Wert VORHER speichern!
        float hitQuality = attackBar != null ? attackBar.value : defaultHitQuality;
        hitQuality = Mathf.Clamp(hitQuality, 0f, 100f);

        if (attackBar != null)
        {
            attackBar.gameObject.SetActive(false);
        }

        int baseDamage = playerStats.attack;

        // ⚡ KRITISCHE ZONE:  40-60% = KRITISCH!  ⚡
        bool isCritical = (hitQuality >= 40f && hitQuality <= 60f);

        // Damage Berechnung
        float damageMultiplier;

        if (isCritical)
        {
            // KRITISCH: 40-60% = 2x Schaden! 
            damageMultiplier = 2.0f;
        }
        else if (hitQuality >= 25f && hitQuality <= 75f)
        {
            // Normal: 25-75% = 1x Schaden
            damageMultiplier = 1.0f;
        }
        else
        {
            // Schwach: 0-25% oder 75-100% = 0. 5x Schaden
            damageMultiplier = 0.5f;
        }

        int finalDamage = (int)(baseDamage * damageMultiplier);
        finalDamage = Mathf.Max(1, finalDamage);

        // Animation
        if (player != null)
        {
            Animator animator = player.GetComponent<Animator>();

            if (isCritical)
            {
                // KRITISCH = ComboAttack
                if (animator != null)
                {
                    animator.SetTrigger("ComboAttack");
                    if (enableDebugLogs) Debug.Log("💥 ComboAttack Animation!");
                }
            }
            else
            {
                // Normal/Schwach = AirSlash
                if (animator != null)
                {
                    animator.SetTrigger("AirSlash");
                    if (enableDebugLogs) Debug.Log("⚔️ AirSlash Animation!");
                }
            }
        }

        // Battle Text
        if (isCritical)
        {
            ShowBattleText("💥 KRITISCH! " + finalDamage + " Schaden!  (Perfect:  " + hitQuality.ToString("F0") + "%)");
        }
        else if (damageMultiplier < 1f)
        {
            ShowBattleText("😞 Schwacher Treffer...  " + finalDamage + " Schaden");
        }
        else
        {
            ShowBattleText("Du triffst für " + finalDamage + " Schaden!");
        }

        // Sound
        if (audioSource != null)
        {
            if (isCritical && criticalHitSound != null)
            {
                audioSource.PlayOneShot(criticalHitSound);
            }
            else if (hitSound != null)
            {
                audioSource.PlayOneShot(hitSound);
            }
        }

        // Camera Shake
        if (isCritical && CameraShake.instance != null)
        {
            CameraShake.instance.CriticalHitShake();
        }

        // ✨ VISUAL EFFECTS ✨
        if (BattleEffects.instance != null && currentEnemy != null)
        {
            Vector3 effectPosition = currentEnemy.transform.position;

            if (isCritical)
            {
                BattleEffects.instance.PlayCriticalHitEffect(effectPosition);
            }
            else if (damageMultiplier < 1f)
            {
                BattleEffects.instance.PlayWeakHitEffect(effectPosition);
            }
            else
            {
                BattleEffects.instance.PlayNormalHitEffect(effectPosition);
            }

            // Enemy damage flash
            BattleEffects.instance.PlayEnemyDamageEffect(currentEnemy.gameObject);
        }

        currentEnemy.TakeDamage(finalDamage);

        if (enemyHPBar != null)
        {
            enemyHPBar.value = currentEnemy.currentHP;
        }

        if (enemyNameText != null)
        {
            enemyNameText.text = currentEnemy.characterName + " Lvl " + currentEnemy.level + " (HP: " + currentEnemy.currentHP + ")";
        }

        if (currentEnemy.currentHP <= 0)
        {
            Invoke("WinBattle", 1.5f);
        }
        else
        {
            Invoke("EnemyCounterAttack", 1.5f);
        }
    }

    void EnemyCounterAttack()
    {
        if (currentEnemy == null || playerStats == null) return;

        // Enemy Attack Animation
        Animator enemyAnimator = currentEnemy.GetComponent<Animator>();
        if (enemyAnimator != null)
        {
            enemyAnimator.SetTrigger("Attack");
        }

        int damage = Mathf.Max(1, currentEnemy.attack - playerStats.defense);
        playerStats.TakeDamage(damage);

        ShowBattleText(currentEnemy.characterName + " greift an!  -" + damage + " HP");

        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        // Player damage effect
        if (BattleEffects.instance != null && player != null)
        {
            BattleEffects.instance.PlayEnemyDamageEffect(player.gameObject);
        }

        if (playerStats.currentHP <= 0)
        {
            if (enableDebugLogs) Debug.Log("💀 Player besiegt!");
            ShowBattleText("Du wurdest besiegt!");
            Invoke("GameOver", 2f);
        }
        else
        {
            Invoke("PlayerTurnStart", 1f);
        }
    }

    void PlayerTurnStart()
    {
        if (hitButton != null)
        {
            hitButton.SetActive(true);
        }

        if (attackBar != null)
        {
            attackBar.gameObject.SetActive(true);
            attackBar.value = 0;
        }

        ShowBattleText("Dein Zug!");
    }

    void WinBattle()
    {
        ShowBattleText("Du hast gewonnen!");

        if (playerStats != null && currentEnemy != null)
        {
            playerStats.GainXP(currentEnemy.xpReward);
            AddGold(currentEnemy.gold);
        }

        if (audioSource != null && winSound != null)
        {
            audioSource.PlayOneShot(winSound);
        }

        Invoke("EndBattle", 2f);
    }

    public void OnFleeButtonPressed()
    {
        ShowBattleText("Du bist geflohen!");

        if (audioSource != null && fleeSound != null)
        {
            audioSource.PlayOneShot(fleeSound);
        }

        Invoke("EndBattle", 1f);
    }

    void EndBattle()
    {
        isBattling = false;

        if (battlePanel != null)
        {
            battlePanel.SetActive(false);
        }

        if (hitButton != null)
        {
            hitButton.SetActive(false);
        }

        if (attackBar != null)
        {
            attackBar.gameObject.SetActive(false);
        }

        if (potionButton != null)
        {
            potionButton.gameObject.SetActive(false);
        }

        if (choiceButtons != null)
        {
            choiceButtons.SetActive(false);
        }

        if (player != null)
        {
            player.StartRunning();
        }

        currentEnemy = null;
    }

    public void ShowBattleText(string text)
    {
        if (battleText != null)
        {
            battleText.text = text;
        }
    }

    public void ShowStory(string text)
    {
        if (storyPanel != null && storyText != null)
        {
            storyPanel.SetActive(true);
            storyText.text = text;

            if (player != null)
            {
                player.StopRunning();
            }

            Invoke("HideStory", 3f);
        }
    }

    void HideStory()
    {
        if (storyPanel != null)
        {
            storyPanel.SetActive(false);
        }

        if (player != null)
        {
            player.StartRunning();
        }
    }

    void UsePotionInBattle()
    {
        if (PlayerInventory.instance == null)
        {
            if (enableDebugLogs) Debug.Log("❌ Kein Inventar gefunden!");
            return;
        }

        if (PlayerInventory.instance.potions.Count == 0)
        {
            if (enableDebugLogs) Debug.Log("❌ Keine Tränke vorhanden!");
            ShowBattleText("Keine Tränke!");
            return;
        }

        int healAmount = PlayerInventory.instance.potions[0].potion.healAmount;
        PlayerInventory.instance.UsePotion(0);
        UpdatePotionUI();
        ShowBattleText("Trank benutzt!  +" + healAmount + " HP");
        Invoke("EnemyAttackTurn", 1f);
    }

    public void UpdatePotionUI()
    {
        if (PlayerInventory.instance == null) return;

        int potionCount = 0;
        foreach (var stack in PlayerInventory.instance.potions)
        {
            potionCount += stack.count;
        }

        if (potionButton != null)
        {
            TextMeshProUGUI btnText = potionButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.text = "💊 Trank (" + potionCount + ")";
            }
            potionButton.interactable = (potionCount > 0);
        }

        if (potionCountText != null)
        {
            potionCountText.text = "💊 Tränke:  " + potionCount;
        }
    }

    void EnemyAttackTurn()
    {
        if (currentEnemy != null && playerStats != null)
        {
            int damage = Mathf.Max(1, currentEnemy.attack - playerStats.defense);
            playerStats.TakeDamage(damage);
            ShowBattleText(currentEnemy.characterName + " greift an! -" + damage + " HP");

            if (playerStats.currentHP <= 0)
            {
                if (enableDebugLogs) Debug.Log("💀 Player besiegt!");
                ShowBattleText("Du wurdest besiegt!");
                Invoke("GameOver", 2f);
            }
        }
    }

    void GameOver()
    {
        if (enableDebugLogs) Debug.Log("🎮 GAME OVER");
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    public void TriggerEncounter(GameObject enemy)
    {
        if (enemy != null)
        {
            CharacterStats enemyStats = enemy.GetComponent<CharacterStats>();
            if (enemyStats != null)
            {
                StartBattle(enemyStats);
            }
        }
    }

    public void TriggerEncounter(CharacterStats enemyStats)
    {
        if (enemyStats != null)
        {
            StartBattle(enemyStats);
        }
    }

    public void PlayCoinSound()
    {
        if (audioSource != null && coinSound != null)
        {
            audioSource.PlayOneShot(coinSound);
        }
    }
}