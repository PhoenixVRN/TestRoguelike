using UnityEngine;

/// <summary>
/// Контроллер анимаций персонажа
/// Управляет состояниями: Idle, Move, Attack, Death
/// </summary>
[RequireComponent(typeof(Animator))]
public class CharacterAnimator : MonoBehaviour
{
    [Header("Ссылки")]
    private Animator animator;
    
    [Header("Настройки")]
    [Tooltip("Показывать отладочную информацию")]
    [SerializeField] private bool showDebug = false;
    
    // Названия параметров в Animator Controller
    private const string PARAM_IS_MOVING = "IsMoving";
    private const string PARAM_IS_ATTACKING = "IsAttacking";
    private const string PARAM_IS_DEAD = "IsDead";
    
    // Текущее состояние
    private AnimationState currentState = AnimationState.Idle;
    private bool isDead = false;

    public enum AnimationState
    {
        Idle,
        Move,
        Attack,
        Death
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        
        if (animator == null)
        {
            Debug.LogError($"Animator не найден на {gameObject.name}!");
        }
    }

    private void Start()
    {
        // По умолчанию Idle
        PlayIdle();
    }

    // ═══════════════════════════════════════════════════════════
    // ПУБЛИЧНЫЕ МЕТОДЫ ДЛЯ УПРАВЛЕНИЯ АНИМАЦИЯМИ
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Воспроизвести анимацию Idle (покой)
    /// </summary>
    public void PlayIdle()
    {
        if (isDead) return;
        
        SetAnimationState(AnimationState.Idle);
        
        // Сбрасываем все bool
        animator.SetBool(PARAM_IS_MOVING, false);
        animator.SetBool(PARAM_IS_ATTACKING, false);
        
        if (showDebug)
            Debug.Log($"{gameObject.name}: Idle анимация");
    }

    /// <summary>
    /// Воспроизвести анимацию Move (движение)
    /// </summary>
    public void PlayMove()
    {
        if (isDead) return;
        
        SetAnimationState(AnimationState.Move);
        
        animator.SetBool(PARAM_IS_MOVING, true);
        animator.SetBool(PARAM_IS_ATTACKING, false);
        
        if (showDebug)
            Debug.Log($"{gameObject.name}: Move анимация");
    }

    /// <summary>
    /// Воспроизвести анимацию Attack (атака)
    /// </summary>
    public void PlayAttack()
    {
        if (isDead) return;
        
        SetAnimationState(AnimationState.Attack);
        
        animator.SetBool(PARAM_IS_MOVING, false);
        animator.SetBool(PARAM_IS_ATTACKING, true);
        
        if (showDebug)
            Debug.Log($"{gameObject.name}: Attack анимация");
    }

    /// <summary>
    /// Воспроизвести анимацию Death (смерть)
    /// </summary>
    public void PlayDeath()
    {
        if (isDead) return;
        
        isDead = true;
        SetAnimationState(AnimationState.Death);
        
        // Сбрасываем всё и устанавливаем смерть
        animator.SetBool(PARAM_IS_MOVING, false);
        animator.SetBool(PARAM_IS_ATTACKING, false);
        animator.SetBool(PARAM_IS_DEAD, true);
        
        if (showDebug)
            Debug.Log($"{gameObject.name}: Death анимация");
    }

    /// <summary>
    /// Воскресить персонажа (сбросить смерть)
    /// </summary>
    public void Resurrect()
    {
        isDead = false;
        animator.SetBool(PARAM_IS_DEAD, false);
        PlayIdle();
        
        if (showDebug)
            Debug.Log($"{gameObject.name}: Воскрешён");
    }

    // ═══════════════════════════════════════════════════════════
    // ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Установить текущее состояние анимации
    /// </summary>
    private void SetAnimationState(AnimationState newState)
    {
        currentState = newState;
    }

    /// <summary>
    /// Получить текущее состояние анимации
    /// </summary>
    public AnimationState GetCurrentState()
    {
        return currentState;
    }

    /// <summary>
    /// Проверить мёртв ли персонаж
    /// </summary>
    public bool IsDead()
    {
        return isDead;
    }

    /// <summary>
    /// Получить компонент Animator
    /// </summary>
    public Animator GetAnimator()
    {
        return animator;
    }

    // ═══════════════════════════════════════════════════════════
    // СОБЫТИЯ АНИМАЦИИ (вызываются из Animation Events)
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Вызывается в конце анимации атаки
    /// </summary>
    public void OnAttackComplete()
    {
        if (showDebug)
            Debug.Log($"{gameObject.name}: Атака завершена");
        
        // Возвращаемся в Idle после атаки
        PlayIdle();
    }

    /// <summary>
    /// Вызывается в момент удара в анимации атаки
    /// </summary>
    public void OnAttackHit()
    {
        if (showDebug)
            Debug.Log($"{gameObject.name}: Момент удара!");
        
        // Здесь можно нанести урон врагу
    }

    /// <summary>
    /// Вызывается в конце анимации смерти
    /// </summary>
    public void OnDeathComplete()
    {
        if (showDebug)
            Debug.Log($"{gameObject.name}: Анимация смерти завершена");
        
        // Можно удалить объект или отключить
        // Destroy(gameObject);
    }

    // ═══════════════════════════════════════════════════════════
    // ДОПОЛНИТЕЛЬНЫЕ УТИЛИТЫ
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Установить скорость анимации
    /// </summary>
    public void SetAnimationSpeed(float speed)
    {
        animator.speed = speed;
    }

    /// <summary>
    /// Пауза/возобновление анимации
    /// </summary>
    public void SetAnimationPaused(bool paused)
    {
        animator.speed = paused ? 0f : 1f;
    }

    // ═══════════════════════════════════════════════════════════
    // ТЕСТОВЫЕ МЕТОДЫ (для проверки в редакторе)
    // ═══════════════════════════════════════════════════════════

    [ContextMenu("Тест: Idle")]
    private void TestIdle()
    {
        PlayIdle();
    }

    [ContextMenu("Тест: Move")]
    private void TestMove()
    {
        PlayMove();
    }

    [ContextMenu("Тест: Attack")]
    private void TestAttack()
    {
        PlayAttack();
    }

    [ContextMenu("Тест: Death")]
    private void TestDeath()
    {
        PlayDeath();
    }

    [ContextMenu("Тест: Resurrect")]
    private void TestResurrect()
    {
        Resurrect();
    }
}

