using UnityEngine;

/// <summary>
/// Конфигурация персонажа - ScriptableObject
/// Хранит все характеристики героя
/// </summary>
[CreateAssetMenu(fileName = "NewCharacter", menuName = "Game/Character Config", order = 1)]
public class CharacterConfig : ScriptableObject
{
    [Header("Основная информация")]
    [Tooltip("Название персонажа")]
    public string characterName = "Warrior";
    
    [Tooltip("Описание персонажа")]
    [TextArea(2, 4)]
    public string description = "";
    
    [Tooltip("Иконка персонажа")]
    public Sprite icon;
    
    [Header("Характеристики боя")]
    [Tooltip("Очки здоровья")]
    [Min(1)]
    public int maxHealth = 100;
    
    [Tooltip("Урон за атаку")]
    [Min(1)]
    public int damage = 10;
    
    [Tooltip("Скорость атаки (атак в секунду)")]
    [Range(0.1f, 5f)]
    public float attackSpeed = 1f;
    
    [Tooltip("Радиус атаки (дальность)")]
    [Min(0.5f)]
    public float attackRange = 1.5f;
    
    [Header("Характеристики передвижения")]
    [Tooltip("Скорость передвижения (единиц в секунду)")]
    [Min(0.1f)]
    public float moveSpeed = 2f;
    
    [Header("Дополнительно")]
    [Tooltip("Тип персонажа")]
    public CharacterType characterType = CharacterType.Melee;
    
    [Tooltip("Команда (0 = игрок, 1 = враг)")]
    [Range(0, 1)]
    public int team = 0;

    public enum CharacterType
    {
        Melee,      // Ближний бой
        Ranged,     // Дальний бой
        Tank,       // Танк
        Support     // Поддержка
    }
    
    /// <summary>
    /// Получить время между атаками в секундах
    /// </summary>
    public float GetAttackCooldown()
    {
        return 1f / attackSpeed;
    }
    
    /// <summary>
    /// Создать копию конфига (для модификаций в runtime)
    /// </summary>
    public CharacterConfig Clone()
    {
        CharacterConfig clone = ScriptableObject.CreateInstance<CharacterConfig>();
        clone.characterName = this.characterName;
        clone.description = this.description;
        clone.icon = this.icon;
        clone.maxHealth = this.maxHealth;
        clone.damage = this.damage;
        clone.attackSpeed = this.attackSpeed;
        clone.attackRange = this.attackRange;
        clone.moveSpeed = this.moveSpeed;
        clone.characterType = this.characterType;
        clone.team = this.team;
        return clone;
    }
}

