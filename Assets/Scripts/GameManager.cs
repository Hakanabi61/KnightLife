using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

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
    public AudioClip winSound;
    public AudioClip fleeSound;
    public AudioClip failSound;
    public AudioClip coinSound;
    public AudioSource musicSource;

    [Header("Kampf-Einstellungen")]
    public float barSpeed = 2f;

    [Header("Level Up Effect")]
    public GameObject levelUpEffect;
    public AudioClip levelUpSound;

    [Header("Shop Items Referenz")]
    public ShopItem defaultHealingPotion;

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
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (musicSource != null && !musicSource.isPlaying) musicSource.Play();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.GetComponent<PlayerController>();
                Debug.Log("✅ Player gefunden:  " + playerObj.name);
            }
            else
            {
                Debug.LogError("❌ KEIN PLAYER GEFUNDEN!");
            }
        }

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

        UpdateUI();
        UpdatePotionUI();

        if (playerStats != null)
        {
            UpdateLevelUI(playerStats.level, playerStats.currentXP, playerStats.maxXP);
        }

        Invoke("UpdatePotionUI", 0.5f);
    }

    void Update()
    {
        if (isBattling && attackBar != null)
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

            Debug.Log("✅ ChoiceButtons aktiviert!");
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

        if (attackBar != null)
        {
            attackBar.gameObject.SetActive(false);
        }

        float hitQuality = attackBar != null ? attackBar.value : 50f;
        int baseDamage = playerStats.attack;

        bool isCritical = hitQuality >= 80f;
        int finalDamage = (int)(baseDamage * (hitQuality / 100f));
        finalDamage = Mathf.Max(1, finalDamage);

        if (isCritical)
        {
            finalDamage = (int)(finalDamage * 1.5f);
        }

        if (player != null)
        {
            Animator animator = player.GetComponent<Animator>();

            if (isCritical)
            {
                if (animator != null)
                {
                    animator.SetTrigger("AirSlash");
                    Debug.Log("⚡ AirSlash Animation!");
                }
            }
            else
            {
                player.PlayAttackAnimation();
            }
        }

        if (isCritical)
        {
            ShowBattleText("💥 KRITISCH! " + finalDamage + " Schaden!");
        }
        else
        {
            ShowBattleText("Du triffst für " + finalDamage + " Schaden!");
        }

        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
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

        int damage = Mathf.Max(1, currentEnemy.attack - playerStats.defense);
        playerStats.TakeDamage(damage);

        ShowBattleText(currentEnemy.characterName + " greift an!  -" + damage + " HP");

        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        if (playerStats.currentHP <= 0)
        {
            Debug.Log("💀 Player besiegt!");
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
            Debug.Log("❌ Kein Inventar gefunden!");
            return;
        }

        if (PlayerInventory.instance.potions.Count == 0)
        {
            Debug.Log("❌ Keine Tränke vorhanden!");
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
                Debug.Log("💀 Player besiegt!");
                ShowBattleText("Du wurdest besiegt!");
                Invoke("GameOver", 2f);
            }
        }
    }

    void GameOver()
    {
        Debug.Log("🎮 GAME OVER");
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