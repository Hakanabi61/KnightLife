using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Debug")]
    public bool enableDebugLogs = true;

    [Header("Player Referenzen")]
    public GameObject player;
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
    public GameObject[] choiceButtons;
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
    public int defaultHitQuality = 50;
    [Space(10)]
    [Tooltip("Verzögerung bis Enemy angreift nach Player Hit")]
    public float enemyAttackDelay = 1.5f;
    [Tooltip("Verzögerung bis nächste Player Runde startet")]
    public float nextTurnDelay = 1f;
    [Tooltip("Verzögerung bis Win Battle Screen")]
    public float winBattleDelay = 1.5f;
    [Tooltip("Verzögerung bis Battle komplett endet")]
    public float endBattleDelay = 2f;

    // Private Variables
    private CharacterStats currentEnemy;
    private bool isAttackBarActive = false;
    private bool barMovingRight = true;

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
        if (enableDebugLogs) Debug.Log("🎮 GameManager START");

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (playerStats == null && player != null)
        {
            playerStats = player.GetComponent<CharacterStats>();
        }

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
            attackBar.minValue = 0;
            attackBar.maxValue = 100;
            attackBar.value = 0;
        }

        UpdateUI();
        UpdatePotionUI();
    }

    void Update()
    {
        // Attack Bar Animation
        if (isAttackBarActive && attackBar != null)
        {
            if (barMovingRight)
            {
                attackBar.value += barSpeed * Time.deltaTime * 100f;
                if (attackBar.value >= attackBar.maxValue)
                {
                    barMovingRight = false;
                }
            }
            else
            {
                attackBar.value -= barSpeed * Time.deltaTime * 100f;
                if (attackBar.value <= attackBar.minValue)
                {
                    barMovingRight = true;
                }
            }

            // HIT Button sichtbar wenn Bar aktiv
            if (hitButton != null && !hitButton.activeSelf)
            {
                hitButton.SetActive(true);
            }
        }
    }

    // ============================================
    // BATTLE SYSTEM
    // ============================================

    public void TriggerEncounter(CharacterStats enemy)
    {
        if (enableDebugLogs) Debug.Log($"🎮 TriggerEncounter: {enemy.characterName}");

        currentEnemy = enemy;

        // Battle Panel öffnen
        if (battlePanel != null)
        {
            battlePanel.SetActive(true);
        }

        // Enemy Info anzeigen
        if (enemyNameText != null)
        {
            enemyNameText.text = $"{enemy.characterName} Lvl {enemy.level} (HP: {enemy.currentHP})";
        }

        if (enemyHPBar != null)
        {
            enemyHPBar.maxValue = enemy.maxHP;
            enemyHPBar.value = enemy.currentHP;
        }

        // HIT Button verbinden
        if (hitButton != null)
        {
            Button btn = hitButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(OnHitButtonPressed);
                if (enableDebugLogs) Debug.Log("✅ HIT Button connected");
            }
            hitButton.SetActive(false);
        }

        // Attack Bar starten
        if (attackBar != null)
        {
            attackBar.gameObject.SetActive(true);
            attackBar.value = 0;
            isAttackBarActive = true;
            barMovingRight = true;
            if (enableDebugLogs) Debug.Log("✅ Attack Bar started");
        }

        ShowBattleText($"{enemy.characterName} erscheint!");
    }

    public void OnHitButtonPressed()
    {
        if (enableDebugLogs) Debug.Log("========== HIT BUTTON PRESSED ==========");

        // DEBUG LOGS
        if (enableDebugLogs)
        {
            Debug.Log($"✓ currentEnemy: {(currentEnemy != null ? currentEnemy.characterName : "NULL!! !")}");
            Debug.Log($"✓ playerStats: {(playerStats != null ? "OK" : "NULL!! !")}");
            Debug.Log($"✓ isAttackBarActive: {isAttackBarActive}");
            Debug.Log($"✓ attackBar:  {(attackBar != null ? $"Value={attackBar.value}" : "NULL!!!")}");
        }

        if (!isAttackBarActive || attackBar == null)
        {
            Debug.LogWarning("⚠️ Attack bar not active!");
            return;
        }

        if (currentEnemy == null)
        {
            Debug.LogError("❌ currentEnemy is NULL!  Battle cannot proceed!");
            return;
        }

        if (playerStats == null)
        {
            Debug.LogError("❌ playerStats is NULL! Battle cannot proceed!");
            return;
        }

        if (enableDebugLogs) Debug.Log("✓ All checks passed, processing hit...");

        // Button verstecken
        if (hitButton != null)
        {
            hitButton.SetActive(false);
            if (enableDebugLogs) Debug.Log("✓ Hit button hidden");
        }

        // Bar stoppen
        isAttackBarActive = false;
        if (enableDebugLogs) Debug.Log("✓ Attack bar stopped");

        // Damage berechnen basierend auf Bar Position
        float barValue = attackBar.value;
        float damageMultiplier = 1f;
        bool isCritical = false;

        // Timing Zones
        if (barValue >= 45f && barValue <= 55f)
        {
            // PERFECT - Grüne Zone (Mitte)
            damageMultiplier = 2f;
            isCritical = true;
            if (enableDebugLogs) Debug.Log("⭐ CRITICAL HIT!");
        }
        else if (barValue >= 30f && barValue <= 70f)
        {
            // GOOD - Gelbe Zone
            damageMultiplier = 1f;
            if (enableDebugLogs) Debug.Log("✓ Normal Hit");
        }
        else
        {
            // MISS - Schwarze Zone
            damageMultiplier = 0.5f;
            if (enableDebugLogs) Debug.Log("✗ Weak Hit");
        }

        // Schaden berechnen
        if (playerStats != null && currentEnemy != null)
        {
            int baseDamage = playerStats.attack;
            int finalDamage = Mathf.RoundToInt(baseDamage * damageMultiplier);
            finalDamage = Mathf.Max(1, finalDamage - currentEnemy.defense);

            if (enableDebugLogs) Debug.Log($"💥 Damage:  {finalDamage} (Base: {baseDamage}, x{damageMultiplier})");

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

            // Visual Effects
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

                BattleEffects.instance.PlayEnemyDamageEffect(currentEnemy.gameObject);
            }

            // Schaden anwenden
            currentEnemy.TakeDamage(finalDamage);

            // UI Update
            if (enemyHPBar != null)
            {
                enemyHPBar.value = currentEnemy.currentHP;
            }

            if (enemyNameText != null)
            {
                enemyNameText.text = $"{currentEnemy.characterName} Lvl {currentEnemy.level} (HP:  {currentEnemy.currentHP})";
            }

            ShowBattleText($"Du triffst für {finalDamage} Schaden!");

            // Enemy besiegt? 
            if (currentEnemy.currentHP <= 0)
            {
                if (enableDebugLogs) Debug.Log("🎉 Enemy defeated!");
                Invoke("WinBattle", winBattleDelay);
            }
            else
            {
                // Enemy greift zurück
                Invoke("EnemyCounterAttack", enemyAttackDelay);
            }
        }
    }

    void EnemyCounterAttack()
    {
        if (currentEnemy == null || playerStats == null) return;

        if (enableDebugLogs) Debug.Log($"👹 {currentEnemy.characterName} greift an!");

        // Enemy Attack Animation
        Animator enemyAnimator = currentEnemy.GetComponent<Animator>();
        if (enemyAnimator != null)
        {
            enemyAnimator.SetTrigger("Attack");
        }

        int damage = Mathf.Max(1, currentEnemy.attack - playerStats.defense);
        playerStats.TakeDamage(damage);

        ShowBattleText($"{currentEnemy.characterName} greift an!  -{damage} HP");

        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        // Player damage effect
        if (BattleEffects.instance != null && player != null)
        {
            BattleEffects.instance.PlayEnemyDamageEffect(player.gameObject);
        }

        UpdateUI();

        if (playerStats.currentHP <= 0)
        {
            if (enableDebugLogs) Debug.Log("💀 Player besiegt!");
            ShowBattleText("Du wurdest besiegt!");
            Invoke("GameOver", endBattleDelay);
        }
        else
        {
            // Nächste Runde - Attack Bar wieder starten
            Invoke("PlayerTurnStart", nextTurnDelay);
        }
    }

    void PlayerTurnStart()
    {
        if (enableDebugLogs) Debug.Log("⚔️ Player Turn");

        if (attackBar != null)
        {
            attackBar.value = 0;
            isAttackBarActive = true;
            barMovingRight = true;
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

        Invoke("EndBattle", endBattleDelay);
    }

    void EndBattle()
    {
        if (enableDebugLogs) Debug.Log("🏁 Battle Ended");

        // Battle UI verstecken
        if (battlePanel != null)
        {
            battlePanel.SetActive(false);
        }

        if (attackBar != null)
        {
            attackBar.gameObject.SetActive(false);
        }

        if (hitButton != null)
        {
            hitButton.SetActive(false);
        }

        isAttackBarActive = false;

        // NEU: Benachrichtige DungeonManager
        if (DungeonManager.instance != null)
        {
            bool playerWon = (playerStats != null && playerStats.currentHP > 0);
            if (enableDebugLogs) Debug.Log($"✅ Calling DungeonManager.OnBattleComplete({playerWon})");
            DungeonManager.instance.OnBattleComplete(playerWon);
        }
        else
        {
            if (enableDebugLogs) Debug.Log("⚠️ DungeonManager. instance is NULL (probably in SampleScene)");
        }
    }

    void GameOver()
    {
        if (enableDebugLogs) Debug.Log("💀 GAME OVER");

        ShowBattleText("GAME OVER");

        // TODO: Game Over Screen
    }

    public void OnFleeButtonPressed()
    {
        ShowBattleText("Du bist geflohen!");

        if (audioSource != null && fleeSound != null)
        {
            audioSource.PlayOneShot(fleeSound);
        }

        Invoke("EndBattle", endBattleDelay);
    }

    public void ShowBattleText(string text)
    {
        if (battleText != null)
        {
            battleText.text = text;
            if (enableDebugLogs) Debug.Log($"📜 Battle Text: {text}");
        }
    }

    // ============================================
    // UI UPDATES
    // ============================================

    public void UpdateUI()
    {
        if (playerStats == null) return;

        if (hpText != null)
        {
            hpText.text = $"HP: {playerStats.currentHP}/{playerStats.maxHP}";
        }

        if (playerHPText != null)
        {
            playerHPText.text = $"HP: {playerStats.currentHP}/{playerStats.maxHP}";
        }

        if (levelText != null)
        {
            levelText.text = $"Level: {playerStats.level}";
        }

        if (goldText != null)
        {
            goldText.text = $"Gold: {playerStats.gold}";
        }

        if (levelBar != null)
        {
            levelBar.maxValue = playerStats.xpToNextLevel;
            levelBar.value = playerStats.currentXP;
        }
    }

    public void UpdatePotionUI()
    {
        if (potionCountText != null && PlayerInventory.instance != null)
        {
            int potionCount = PlayerInventory.instance.potions.Count;
            potionCountText.text = $"x{potionCount}";
        }
    }

    public void AddGold(int amount)
    {
        if (playerStats != null)
        {
            playerStats.gold += amount;
            PlayerPrefs.SetInt("Gold", playerStats.gold);
            UpdateUI();
            if (enableDebugLogs) Debug.Log($"💰 +{amount} Gold!  Total: {playerStats.gold}");
        }
    }

    public void PlayCoinSound()
    {
        if (audioSource != null && coinSound != null)
        {
            audioSource.PlayOneShot(coinSound);
        }
    }

    // ============================================
    // STORY SYSTEM (Optional)
    // ============================================

    public void ShowStory(string text)
    {
        if (storyPanel != null)
        {
            storyPanel.SetActive(true);
        }

        if (storyText != null)
        {
            storyText.text = text;
        }
    }

    public void HideStory()
    {
        if (storyPanel != null)
        {
            storyPanel.SetActive(false);
        }
    }

    // ============================================
    // ZUSÄTZLICHE UI UPDATES (für CharacterStats)
    // ============================================

    public void UpdatePlayerHPText(int current, int max)
    {
        if (playerHPText != null)
        {
            playerHPText.text = $"HP: {current}/{max}";
        }

        if (hpText != null)
        {
            hpText.text = $"HP: {current}/{max}";
        }
    }

    public void UpdateLevelUI(int level, int currentXP, int maxXP)
    {
        if (levelText != null)
        {
            levelText.text = $"Level: {level}";
        }

        if (levelBar != null)
        {
            levelBar.maxValue = maxXP;
            levelBar.value = currentXP;
        }
    }

    public void ShowLevelUpEffect()
    {
        if (enableDebugLogs) Debug.Log("🎉 LEVEL UP EFFECT!");

        if (audioSource != null && winSound != null)
        {
            audioSource.PlayOneShot(winSound);
        }
    }
}