using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Менеджер игры - управляет UI и логикой игры
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private WaveSpawner waveSpawner;
    [SerializeField] private BattleManager battleManager;
    
    [Header("UI")]
    [Tooltip("Кнопка которая показывается когда есть герои")]
    [SerializeField] private GameObject startButton;
    
    [Tooltip("Объект который будет двигаться при нажатии кнопки")]
    [SerializeField] private RectTransform objectToMove;
    
    private Vector2 objectToMoveStartPosition; // Начальная позиция для возврата
    
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

        // Сохраняем начальную позицию objectToMove
        if (objectToMove != null)
        {
            objectToMoveStartPosition = objectToMove.anchoredPosition;
            
            if (showDebug)
            {
                Debug.Log($"📍 Начальная позиция objectToMove сохранена: {objectToMoveStartPosition}");
            }
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

        // Считаем только игроков (Team 0), не врагов!
        int currentCount = 0;
        var occupiedCells = gridManager.GetOccupiedCells();
        
        foreach (var cell in occupiedCells)
        {
            GameObject obj = cell.GetPlacedObject();
            if (obj != null)
            {
                CharacterController controller = obj.GetComponent<CharacterController>();
                if (controller != null && controller.GetTeam() == 0)
                {
                    currentCount++; // Только игроки (Team 0)
                }
            }
        }

        // Если количество изменилось - обновляем UI
        if (currentCount != heroCount)
        {
            heroCount = currentCount;
            UpdateButtonVisibility();

            if (showDebug)
            {
                Debug.Log($"Героев игрока на поле: {heroCount}");
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

        // Проверяем что размещение НЕ заблокировано (не идёт бой)
        bool isPlacementMode = gridManager != null && !gridManager.IsPlacementLocked();
        
        // Проверяем что objectToMove в исходном положении (X близко к 959)
        bool isInStartPosition = true;
        if (objectToMove != null)
        {
            float currentX = objectToMove.anchoredPosition.x;
            float startX = objectToMoveStartPosition.x;
            isInStartPosition = Mathf.Abs(currentX - startX) < 50f; // Допуск 50 пикселей
        }

        // Кнопка показывается ТОЛЬКО если:
        // 1. Есть герои на поле
        // 2. НЕ идёт бой (размещение не заблокировано)
        // 3. UI в исходном положении
        bool shouldShow = heroCount > 0 && isPlacementMode && isInStartPosition;
        
        startButton.SetActive(shouldShow);

        if (showDebug && shouldShow != startButton.activeSelf)
        {
            Debug.Log($"Кнопка: {(shouldShow ? "ПОКАЗАНА" : "СКРЫТА")} | Героев: {heroCount} | Режим расстановки: {isPlacementMode} | UI на месте: {isInStartPosition}");
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

        // СОХРАНЯЕМ РАССТАНОВКУ ГЕРОЕВ (для респавна после победы)
        if (battleManager != null)
        {
            battleManager.SaveHeroSetup();
        }

        // БЛОКИРУЕМ РАЗМЕЩЕНИЕ ПЕРСОНАЖЕЙ
        if (gridManager != null)
        {
            gridManager.LockPlacement();
        }

        // Скрываем кнопку
        HideButton();

        // Запускаем плавное перемещение объекта
        if (objectToMove != null && !isMoving)
        {
            StartCoroutine(MoveObjectToZero());
        }
        
        // Запускаем бой (с врагами из WaveSpawner если есть)
        if (waveSpawner != null)
        {
            StartCoroutine(SpawnWaveAndStartBattle());
        }
        else
        {
            StartBattle();
        }
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
    /// Начать бой
    /// </summary>
    private void StartBattle()
    {
        Debug.Log("═══════════════════════════════════════");
        Debug.Log("⚔️ БОЙ НАЧАЛСЯ!");
        
        // Получаем всех размещенных героев
        var occupiedCells = gridManager.GetOccupiedCells();
        
        if (occupiedCells.Count == 0)
        {
            Debug.LogWarning("❌ Нет героев для боя!");
            return;
        }
        
        Debug.Log($"📊 Всего героев на поле: {occupiedCells.Count}");
        
        int team0Count = 0;
        int team1Count = 0;
        
        // Активируем AI всех персонажей
        foreach (var cell in occupiedCells)
        {
            GameObject heroObj = cell.GetPlacedObject();
            
            if (heroObj == null)
            {
                Debug.LogWarning("⚠️ Пустой объект в ячейке!");
                continue;
            }
            
            CharacterController controller = heroObj.GetComponent<CharacterController>();
            
            if (controller != null)
            {
                // Проверяем конфиг
                var config = controller.GetConfig();
                if (config == null)
                {
                    Debug.LogError($"❌ У {heroObj.name} НЕТ CharacterConfig! Назначьте в CharacterController!");
                    continue;
                }
                
                controller.StartBattle(); // Запускаем AI
                
                int team = controller.GetTeam();
                if (team == 0) team0Count++;
                else team1Count++;
                
                Debug.Log($"✅ {controller.GetCharacterName()} готов к бою! " +
                         $"[Team: {team}] [HP: {controller.GetMaxHealth()}] " +
                         $"[Damage: {config.damage}] " +
                         $"[Config: {config.name}]");
            }
            else
            {
                Debug.LogError($"❌ У {heroObj.name} НЕТ CharacterController! Добавьте компонент!");
                
                // Покажем какие компоненты есть
                var components = heroObj.GetComponents<Component>();
                string componentsList = "";
                foreach (var comp in components)
                {
                    componentsList += comp.GetType().Name + ", ";
                }
                Debug.Log($"   Компоненты на объекте: {componentsList}");
            }
        }
        
        Debug.Log($"📊 Команда 0 (игрок): {team0Count} героев");
        Debug.Log($"📊 Команда 1 (враги): {team1Count} героев");
        
        if (team0Count > 0 && team1Count > 0)
        {
            Debug.Log("✅ Обе команды готовы! Бой начинается!");
        }
        else if (team0Count == 0)
        {
            Debug.LogWarning("⚠️ Нет героев в команде 0 (игрок)!");
        }
        else if (team1Count == 0)
        {
            Debug.LogWarning("⚠️ Нет героев в команде 1 (враги)! Некого атаковать!");
        }
        
        Debug.Log("═══════════════════════════════════════");
    }
    
    /// <summary>
    /// Заспавнить волну врагов и начать бой
    /// </summary>
    private System.Collections.IEnumerator SpawnWaveAndStartBattle()
    {
        Debug.Log("🌊 Спавним волну врагов...");
        
        // Спавним первую волну
        waveSpawner.SpawnWave(0);
        
        // Ждём пока закончится спавн
        while (waveSpawner.IsSpawning())
        {
            yield return null;
        }
        
        yield return new WaitForSeconds(0.5f); // Небольшая пауза
        
        // Теперь запускаем бой
        StartBattle();
        
        // Уведомляем Battle Manager что бой начался
        if (battleManager != null)
        {
            battleManager.StartBattle();
        }
    }

    /// <summary>
    /// Сбросить состояние игры (для следующего раунда)
    /// </summary>
    public void ResetGame()
    {
        // Разблокируем размещение
        if (gridManager != null)
        {
            gridManager.UnlockPlacement();
            gridManager.ClearAll();
        }
        
        // Показываем кнопку снова
        if (startButton != null)
        {
            startButton.SetActive(false); // Сначала скрыта пока не разместят героев
        }
        
        heroCount = 0;
        
        if (showDebug)
        {
            Debug.Log("🔄 Игра сброшена! Можно расставлять героев снова.");
        }
    }

    /// <summary>
    /// Вернуть objectToMove в начальную позицию (X=959) МГНОВЕННО
    /// </summary>
    public void ResetObjectToMovePosition()
    {
        if (objectToMove != null)
        {
            objectToMove.anchoredPosition = objectToMoveStartPosition;
            
            if (showDebug)
            {
                Debug.Log($"⚡ objectToMove вернулся МГНОВЕННО в: {objectToMoveStartPosition}");
            }
        }
    }

    /// <summary>
    /// Получить начальную позицию objectToMove
    /// </summary>
    public Vector2 GetObjectToMoveStartPosition()
    {
        return objectToMoveStartPosition;
    }
}


