using UnityEngine;
using System.Collections;

/// <summary>
/// Контроллер персонажа для автобаттлера
/// Управляет AI, движением и боем
/// </summary>
public class CharacterController : MonoBehaviour
{
    [Header("Конфигурация")]
    [Tooltip("Конфиг персонажа со всеми характеристиками")]
    [SerializeField] private CharacterConfig config;
    
    [Header("Ссылки")]
    [SerializeField] private CharacterAnimator characterAnimator;
    [SerializeField] private HealthBar healthBar;
    
    [Header("Настройки")]
    [Tooltip("Показывать отладочную информацию")]
    [SerializeField] private bool showDebug = false;
    
    // Текущие характеристики
    private int currentHealth;
    private bool isDead = false;
    private bool isInBattle = false;
    
    // AI и бой
    private CharacterController currentTarget;
    private float attackCooldownTimer = 0f;
    
    // Движение
    private Vector2 targetPosition;
    private bool isMoving = false;

    public enum CharacterState
    {
        Idle,       // Стоит
        Moving,     // Движется
        Attacking,  // Атакует
        Dead        // Мёртв
    }
    
    private CharacterState currentState = CharacterState.Idle;

    private void Awake()
    {
        if (characterAnimator == null)
        {
            characterAnimator = GetComponent<CharacterAnimator>();
        }
        
        if (healthBar == null)
        {
            healthBar = GetComponentInChildren<HealthBar>();
        }
    }

    private void Start()
    {
        Initialize();
    }

    private void Update()
    {
        if (isDead || !isInBattle)
            return;

        // Обновляем кулдаун атаки
        if (attackCooldownTimer > 0)
        {
            attackCooldownTimer -= Time.deltaTime;
        }

        // AI логика
        UpdateAI();
    }

    // ═══════════════════════════════════════════════════════════
    // ИНИЦИАЛИЗАЦИЯ
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Инициализация персонажа
    /// </summary>
    public void Initialize()
    {
        if (config == null)
        {
            Debug.LogError($"CharacterConfig не назначен для {gameObject.name}!");
            return;
        }

        currentHealth = config.maxHealth;
        isDead = false;
        currentState = CharacterState.Idle;
        
        if (characterAnimator != null)
        {
            characterAnimator.PlayIdle();
        }
        
        // Обновляем HP бар
        UpdateHealthBar();

        if (showDebug)
        {
            Debug.Log($"✅ {config.characterName} инициализирован. HP: {currentHealth}");
        }
    }

    /// <summary>
    /// Начать бой
    /// </summary>
    public void StartBattle()
    {
        isInBattle = true;
        
        if (showDebug)
        {
            Debug.Log($"⚔️ {config.characterName} вступил в бой!");
        }
    }

    // ═══════════════════════════════════════════════════════════
    // AI ЛОГИКА
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Основная AI логика
    /// </summary>
    private void UpdateAI()
    {
        // Если нет цели - ищем
        if (currentTarget == null || currentTarget.IsDead())
        {
            FindNearestEnemy();
        }

        // Если цель найдена
        if (currentTarget != null)
        {
            float distanceToTarget = GetDistanceToTarget();
            
            if (showDebug && Time.frameCount % 60 == 0) // Раз в секунду
            {
                Debug.Log($"📊 {config.characterName} [Team:{config.team}] → Цель: {currentTarget.GetCharacterName()} [Team:{currentTarget.GetTeam()}], Дистанция: {distanceToTarget:F1}, Range: {config.attackRange}");
            }
            
            // Если в радиусе атаки - атакуем
            if (distanceToTarget <= config.attackRange)
            {
                Attack();
            }
            // Иначе двигаемся к цели
            else
            {
                MoveToTarget();
            }
        }
        else
        {
            // Нет врагов - Idle
            SetState(CharacterState.Idle);
            
            if (showDebug && Time.frameCount % 120 == 0)
            {
                Debug.Log($"😴 {config.characterName} не нашёл врагов...");
            }
        }
    }

    /// <summary>
    /// Найти ближайшего врага
    /// </summary>
    private void FindNearestEnemy()
    {
        CharacterController[] allCharacters = FindObjectsOfType<CharacterController>();
        CharacterController nearest = null;
        float minDistance = float.MaxValue;

        foreach (var character in allCharacters)
        {
            // Пропускаем себя, мёртвых и союзников
            if (character == this || character.IsDead() || character.GetTeam() == GetTeam())
                continue;

            float distance = Vector2.Distance(transform.position, character.transform.position);
            
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = character;
            }
        }

        currentTarget = nearest;
        
        if (showDebug && nearest != null)
        {
            Debug.Log($"🎯 {config.characterName} нашёл цель: {nearest.GetCharacterName()}");
        }
    }

    /// <summary>
    /// Двигаться к цели
    /// </summary>
    private void MoveToTarget()
    {
        if (currentTarget == null)
            return;

        SetState(CharacterState.Moving);
        
        Vector2 direction = (currentTarget.transform.position - transform.position).normalized;
        Vector2 newPosition = (Vector2)transform.position + direction * config.moveSpeed * Time.deltaTime;
        
        if (showDebug && Time.frameCount % 60 == 0) // Каждую секунду
        {
            float dist = GetDistanceToTarget();
            Debug.Log($"🏃 {config.characterName} движется к {currentTarget.GetCharacterName()}. Дистанция: {dist:F1}, Скорость: {config.moveSpeed}");
        }
        
        GetComponent<RectTransform>().position = newPosition;
    }

    /// <summary>
    /// Атаковать цель
    /// </summary>
    private void Attack()
    {
        if (currentTarget == null || isDead)
            return;

        // Проверяем кулдаун
        if (attackCooldownTimer > 0)
        {
            SetState(CharacterState.Idle);
            
            if (showDebug && Time.frameCount % 60 == 0)
            {
                Debug.Log($"⏳ {config.characterName} ждёт кулдаун: {attackCooldownTimer:F1}с");
            }
            return;
        }

        SetState(CharacterState.Attacking);
        
        // Запускаем анимацию атаки
        if (characterAnimator != null)
        {
            characterAnimator.PlayAttack();
        }
        
        // Наносим урон
        currentTarget.TakeDamage(config.damage, this);
        
        // Устанавливаем кулдаун
        attackCooldownTimer = config.GetAttackCooldown();
        
        if (showDebug)
        {
            Debug.Log($"⚔️ {config.characterName} атаковал {currentTarget.GetCharacterName()} на {config.damage} урона!");
        }
    }

    // ═══════════════════════════════════════════════════════════
    // УРОН И СМЕРТЬ
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Получить урон
    /// </summary>
    public void TakeDamage(int damage, CharacterController attacker)
    {
        if (isDead)
            return;

        currentHealth -= damage;
        
        // Обновляем HP бар
        UpdateHealthBar();
        
        if (showDebug)
        {
            Debug.Log($"💥 {config.characterName} получил {damage} урона! HP: {currentHealth}/{config.maxHealth}");
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Смерть персонажа
    /// </summary>
    private void Die()
    {
        if (isDead)
            return;

        isDead = true;
        currentHealth = 0;
        SetState(CharacterState.Dead);
        
        if (characterAnimator != null)
        {
            characterAnimator.PlayDeath();
        }

        if (showDebug)
        {
            Debug.Log($"💀 {config.characterName} погиб!");
        }

        // Можно удалить через несколько секунд
        // Destroy(gameObject, 3f);
    }

    // ═══════════════════════════════════════════════════════════
    // СОСТОЯНИЯ
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Установить состояние
    /// </summary>
    private void SetState(CharacterState newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;
        
        // Обновляем анимацию
        if (characterAnimator != null)
        {
            switch (currentState)
            {
                case CharacterState.Idle:
                    characterAnimator.PlayIdle();
                    break;
                case CharacterState.Moving:
                    characterAnimator.PlayMove();
                    break;
                case CharacterState.Attacking:
                    // Анимация уже запущена в Attack()
                    break;
                case CharacterState.Dead:
                    // Анимация уже запущена в Die()
                    break;
            }
        }
    }

    // ═══════════════════════════════════════════════════════════
    // ГЕТТЕРЫ
    // ═══════════════════════════════════════════════════════════

    public CharacterConfig GetConfig() => config;
    public string GetCharacterName() => config != null ? config.characterName : "Unknown";
    public int GetTeam() => config != null ? config.team : 0;
    public bool IsDead() => isDead;
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => config != null ? config.maxHealth : 100;
    public CharacterState GetCurrentState() => currentState;
    
    public float GetHealthPercent()
    {
        if (config == null) return 0f;
        return (float)currentHealth / config.maxHealth;
    }

    private float GetDistanceToTarget()
    {
        if (currentTarget == null) return float.MaxValue;
        return Vector2.Distance(transform.position, currentTarget.transform.position);
    }

    // ═══════════════════════════════════════════════════════════
    // ПУБЛИЧНЫЕ МЕТОДЫ
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Установить конфиг
    /// </summary>
    public void SetConfig(CharacterConfig newConfig)
    {
        config = newConfig;
        Initialize();
    }

    /// <summary>
    /// Вылечить персонажа
    /// </summary>
    public void Heal(int amount)
    {
        if (isDead) return;
        
        currentHealth = Mathf.Min(currentHealth + amount, config.maxHealth);
        
        // Обновляем HP бар
        UpdateHealthBar();
        
        if (showDebug)
        {
            Debug.Log($"💚 {config.characterName} вылечен на {amount}. HP: {currentHealth}/{config.maxHealth}");
        }
    }
    
    /// <summary>
    /// Обновить HP бар
    /// </summary>
    private void UpdateHealthBar()
    {
        if (healthBar != null && config != null)
        {
            healthBar.SetHealth(currentHealth, config.maxHealth);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // ОТЛАДКА
    // ═══════════════════════════════════════════════════════════

    private void OnDrawGizmos()
    {
        if (!showDebug || config == null)
            return;

        // Рисуем радиус атаки
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, config.attackRange);

        // Рисуем линию к цели
        if (currentTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, currentTarget.transform.position);
        }
    }
}

