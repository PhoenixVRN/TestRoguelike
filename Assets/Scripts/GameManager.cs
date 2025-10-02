using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Менеджер игры - управляет UI и логикой игры
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private GridManager gridManager;
    
    [Header("UI")]
    [Tooltip("Кнопка которая показывается когда есть герои")]
    [SerializeField] private GameObject startButton;
    
    [Tooltip("Объект который будет двигаться при нажатии кнопки")]
    [SerializeField] private RectTransform objectToMove;
    
    [Header("Настройки анимации")]
    [Tooltip("Скорость перемещения (секунды)")]
    [SerializeField] private float moveDuration = 1f;
    
    [Tooltip("Тип плавности движения")]
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Настройки")]
    [Tooltip("Показывать отладочную информацию")]
    [SerializeField] private bool showDebug = false;

    private int heroCount = 0;
    private bool isMoving = false;

    private void Start()
    {
        if (gridManager == null)
        {
            gridManager = FindObjectOfType<GridManager>();
        }

        // Скрываем кнопку при старте
        UpdateButtonVisibility();
    }

    private void Update()
    {
        // Проверяем количество героев каждый кадр
        CheckHeroCount();
    }

    /// <summary>
    /// Проверить количество героев на поле
    /// </summary>
    private void CheckHeroCount()
    {
        if (gridManager == null)
            return;

        int currentCount = gridManager.GetOccupiedCells().Count;

        // Если количество изменилось - обновляем UI
        if (currentCount != heroCount)
        {
            heroCount = currentCount;
            UpdateButtonVisibility();

            if (showDebug)
            {
                Debug.Log($"Героев на поле: {heroCount}");
            }
        }
    }

    /// <summary>
    /// Обновить видимость кнопки
    /// </summary>
    private void UpdateButtonVisibility()
    {
        if (startButton == null)
        {
            if (showDebug)
                Debug.LogWarning("Start Button не назначена!");
            return;
        }

        bool shouldShow = heroCount > 0;
        startButton.SetActive(shouldShow);

        if (showDebug)
        {
            Debug.Log($"Кнопка: {(shouldShow ? "ПОКАЗАНА" : "СКРЫТА")} (героев: {heroCount})");
        }
    }

    /// <summary>
    /// Показать кнопку принудительно
    /// </summary>
    public void ShowButton()
    {
        if (startButton != null)
        {
            startButton.SetActive(true);
        }
    }

    /// <summary>
    /// Скрыть кнопку принудительно
    /// </summary>
    public void HideButton()
    {
        if (startButton != null)
        {
            startButton.SetActive(false);
        }
    }

    /// <summary>
    /// Получить количество героев на поле
    /// </summary>
    public int GetHeroCount()
    {
        return heroCount;
    }

    /// <summary>
    /// Проверить есть ли герои на поле
    /// </summary>
    public bool HasHeroes()
    {
        return heroCount > 0;
    }

    /// <summary>
    /// Обработчик нажатия кнопки (назначьте в Inspector)
    /// </summary>
    public void OnStartButtonClicked()
    {
        if (showDebug)
        {
            Debug.Log($"🎮 Кнопка нажата! Героев на поле: {heroCount}");
        }

        // Запускаем плавное перемещение объекта
        if (objectToMove != null && !isMoving)
        {
            StartCoroutine(MoveObjectToZero());
        }
        
        StartBattle();
    }
    
    /// <summary>
    /// Плавно переместить объект к X = 0
    /// </summary>
    private System.Collections.IEnumerator MoveObjectToZero()
    {
        if (objectToMove == null)
            yield break;
            
        isMoving = true;
        
        Vector2 startPos = objectToMove.anchoredPosition;
        Vector2 targetPos = new Vector2(0, startPos.y); // X = 0, Y остаётся
        
        float elapsed = 0f;
        
        if (showDebug)
        {
            Debug.Log($"📍 Начало движения: {startPos} → {targetPos}");
        }
        
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;
            
            // Применяем кривую для плавности
            float curveValue = moveCurve.Evaluate(t);
            
            // Интерполяция позиции
            objectToMove.anchoredPosition = Vector2.Lerp(startPos, targetPos, curveValue);
            
            yield return null;
        }
        
        // Устанавливаем точную конечную позицию
        objectToMove.anchoredPosition = targetPos;
        
        isMoving = false;
        
        if (showDebug)
        {
            Debug.Log($"✅ Движение завершено: {objectToMove.anchoredPosition}");
        }
    }
    
    /// <summary>
    /// Переместить объект к X = 0 мгновенно
    /// </summary>
    public void MoveObjectToZeroInstant()
    {
        if (objectToMove != null)
        {
            Vector2 pos = objectToMove.anchoredPosition;
            objectToMove.anchoredPosition = new Vector2(0, pos.y);
        }
    }
    
    /// <summary>
    /// Вернуть объект на исходную позицию
    /// </summary>
    public void ResetObjectPosition(Vector2 position)
    {
        if (objectToMove != null)
        {
            objectToMove.anchoredPosition = position;
        }
    }

    /// <summary>
    /// Начать бой (пример)
    /// </summary>
    private void StartBattle()
    {
        Debug.Log("⚔️ БОЙ НАЧАЛСЯ!");
        
        // Здесь ваша логика боя
        // Можно получить всех героев:
        var occupiedCells = gridManager.GetOccupiedCells();
        
        foreach (var cell in occupiedCells)
        {
            GameObject hero = cell.GetPlacedObject();
            Debug.Log($"Герой в бою: {hero.name} на позиции {cell.gridPosition}");
            
            // Можно запустить анимацию атаки
            CharacterAnimator anim = hero.GetComponent<CharacterAnimator>();
            if (anim != null)
            {
                anim.PlayAttack();
            }
        }
    }
}

