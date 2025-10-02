using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Менеджер боя - отслеживает победу/поражение и управляет циклом раундов
/// </summary>
public class BattleManager : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private RectTransform gridTransform;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private ScreenFade screenFade;
    
    [Header("Настройки победы")]
    [Tooltip("Задержка после победы перед возвратом Grid")]
    [SerializeField] private float victoryDelay = 2f;
    
    [Tooltip("Скорость возврата Grid в начальную позицию")]
    [SerializeField] private float returnSpeed = 500f;
    
    [Tooltip("Задержка перед респавном героев")]
    [SerializeField] private float respawnDelay = 0.5f;
    
    [Header("Настройки")]
    [Tooltip("Показывать отладочную информацию")]
    [SerializeField] private bool showDebug = false;

    [Header("Префабы для респавна")]
    [Tooltip("Префаб героя (Character)")]
    [SerializeField] private GameObject heroPrefab;

    // Сохранённая расстановка героев
    private class HeroSetup
    {
        public Vector2Int gridPosition;
        public CharacterConfig config;
    }

    private List<HeroSetup> savedHeroSetup = new List<HeroSetup>();
    private Vector2 gridStartPosition;
    private bool isBattleActive = false;
    private bool isReturning = false;

    private void Start()
    {
        if (gridTransform == null && gridManager != null)
        {
            gridTransform = gridManager.GetComponent<RectTransform>();
        }

        if (gridTransform != null)
        {
            // Сохраняем начальную позицию Grid
            gridStartPosition = gridTransform.anchoredPosition;
            
            if (showDebug)
            {
                Debug.Log($"📍 Начальная позиция Grid сохранена: {gridStartPosition}");
            }
        }
        
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }
    }

    private void Update()
    {
        if (isBattleActive && !isReturning)
        {
            CheckBattleStatus();
        }
    }

    /// <summary>
    /// Сохранить текущую расстановку героев (вызывать перед началом боя)
    /// </summary>
    public void SaveHeroSetup()
    {
        savedHeroSetup.Clear();

        if (gridManager == null)
        {
            Debug.LogError("GridManager не назначен!");
            return;
        }

        var occupiedCells = gridManager.GetOccupiedCells();

        foreach (var cell in occupiedCells)
        {
            GameObject hero = cell.GetPlacedObject();
            if (hero == null)
                continue;

            CharacterController controller = hero.GetComponent<CharacterController>();
            if (controller == null)
                continue;

            // Сохраняем только игроков (Team 0)
            if (controller.GetTeam() == 0)
            {
                HeroSetup setup = new HeroSetup
                {
                    gridPosition = cell.gridPosition,
                    config = controller.GetConfig()
                };

                savedHeroSetup.Add(setup);
            }
        }

        if (showDebug)
        {
            Debug.Log($"💾 Сохранена расстановка: {savedHeroSetup.Count} героев");
            foreach (var setup in savedHeroSetup)
            {
                Debug.Log($"  → {setup.config.characterName} на ({setup.gridPosition.x}, {setup.gridPosition.y})");
            }
        }
    }

    /// <summary>
    /// Начать бой (вызывать из GameManager.OnStartButtonClicked)
    /// </summary>
    public void StartBattle()
    {
        isBattleActive = true;
        
        if (showDebug)
        {
            Debug.Log("⚔️ Battle Manager: Бой начался!");
        }
    }

    /// <summary>
    /// Проверить статус боя (все враги мёртвы?)
    /// </summary>
    private void CheckBattleStatus()
    {
        CharacterController[] allCharacters = FindObjectsOfType<CharacterController>();
        
        int aliveEnemies = 0;
        int alivePlayers = 0;

        foreach (var character in allCharacters)
        {
            if (character.IsDead())
                continue;

            if (character.GetTeam() == 1)
                aliveEnemies++;
            else if (character.GetTeam() == 0)
                alivePlayers++;
        }

        // Победа - все враги мёртвы
        if (aliveEnemies == 0 && alivePlayers > 0)
        {
            OnVictory();
        }
        // Поражение - все игроки мёртвы
        else if (alivePlayers == 0 && aliveEnemies > 0)
        {
            OnDefeat();
        }
    }

    /// <summary>
    /// Победа - все враги убиты
    /// </summary>
    private void OnVictory()
    {
        isBattleActive = false;
        isReturning = true;

        if (showDebug)
        {
            Debug.Log("═══════════════════════════════════════════════════════");
            Debug.Log("🎉 ПОБЕДА! Все враги повержены!");
            Debug.Log("═══════════════════════════════════════════════════════");
        }

        StartCoroutine(ReturnToStartAndRespawn());
    }

    /// <summary>
    /// Поражение - все игроки мёртвы
    /// </summary>
    private void OnDefeat()
    {
        isBattleActive = false;

        if (showDebug)
        {
            Debug.Log("═══════════════════════════════════════════════════════");
            Debug.Log("💀 ПОРАЖЕНИЕ! Все герои погибли!");
            Debug.Log("═══════════════════════════════════════════════════════");
        }

        // Можно добавить экран поражения или перезапуск
    }

    /// <summary>
    /// Вернуть Grid в начало и респавнить героев
    /// </summary>
    private IEnumerator ReturnToStartAndRespawn()
    {
        // Ждём после победы
        yield return new WaitForSeconds(victoryDelay);

        // ЗАТЕМНЯЕМ ЭКРАН (Fade to Black)
        if (screenFade != null)
        {
            bool fadeComplete = false;
            screenFade.FadeIn(() => fadeComplete = true);
            
            while (!fadeComplete)
                yield return null;
                
            if (showDebug)
            {
                Debug.Log("🌑 Экран затемнён - выполняем рестарт...");
            }
        }

        // ВО ВРЕМЯ ЗАТЕМНЕНИЯ выполняем все действия
        
        if (showDebug)
        {
            Debug.Log("🔄 Возвращаем Grid и UI в начальную позицию МГНОВЕННО...");
        }

        // МГНОВЕННО возвращаем Grid на место (X = 959)
        if (gridTransform != null)
        {
            gridTransform.anchoredPosition = gridStartPosition;
            
            if (showDebug)
            {
                Debug.Log($"📍 Grid МГНОВЕННО вернулся: {gridStartPosition}");
            }
        }
        
        // МГНОВЕННО возвращаем objectToMove
        if (gameManager != null)
        {
            gameManager.ResetObjectToMovePosition();
        }

        // Очищаем поле от всех персонажей
        if (gridManager != null)
        {
            gridManager.ClearAll();
            gridManager.UnlockPlacement();
        }

        yield return new WaitForSeconds(0.3f);

        // Респавним героев
        yield return StartCoroutine(RespawnHeroes());

        yield return new WaitForSeconds(0.2f);

        // РАЗВЕИВАЕМ ЗАТЕМНЕНИЕ (Fade from Black)
        if (screenFade != null)
        {
            bool fadeComplete = false;
            screenFade.FadeOut(() => fadeComplete = true);
            
            while (!fadeComplete)
                yield return null;
                
            if (showDebug)
            {
                Debug.Log("☀️ Затемнение развеяно - готово!");
            }
        }

        isReturning = false;

        if (showDebug)
        {
            Debug.Log("✅ Герои восстановлены! Готовы к следующей волне!");
        }
    }


    /// <summary>
    /// Респавнить сохранённых героев
    /// </summary>
    private IEnumerator RespawnHeroes()
    {
        if (savedHeroSetup.Count == 0)
        {
            if (showDebug)
            {
                Debug.LogWarning("⚠️ Нет сохранённых героев для респавна!");
            }
            yield break;
        }

        if (showDebug)
        {
            Debug.Log($"♻️ Респавним {savedHeroSetup.Count} героев...");
        }

        if (heroPrefab == null)
        {
            Debug.LogError("❌ Hero Prefab не назначен в BattleManager!");
            yield break;
        }

        foreach (var setup in savedHeroSetup)
        {
            if (setup.config == null)
                continue;

            GridCell cell = gridManager.GetCell(setup.gridPosition);
            if (cell == null)
            {
                Debug.LogWarning($"Ячейка ({setup.gridPosition.x}, {setup.gridPosition.y}) не найдена!");
                continue;
            }

            // Создаём героя заново из префаба
            bool success = cell.PlaceObject(heroPrefab);

            if (success)
            {
                // Получаем созданного героя и настраиваем
                GameObject newHero = cell.GetPlacedObject();
                CharacterController controller = newHero.GetComponent<CharacterController>();
                
                if (controller != null)
                {
                    // Восстанавливаем конфиг
                    controller.SetConfig(setup.config);
                }

                if (showDebug)
                {
                    Debug.Log($"✅ {setup.config.characterName} респавнен на ({setup.gridPosition.x}, {setup.gridPosition.y})");
                }
            }

            yield return new WaitForSeconds(respawnDelay);
        }

        if (showDebug)
        {
            Debug.Log("✅ Все герои восстановлены!");
        }
    }

    /// <summary>
    /// Получить количество живых врагов
    /// </summary>
    public int GetAliveEnemiesCount()
    {
        CharacterController[] allCharacters = FindObjectsOfType<CharacterController>();
        int count = 0;

        foreach (var character in allCharacters)
        {
            if (!character.IsDead() && character.GetTeam() == 1)
                count++;
        }

        return count;
    }

    /// <summary>
    /// Получить количество живых игроков
    /// </summary>
    public int GetAlivePlayersCount()
    {
        CharacterController[] allCharacters = FindObjectsOfType<CharacterController>();
        int count = 0;

        foreach (var character in allCharacters)
        {
            if (!character.IsDead() && character.GetTeam() == 0)
                count++;
        }

        return count;
    }
}

