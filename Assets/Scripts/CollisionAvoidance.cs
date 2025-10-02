using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Простая система избегания столкновений между персонажами
/// Работает без NavMesh и A* - просто отталкивает персонажей друг от друга
/// </summary>
public class CollisionAvoidance : MonoBehaviour
{
    [Header("Настройки избегания")]
    [Tooltip("Радиус проверки других персонажей")]
    [SerializeField] private float avoidanceRadius = 50f;
    
    [Tooltip("Сила отталкивания от других персонажей")]
    [SerializeField] private float avoidanceForce = 100f;
    
    [Tooltip("Минимальная дистанция до других (персональное пространство)")]
    [SerializeField] private float personalSpace = 30f;
    
    [Tooltip("Проверять только союзников (той же команды)")]
    [SerializeField] private bool avoidOnlyAllies = true;
    
    [Header("Настройки")]
    [Tooltip("Показывать отладочную информацию")]
    [SerializeField] private bool showDebug = false;

    private CharacterController characterController;
    private RectTransform rectTransform;
    private Vector2 avoidanceVelocity = Vector2.zero;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        rectTransform = GetComponent<RectTransform>();
    }

    private void LateUpdate()
    {
        // Применяем избегание только если персонаж двигается
        if (characterController != null && 
            (characterController.GetCurrentState() == CharacterController.CharacterState.Moving ||
             characterController.GetCurrentState() == CharacterController.CharacterState.Attacking))
        {
            ApplyAvoidance();
        }
    }

    /// <summary>
    /// Применить избегание столкновений
    /// </summary>
    private void ApplyAvoidance()
    {
        Vector2 separationForce = CalculateSeparation();
        
        if (separationForce.magnitude > 0.01f)
        {
            // Применяем силу отталкивания
            rectTransform.position += (Vector3)separationForce * Time.deltaTime;
            
            if (showDebug && Time.frameCount % 60 == 0)
            {
                Debug.Log($"🚫 {gameObject.name} отталкивается от союзников: {separationForce.magnitude:F1}");
            }
        }
    }

    /// <summary>
    /// Рассчитать силу отталкивания от ближайших персонажей
    /// </summary>
    private Vector2 CalculateSeparation()
    {
        Vector2 separationForce = Vector2.zero;
        int neighborCount = 0;

        // Находим всех персонажей поблизости
        CharacterController[] allCharacters = FindObjectsOfType<CharacterController>();

        foreach (var other in allCharacters)
        {
            if (other == characterController || other.IsDead())
                continue;

            // Если нужно избегать только союзников
            if (avoidOnlyAllies && other.GetTeam() != characterController.GetTeam())
                continue;

            float distance = Vector2.Distance(transform.position, other.transform.position);

            // Если слишком близко
            if (distance < avoidanceRadius && distance > 0.1f)
            {
                // Вектор от другого персонажа к нам
                Vector2 diff = (Vector2)(transform.position - other.transform.position);
                
                // Чем ближе, тем сильнее отталкивание
                float strength = 1f - (distance / avoidanceRadius);
                
                // Если ОЧЕНЬ близко - усиливаем отталкивание
                if (distance < personalSpace)
                {
                    strength *= 2f;
                }
                
                separationForce += diff.normalized * strength * avoidanceForce;
                neighborCount++;
            }
        }

        // Усредняем если много соседей
        if (neighborCount > 0)
        {
            separationForce /= neighborCount;
        }

        return separationForce;
    }

    /// <summary>
    /// Проверить свободен ли путь к цели
    /// </summary>
    public bool IsPathClear(Vector2 targetPosition)
    {
        CharacterController[] allCharacters = FindObjectsOfType<CharacterController>();
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        float distance = Vector2.Distance(transform.position, targetPosition);

        foreach (var other in allCharacters)
        {
            if (other == characterController || other.IsDead())
                continue;

            // Проверяем только союзников
            if (avoidOnlyAllies && other.GetTeam() != characterController.GetTeam())
                continue;

            Vector2 toOther = (Vector2)other.transform.position - (Vector2)transform.position;
            float distToOther = toOther.magnitude;

            // Если союзник на пути
            if (distToOther < distance)
            {
                float angle = Vector2.Angle(direction, toOther.normalized);
                
                // Если союзник прямо по курсу
                if (angle < 30f && distToOther < personalSpace * 2)
                {
                    return false; // Путь заблокирован
                }
            }
        }

        return true; // Путь свободен
    }

    private void OnDrawGizmosSelected()
    {
        if (!showDebug)
            return;

        // Рисуем радиус избегания
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, avoidanceRadius);

        // Рисуем персональное пространство
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, personalSpace);
    }
}

