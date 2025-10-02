using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Сервис автоматической расстановки врагов из конфига
/// </summary>
public class WaveSpawner : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private GridManager gridManager;
    
    [Header("Конфиги волн")]
    [Tooltip("Список волн врагов")]
    [SerializeField] private List<WaveConfig> waves = new List<WaveConfig>();
    
    [Header("Настройки спавна")]
    [Tooltip("Задержка между спавном врагов (секунды)")]
    [SerializeField] private float spawnDelay = 0.2f;
    
    [Tooltip("Автоматически спавнить первую волну при старте")]
    [SerializeField] private bool spawnOnStart = false;
    
    [Tooltip("Зона для размещения врагов (от какого X до какого)")]
    [SerializeField] private Vector2Int spawnZoneX = new Vector2Int(5, 9);
    
    [Tooltip("Зона для размещения врагов (от какого Y до какого)")]
    [SerializeField] private Vector2Int spawnZoneY = new Vector2Int(0, 9);
    
    [Header("Настройки")]
    [Tooltip("Показывать отладочную информацию")]
    [SerializeField] private bool showDebug = false;

    private int currentWaveIndex = 0;
    private bool isSpawning = false;

    private void Start()
    {
        if (gridManager == null)
        {
            gridManager = FindObjectOfType<GridManager>();
        }

        if (spawnOnStart && waves.Count > 0)
        {
            SpawnWave(0);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // ПУБЛИЧНЫЕ МЕТОДЫ
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Заспавнить волну по индексу
    /// </summary>
    public void SpawnWave(int waveIndex)
    {
        if (waveIndex < 0 || waveIndex >= waves.Count)
        {
            Debug.LogWarning($"Волна {waveIndex} не существует!");
            return;
        }

        if (isSpawning)
        {
            Debug.LogWarning("Уже идёт спавн волны!");
            return;
        }

        currentWaveIndex = waveIndex;
        StartCoroutine(SpawnWaveCoroutine(waves[waveIndex]));
    }

    /// <summary>
    /// Заспавнить текущую волну
    /// </summary>
    public void SpawnCurrentWave()
    {
        SpawnWave(currentWaveIndex);
    }

    /// <summary>
    /// Заспавнить следующую волну
    /// </summary>
    public void SpawnNextWave()
    {
        int nextIndex = currentWaveIndex + 1;
        if (nextIndex >= waves.Count)
        {
            if (showDebug)
            {
                Debug.Log("Все волны закончились!");
            }
            return;
        }

        SpawnWave(nextIndex);
    }

    /// <summary>
    /// Заспавнить врагов из конфига
    /// </summary>
    public void SpawnWaveFromConfig(WaveConfig config)
    {
        if (config == null)
        {
            Debug.LogError("WaveConfig не назначен!");
            return;
        }

        StartCoroutine(SpawnWaveCoroutine(config));
    }

    // ═══════════════════════════════════════════════════════════
    // ЛОГИКА СПАВНА
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Корутина спавна волны
    /// </summary>
    private IEnumerator SpawnWaveCoroutine(WaveConfig wave)
    {
        isSpawning = true;

        if (showDebug)
        {
            Debug.Log($"╔═══════════════════════════════════════╗");
            Debug.Log($"║ СПАВН ВОЛНЫ: {wave.waveName}");
            Debug.Log($"║ Врагов: {wave.GetEnemyCount()}");
            Debug.Log($"╚═══════════════════════════════════════╝");
        }

        int spawnedCount = 0;
        int failedCount = 0;

        foreach (var enemyData in wave.enemies)
        {
            if (enemyData.enemyPrefab == null)
            {
                Debug.LogWarning("⚠️ Префаб врага не назначен! Пропускаем...");
                failedCount++;
                continue;
            }

            Vector2Int spawnPosition;

            // Определяем позицию спавна
            if (enemyData.randomPosition)
            {
                spawnPosition = GetRandomFreePosition();
                if (showDebug)
                {
                    Debug.Log($"🎲 Случайная позиция выбрана: ({spawnPosition.x}, {spawnPosition.y})");
                }
            }
            else
            {
                spawnPosition = enemyData.gridPosition;
                if (showDebug)
                {
                    Debug.Log($"📍 Фиксированная позиция: ({spawnPosition.x}, {spawnPosition.y})");
                }
            }

            // Пытаемся заспавнить
            if (showDebug)
            {
                Debug.Log($"🔄 Пытаюсь заспавнить {enemyData.enemyPrefab.name} в ({spawnPosition.x}, {spawnPosition.y})...");
            }
            
            bool success = SpawnEnemyAtPosition(enemyData.enemyPrefab, spawnPosition);

            if (success)
            {
                spawnedCount++;
                
                if (showDebug)
                {
                    Debug.Log($"✅ Враг заспавнен в ({spawnPosition.x}, {spawnPosition.y})");
                }
            }
            else
            {
                failedCount++;
                
                Debug.LogWarning($"❌ НЕ УДАЛОСЬ заспавнить в ({spawnPosition.x}, {spawnPosition.y}) - причина выше!");
            }

            // Задержка между спавном
            yield return new WaitForSeconds(spawnDelay);
        }

        isSpawning = false;

        if (showDebug)
        {
            Debug.Log($"📊 Волна завершена! Заспавнено: {spawnedCount}, Ошибок: {failedCount}");
        }
    }

    /// <summary>
    /// Заспавнить врага в конкретной позиции
    /// </summary>
    private bool SpawnEnemyAtPosition(GameObject enemyPrefab, Vector2Int gridPosition)
    {
        if (gridManager == null)
        {
            Debug.LogError("❌ GridManager не найден!");
            return false;
        }

        // Получаем ячейку
        GridCell cell = gridManager.GetCell(gridPosition);

        if (cell == null)
        {
            Debug.LogWarning($"❌ Ячейка ({gridPosition.x}, {gridPosition.y}) НЕ НАЙДЕНА! Проверьте координаты!");
            return false;
        }

        // Проверяем занята ли
        if (cell.IsOccupied())
        {
            Debug.LogWarning($"❌ Ячейка ({gridPosition.x}, {gridPosition.y}) УЖЕ ЗАНЯТА!");
            return false;
        }

        // Размещаем врага
        if (showDebug)
        {
            Debug.Log($"🎯 Размещаю {enemyPrefab.name} в ячейке ({gridPosition.x}, {gridPosition.y})...");
        }
        
        bool result = cell.PlaceObject(enemyPrefab);
        
        if (!result && showDebug)
        {
            Debug.LogWarning($"❌ PlaceObject вернул false для ({gridPosition.x}, {gridPosition.y})!");
        }
        
        return result;
    }

    /// <summary>
    /// Получить случайную свободную позицию в зоне спавна
    /// </summary>
    private Vector2Int GetRandomFreePosition()
    {
        List<GridCell> freeCells = gridManager.GetFreeCells();
        
        // Фильтруем только ячейки в зоне спавна
        List<GridCell> validCells = new List<GridCell>();
        
        foreach (var cell in freeCells)
        {
            Vector2Int pos = cell.gridPosition;
            
            if (pos.x >= spawnZoneX.x && pos.x <= spawnZoneX.y &&
                pos.y >= spawnZoneY.x && pos.y <= spawnZoneY.y)
            {
                validCells.Add(cell);
            }
        }

        if (validCells.Count == 0)
        {
            // Если нет свободных в зоне, берём любую
            if (freeCells.Count > 0)
            {
                return freeCells[Random.Range(0, freeCells.Count)].gridPosition;
            }
            
            // Если вообще нет свободных, возвращаем центр зоны
            return new Vector2Int(
                (spawnZoneX.x + spawnZoneX.y) / 2,
                (spawnZoneY.x + spawnZoneY.y) / 2
            );
        }

        return validCells[Random.Range(0, validCells.Count)].gridPosition;
    }

    // ═══════════════════════════════════════════════════════════
    // УТИЛИТЫ
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Очистить всех врагов
    /// </summary>
    public void ClearAllEnemies()
    {
        if (gridManager != null)
        {
            gridManager.ClearAll();
            
            if (showDebug)
            {
                Debug.Log("Все враги очищены!");
            }
        }
    }

    /// <summary>
    /// Получить количество волн
    /// </summary>
    public int GetWaveCount()
    {
        return waves.Count;
    }

    /// <summary>
    /// Получить текущий индекс волны
    /// </summary>
    public int GetCurrentWaveIndex()
    {
        return currentWaveIndex;
    }

    /// <summary>
    /// Проверить идёт ли спавн
    /// </summary>
    public bool IsSpawning()
    {
        return isSpawning;
    }

    // ═══════════════════════════════════════════════════════════
    // ТЕСТОВЫЕ МЕТОДЫ
    // ═══════════════════════════════════════════════════════════

    [ContextMenu("Заспавнить текущую волну")]
    private void TestSpawnCurrent()
    {
        SpawnCurrentWave();
    }

    [ContextMenu("Заспавнить следующую волну")]
    private void TestSpawnNext()
    {
        SpawnNextWave();
    }

    [ContextMenu("Очистить всех")]
    private void TestClear()
    {
        ClearAllEnemies();
    }
}

