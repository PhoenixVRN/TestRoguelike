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
            
            if (showDebug && Time.frameCount % 60 == 0)
            {
                Debug.Log($"⏱️ {config.characterName} Update: Кулдаун уменьшается: {attackCooldownTimer:F2}с, State: {currentState}");
            }
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
                Debug.Log($"📊 {config.characterName} [Team:{config.team}] → Цель: {currentTarget.GetCharacterName()} [Team:{currentTarget.GetTeam()}], Дистанция: {distanceToTarget:F1}, Range: {config.attackRange}, Кулдаун: {attackCooldownTimer:F2}");
            }
            
            // Если в радиусе атаки - атакуем
            if (distanceToTarget <= config.attackRange)
            {
                Attack(); // Эта функция сама проверит кулдаун
            }
            // Если далеко И кулдаун закончился - двигаемся
            else if (attackCooldownTimer <= 0)
            {
                MoveToTarget();
            }
            // Иначе стоим на месте (кулдаун идёт, но цель далеко - можем подождать)
            else
            {
                // Можно либо стоять, либо всё равно двигаться
                // Вариант 1: Стоим
                // SetState(CharacterState.Idle);
                
                // Вариант 2: Двигаемся даже во время кулдауна (реалистичнее)
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

        // Устанавливаем состояние атаки (анимация зациклена, будет играть постоянно)
        SetState(CharacterState.Attacking);

        // Проверяем кулдаун для НАНЕСЕНИЯ УРОНА (не для анимации!)
        if (attackCooldownTimer > 0)
        {
            // Анимация играет, но урон не наносим (кулдаун)
            return;
        }

        // Кулдаун закончился - наносим урон!
        if (showDebug)
        {
            Debug.Log($"⚔️ {config.characterName} наносит урон {currentTarget.GetCharacterName()} на {config.damage}!");
        }
        
        currentTarget.TakeDamage(config.damage, this);
        
        // Устанавливаем кулдаун для следующего урона
        attackCooldownTimer = config.GetAttackCooldown();
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

    [Header("Настройки смерти")]
    [Tooltip("Удалять труп через N секунд после смерти (0 = не удалять)")]
    [SerializeField] private float removeCorpseDelay = 3f;
    
    [Tooltip("Плавно растворять труп перед удалением")]
    [SerializeField] private bool fadeOutCorpse = true;
    
    [Tooltip("Длительность растворения (секунды)")]
    [SerializeField] private float fadeOutDuration = 1f;

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

        // Удаляем труп через заданное время
        if (removeCorpseDelay > 0)
        {
            if (fadeOutCorpse)
            {
                StartCoroutine(FadeOutAndDestroy());
            }
            else
            {
                Destroy(gameObject, removeCorpseDelay);
                
                if (showDebug)
                {
                    Debug.Log($"🗑️ {config.characterName} будет удалён через {removeCorpseDelay}с");
                }
            }
        }
    }

    /// <summary>
    /// Плавно растворить труп и удалить
    /// </summary>
    private System.Collections.IEnumerator FadeOutAndDestroy()
    {
        // Ждём перед началом растворения
        float waitBeforeFade = removeCorpseDelay - fadeOutDuration;
        if (waitBeforeFade > 0)
        {
            yield return new WaitForSeconds(waitBeforeFade);
        }

        if (showDebug)
        {
            Debug.Log($"👻 {config.characterName} начинает растворяться...");
        }

        // Получаем все Image компоненты для растворения
        UnityEngine.UI.Image[] images = GetComponentsInChildren<UnityEngine.UI.Image>();
        TMPro.TextMeshProUGUI[] texts = GetComponentsInChildren<TMPro.TextMeshProUGUI>();
        
        // Сохраняем исходные цвета
        Color[] originalColors = new Color[images.Length];
        for (int i = 0; i < images.Length; i++)
        {
            originalColors[i] = images[i].color;
        }
        
        Color[] originalTextColors = new Color[texts.Length];
        for (int i = 0; i < texts.Length; i++)
        {
            originalTextColors[i] = texts[i].color;
        }

        // Плавное растворение
        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - (elapsed / fadeOutDuration);

            // Уменьшаем прозрачность
            for (int i = 0; i < images.Length; i++)
            {
                if (images[i] != null)
                {
                    Color c = originalColors[i];
                    c.a = alpha;
                    images[i].color = c;
                }
            }
            
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i] != null)
                {
                    Color c = originalTextColors[i];
                    c.a = alpha;
                    texts[i].color = c;
                }
            }

            yield return null;
        }

        // Удаляем объект
        if (showDebug)
        {
            Debug.Log($"🗑️ {config.characterName} удалён!");
        }
        
        Destroy(gameObject);
    }

    // ═══════════════════════════════════════════════════════════
    // СОСТОЯНИЯ
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Установить состояние
    /// </summary>
    private void SetState(CharacterState newState)
    {
        // Для Attacking разрешаем повторный вызов (чтобы анимация обновлялась)
        bool shouldUpdate = (currentState != newState) || (newState == CharacterState.Attacking);
        
        if (!shouldUpdate)
            return;

        CharacterState oldState = currentState;
        currentState = newState;
        
        // Обновляем анимацию
        if (characterAnimator != null)
        {
            switch (currentState)
            {
                case CharacterState.Idle:
                    if (oldState != CharacterState.Idle)
                        characterAnimator.PlayIdle();
                    break;
                case CharacterState.Moving:
                    if (oldState != CharacterState.Moving)
                        characterAnimator.PlayMove();
                    break;
                case CharacterState.Attacking:
                    // Всегда обновляем анимацию атаки (даже если уже Attacking)
                    characterAnimator.PlayAttack();
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

    // ═══════════════════════════════════════════════════════════
    // ТЕСТОВЫЕ МЕТОДЫ
    // ═══════════════════════════════════════════════════════════

    [ContextMenu("🎬 ТЕСТ: Проиграть анимацию атаки")]
    private void TestAttackAnimation()
    {
        if (characterAnimator != null)
        {
            Debug.Log($"🧪 ТЕСТ: Запускаю анимацию атаки для {gameObject.name}");
            characterAnimator.PlayAttack();
        }
        else
        {
            Debug.LogError($"❌ Character Animator НЕ НАЗНАЧЕН на {gameObject.name}!");
        }
    }

    [ContextMenu("📊 ТЕСТ: Проверить настройки")]
    private void TestSettings()
    {
        Debug.Log("═══════════════════════════════════════════════════════");
        Debug.Log($"🧪 ДИАГНОСТИКА: {gameObject.name}");
        Debug.Log("═══════════════════════════════════════════════════════");
        Debug.Log($"Config: {(config != null ? config.name : "НЕ НАЗНАЧЕН ❌")}");
        Debug.Log($"Character Animator: {(characterAnimator != null ? "Назначен ✅" : "НЕ НАЗНАЧЕН ❌")}");
        
        if (characterAnimator != null)
        {
            var animator = characterAnimator.GetAnimator();
            Debug.Log($"Animator: {(animator != null ? "Назначен ✅" : "НЕ НАЗНАЧЕН ❌")}");
            
            if (animator != null)
            {
                Debug.Log($"Animator Controller: {(animator.runtimeAnimatorController != null ? animator.runtimeAnimatorController.name + " ✅" : "НЕ НАЗНАЧЕН ❌")}");
            }
        }
        
        Debug.Log($"Health Bar: {(healthBar != null ? "Назначен ✅" : "Нет (опционально)")}");
        Debug.Log($"Current State: {currentState}");
        Debug.Log($"Is Dead: {isDead}");
        Debug.Log($"In Battle: {isInBattle}");
        Debug.Log("═══════════════════════════════════════════════════════");
    }
}

