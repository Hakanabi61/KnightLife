using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "KnightLife/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Basic Info")]
    public string enemyName = "Goblin";
    public Sprite enemySprite;

    [Header("Stats")]
    public int level = 1;
    public int maxHealth = 50;
    public int attack = 10;
    public int defense = 5;

    [Header("Rewards")]
    public int goldReward = 30;
    public int xpReward = 15;

    [Header("Battle")]
    public RuntimeAnimatorController animatorController; // Enemy Animator
}