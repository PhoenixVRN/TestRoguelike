using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// Менеджер сетки - управляет всеми ячейками GridCell
/// Размещайте GridCell пустышки вручную в центрах ромбов
/// </summary>
public class GridManager : MonoBehaviour
{
    [Header("Префаб для размещения")]
    [Tooltip("UI префаб, который будет размещаться при клике")]
    [SerializeField] private GameObject prefabToPlace;
    
    [Header("Настройки")]
    [Tooltip("Разрешить размещение только в свободных ячейках")]
    [SerializeField] private bool checkOccupancy = true;
    
    [Tooltip("ПКМ удаляет объект из ячейки")]
    [SerializeField] private bool rightClickToRemove = true;
    
    [Tooltip("Показывать маркеры ячеек в игре")]
    [SerializeField] private bool showCellMarkers = false;
    
    [Tooltip("Показывать отладочную информацию")]
    [SerializeField] private bool showDebug = false;

    [Header("Множественные префабы (опционально)")]
    [Tooltip("Список префабов для переключения (1-9)")]
    [SerializeField] private GameObject[] prefabs;
    
    [Tooltip("Показывать подсказки управления")]
    [SerializeField] private bool showControlHints = true;

    private List<GridCell> cells = new List<GridCell>();
    private int currentPrefabIndex = 0;
    private GameObject currentPrefab;
    private bool isPlacementLocked = false; // Блокировка размещения

    private void Awake()
    {
        // Собираем все ячейки
        cells.AddRange(GetComponentsInChildren<GridCell>());
        
        currentPrefab = prefabToPlace;
        
        Debug.Log($"GridManager инициализирован. Найдено ячеек: {cells.Count}");
    }

    private void Start()
    {
        // Применяем настройку видимости маркеров
        SetAllMarkersVisible(showCellMarkers);
    }

    private void Update()
    {
        HandleInput();
    }

    /// <summary>
    /// Обработка ввода с клавиатуры
    /// </summary>
    private void HandleInput()
    {
        if (prefabs == null || prefabs.Length == 0)
            return;

        // Переключение префабов на цифры 1-9
        for (int i = 0; i < Mathf.Min(9, prefabs.Length); i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectPrefab(i);
            }
        }

        // Переключение стрелками
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            SelectPrefab((currentPrefabIndex + 1) % prefabs.Length);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            SelectPrefab((currentPrefabIndex - 1 + prefabs.Length) % prefabs.Length);
        }

        // Очистка всей сетки на Delete
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            ClearAll();
        }

        // Переключение видимости маркеров на M
        if (Input.GetKeyDown(KeyCode.M))
        {
            showCellMarkers = !showCellMarkers;
            SetAllMarkersVisible(showCellMarkers);
            Debug.Log($"Маркеры ячеек: {(showCellMarkers ? "Показаны" : "Скрыты")}");
        }
    }

    /// <summary>
    /// Обработка клика ЛКМ по ячейке
    /// </summary>
    public void OnCellClicked(GridCell cell, PointerEventData eventData)
    {
        // БЛОКИРОВКА: Если размещение заблокировано - игнорируем клик
        if (isPlacementLocked)
        {
            if (showDebug)
            {
                Debug.Log("🔒 Размещение заблокировано! Бой уже начался.");
            }
            return;
        }
        
        GameObject prefab = GetCurrentPrefab();
        
        if (prefab == null)
        {
            Debug.LogWarning("Префаб для размещения не назначен!");
            return;
        }

        // Проверяем занятость
        if (checkOccupancy && cell.IsOccupied())
        {
            if (showDebug)
            {
                Debug.Log($"Ячейка {cell.gridPosition} уже занята!");
            }
            return;
        }

        // Размещаем объект
        bool success = cell.PlaceObject(prefab);
        
        if (success && showDebug)
        {
            Debug.Log($"Размещен объект в ячейке {cell.gridPosition}");
        }
    }

    /// <summary>
    /// Обработка клика ПКМ по ячейке
    /// </summary>
    public void OnCellRightClicked(GridCell cell, PointerEventData eventData)
    {
        // БЛОКИРОВКА: Если размещение заблокировано - запрещаем удаление
        if (isPlacementLocked)
        {
            if (showDebug)
            {
                Debug.Log("🔒 Удаление заблокировано! Бой уже начался.");
            }
            return;
        }
        
        if (showDebug)
        {
            Debug.Log($"🖱️ ПКМ по ячейке {cell.gridPosition}");
        }

        if (!rightClickToRemove)
        {
            if (showDebug)
            {
                Debug.LogWarning("⚠️ Удаление по ПКМ ВЫКЛЮЧЕНО в настройках!");
            }
            return;
        }

        if (cell.IsOccupied())
        {
            cell.RemoveObject();
            
            if (showDebug)
            {
                Debug.Log($"✅ Удален объект из ячейки {cell.gridPosition}");
            }
        }
        else
        {
            if (showDebug)
            {
                Debug.Log($"ℹ️ Ячейка {cell.gridPosition} пустая, нечего удалять");
            }
        }
    }

    /// <summary>
    /// Выбрать префаб по индексу
    /// </summary>
    public void SelectPrefab(int index)
    {
        if (prefabs == null || prefabs.Length == 0)
            return;

        if (index < 0 || index >= prefabs.Length)
        {
            Debug.LogWarning($"Некорректный индекс префаба: {index}");
            return;
        }

        currentPrefabIndex = index;
        currentPrefab = prefabs[index];
        
        Debug.Log($"Выбран префаб [{index + 1}/{prefabs.Length}]: {currentPrefab.name}");
    }

    /// <summary>
    /// Установить текущий префаб
    /// </summary>
    public void SetCurrentPrefab(GameObject prefab)
    {
        currentPrefab = prefab;
        
        if (showDebug)
        {
            Debug.Log($"Установлен текущий префаб: {(prefab != null ? prefab.name : "null")}");
        }
    }

    /// <summary>
    /// Получить текущий префаб
    /// </summary>
    public GameObject GetCurrentPrefab()
    {
        return currentPrefab ?? prefabToPlace;
    }

    /// <summary>
    /// Очистить все ячейки
    /// </summary>
    public void ClearAll()
    {
        foreach (var cell in cells)
        {
            cell.RemoveObject();
        }
        
        Debug.Log("Все ячейки очищены!");
    }

    /// <summary>
    /// Показать/скрыть все маркеры
    /// </summary>
    public void SetAllMarkersVisible(bool visible)
    {
        showCellMarkers = visible;
        
        foreach (var cell in cells)
        {
            cell.SetMarkerVisible(visible);
        }
    }

    /// <summary>
    /// Получить ячейку по координатам
    /// </summary>
    public GridCell GetCell(Vector2Int gridPosition)
    {
        return cells.Find(c => c.gridPosition == gridPosition);
    }

    /// <summary>
    /// Получить все ячейки
    /// </summary>
    public List<GridCell> GetAllCells()
    {
        return new List<GridCell>(cells);
    }

    /// <summary>
    /// Получить все занятые ячейки
    /// </summary>
    public List<GridCell> GetOccupiedCells()
    {
        return cells.FindAll(c => c.IsOccupied());
    }

    /// <summary>
    /// Получить все свободные ячейки
    /// </summary>
    public List<GridCell> GetFreeCells()
    {
        return cells.FindAll(c => !c.IsOccupied());
    }

    /// <summary>
    /// Заблокировать размещение персонажей (бой начался)
    /// </summary>
    public void LockPlacement()
    {
        isPlacementLocked = true;
        
        if (showDebug)
        {
            Debug.Log("🔒 Размещение персонажей ЗАБЛОКИРОВАНО!");
        }
    }

    /// <summary>
    /// Разблокировать размещение персонажей (вернуться к расстановке)
    /// </summary>
    public void UnlockPlacement()
    {
        isPlacementLocked = false;
        
        if (showDebug)
        {
            Debug.Log("🔓 Размещение персонажей РАЗБЛОКИРОВАНО!");
        }
    }

    /// <summary>
    /// Проверить заблокировано ли размещение
    /// </summary>
    public bool IsPlacementLocked()
    {
        return isPlacementLocked;
    }

    private void OnGUI()
    {
        if (!showControlHints || (prefabs == null || prefabs.Length == 0))
            return;

        // Показываем подсказки на экране
        GUIStyle style = new GUIStyle();
        style.fontSize = 16;
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.UpperLeft;

        string help = "═══ УПРАВЛЕНИЕ ═══\n";
        help += $"1-{Mathf.Min(9, prefabs.Length)} - выбор префаба\n";
        help += "← → - переключение\n";
        help += "ЛКМ - разместить\n";
        help += "ПКМ - удалить\n";
        help += "M - вкл/выкл маркеры\n";
        help += "Delete - очистить всё\n\n";
        help += $"Префаб: [{currentPrefabIndex + 1}/{prefabs.Length}]\n";
        help += $"{GetCurrentPrefab().name}\n\n";
        help += $"Ячеек: {cells.Count}\n";
        help += $"Занято: {GetOccupiedCells().Count}\n";
        help += $"Свободно: {GetFreeCells().Count}";

        GUI.Label(new Rect(10, 10, 400, 300), help, style);
    }

    // ═══════════════════════════════════════════════════════════
    // ИНСТРУМЕНТЫ ДЛЯ РАЗРАБОТЧИКА
    // ═══════════════════════════════════════════════════════════

    [ContextMenu("📍 Показать все координаты")]
    private void ShowAllCoordinates()
    {
        // Получаем ячейки заново (работает и в Edit mode)
        GridCell[] allCells = GetComponentsInChildren<GridCell>();
        
        if (allCells.Length == 0)
        {
            Debug.LogWarning("❌ Ячейки не найдены! Убедитесь что:");
            Debug.LogWarning("   1. GridCell объекты созданы в Hierarchy");
            Debug.LogWarning("   2. Они являются дочерними объектами Grid");
            Debug.LogWarning("   3. У них есть компонент GridCell");
            return;
        }

        Debug.Log("═══════════════════════════════════════════════════════════");
        Debug.Log($"✅ ДОСТУПНЫЕ КООРДИНАТЫ ({allCells.Length} ячеек):");
        Debug.Log("═══════════════════════════════════════════════════════════");

        // Находим границы сетки
        int minX = int.MaxValue;
        int maxX = int.MinValue;
        int minY = int.MaxValue;
        int maxY = int.MinValue;

        foreach (var cell in allCells)
        {
            if (cell.gridPosition.x < minX) minX = cell.gridPosition.x;
            if (cell.gridPosition.x > maxX) maxX = cell.gridPosition.x;
            if (cell.gridPosition.y < minY) minY = cell.gridPosition.y;
            if (cell.gridPosition.y > maxY) maxY = cell.gridPosition.y;
        }

        Debug.Log($"📐 Размер сетки: X=[{minX}..{maxX}], Y=[{minY}..{maxY}]\n");

        // Разделяем на левую и правую стороны
        int middleX = (minX + maxX) / 2;

        // Левая сторона (игрок)
        Debug.Log($"<color=cyan>🛡️ Левая сторона (Team 0 - игрок, X ≤ {middleX}):</color>");
        string leftCoords = "";
        int leftCount = 0;
        
        // Сортируем для красоты
        System.Array.Sort(allCells, (a, b) => {
            if (a.gridPosition.x != b.gridPosition.x)
                return a.gridPosition.x.CompareTo(b.gridPosition.x);
            return a.gridPosition.y.CompareTo(b.gridPosition.y);
        });
        
        foreach (var cell in allCells)
        {
            if (cell.gridPosition.x <= middleX)
            {
                leftCoords += $"({cell.gridPosition.x},{cell.gridPosition.y}), ";
                leftCount++;
                if (leftCount % 6 == 0) leftCoords += "\n";
            }
        }
        if (leftCount > 0)
            Debug.Log(leftCoords.TrimEnd(',', ' '));
        else
            Debug.Log("(нет ячеек)");

        Debug.Log("");

        // Правая сторона (враги)
        Debug.Log($"<color=red>⚔️ Правая сторона (Team 1 - враги, X > {middleX}):</color>");
        string rightCoords = "";
        int rightCount = 0;
        foreach (var cell in allCells)
        {
            if (cell.gridPosition.x > middleX)
            {
                rightCoords += $"({cell.gridPosition.x},{cell.gridPosition.y}), ";
                rightCount++;
                if (rightCount % 6 == 0) rightCoords += "\n";
            }
        }
        if (rightCount > 0)
            Debug.Log(rightCoords.TrimEnd(',', ' '));
        else
            Debug.Log("(нет ячеек)");

        Debug.Log("");
        Debug.Log($"💡 ДЛЯ WAVE CONFIG:");
        Debug.Log($"   → Используйте координаты из ПРАВОЙ стороны (красные)");
        Debug.Log($"   → ИЛИ включите Random Position и настройте:");
        Debug.Log($"      Spawn Zone X: ({middleX + 1}, {maxX})");
        Debug.Log($"      Spawn Zone Y: ({minY}, {maxY})");
        Debug.Log("═══════════════════════════════════════════════════════════");
    }

    [ContextMenu("📊 Статистика сетки")]
    private void ShowGridStats()
    {
        // Получаем ячейки заново
        GridCell[] allCells = GetComponentsInChildren<GridCell>();
        
        Debug.Log("═══════════════════════════════════════════════════════════");
        Debug.Log("📊 СТАТИСТИКА СЕТКИ:");
        Debug.Log("═══════════════════════════════════════════════════════════");
        Debug.Log($"Всего ячеек: {allCells.Length}");
        
        if (Application.isPlaying)
        {
            Debug.Log($"Занято: {GetOccupiedCells().Count}");
            Debug.Log($"Свободно: {GetFreeCells().Count}");
            Debug.Log($"Размещение заблокировано: {(isPlacementLocked ? "ДА 🔒" : "НЕТ 🔓")}");
        }
        else
        {
            Debug.Log("⏸️ (Запустите игру для детальной статистики)");
        }
        
        Debug.Log("═══════════════════════════════════════════════════════════");
    }

#if UNITY_EDITOR
    [ContextMenu("🔧 Автоматически установить координаты по позиции")]
    private void AutoAssignCoordinates()
    {
        GridCell[] allCells = GetComponentsInChildren<GridCell>();
        
        if (allCells.Length == 0)
        {
            Debug.LogWarning("❌ Ячейки не найдены!");
            return;
        }

        Debug.Log("═══════════════════════════════════════════════════════════");
        Debug.Log($"🔧 АВТОМАТИЧЕСКАЯ УСТАНОВКА КООРДИНАТ ({allCells.Length} ячеек)");
        Debug.Log("═══════════════════════════════════════════════════════════");

        // Находим позиции всех ячеек
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        foreach (var cell in allCells)
        {
            Vector3 pos = cell.transform.position;
            if (pos.x < minX) minX = pos.x;
            if (pos.x > maxX) maxX = pos.x;
            if (pos.y < minY) minY = pos.y;
            if (pos.y > maxY) maxY = pos.y;
        }

        // Вычисляем шаг сетки
        float stepX = 0;
        float stepY = 0;
        
        // Ищем два ближайших разных X
        for (int i = 0; i < allCells.Length; i++)
        {
            for (int j = i + 1; j < allCells.Length; j++)
            {
                float diffX = Mathf.Abs(allCells[i].transform.position.x - allCells[j].transform.position.x);
                if (diffX > 0.01f && (stepX == 0 || diffX < stepX))
                    stepX = diffX;
                    
                float diffY = Mathf.Abs(allCells[i].transform.position.y - allCells[j].transform.position.y);
                if (diffY > 0.01f && (stepY == 0 || diffY < stepY))
                    stepY = diffY;
            }
        }

        if (stepX == 0) stepX = 100; // Если только одна ячейка по X
        if (stepY == 0) stepY = 100; // Если только одна ячейка по Y

        Debug.Log($"📐 Границы сетки: X=[{minX:F1}..{maxX:F1}], Y=[{minY:F1}..{maxY:F1}]");
        Debug.Log($"📏 Шаг сетки: X={stepX:F1}, Y={stepY:F1}");

        int updatedCount = 0;

        foreach (var cell in allCells)
        {
            Vector3 pos = cell.transform.position;
            
            // Вычисляем координаты на основе позиции
            int gridX = Mathf.RoundToInt((pos.x - minX) / stepX);
            int gridY = Mathf.RoundToInt((pos.y - minY) / stepY);

            Vector2Int oldCoord = cell.gridPosition;
            cell.gridPosition = new Vector2Int(gridX, gridY);
            
            // Помечаем объект как изменённый для сохранения
            UnityEditor.EditorUtility.SetDirty(cell);
            
            updatedCount++;
        }

        Debug.Log($"✅ Обновлено координат: {updatedCount}");
        Debug.Log("💾 Не забудьте СОХРАНИТЬ сцену! (Ctrl+S)");
        Debug.Log("═══════════════════════════════════════════════════════════");
        
        // Показываем результат
        ShowAllCoordinates();
    }

    [ContextMenu("🔧 Установить координаты вручную (сетка 10x10)")]
    private void AutoAssignCoordinates10x10()
    {
        GridCell[] allCells = GetComponentsInChildren<GridCell>();
        
        if (allCells.Length == 0)
        {
            Debug.LogWarning("❌ Ячейки не найдены!");
            return;
        }

        Debug.Log("═══════════════════════════════════════════════════════════");
        Debug.Log($"🔧 УСТАНОВКА КООРДИНАТ 10x10 ({allCells.Length} ячеек)");
        Debug.Log("═══════════════════════════════════════════════════════════");

        // Сортируем по позиции (слева-направо, снизу-вверх)
        System.Array.Sort(allCells, (a, b) => {
            float diffY = a.transform.position.y - b.transform.position.y;
            if (Mathf.Abs(diffY) > 0.1f)
                return diffY < 0 ? -1 : 1;
            float diffX = a.transform.position.x - b.transform.position.x;
            return diffX < 0 ? -1 : 1;
        });

        int gridWidth = 10; // Ширина сетки
        int updatedCount = 0;

        for (int i = 0; i < allCells.Length; i++)
        {
            int gridX = i % gridWidth;
            int gridY = i / gridWidth;

            allCells[i].gridPosition = new Vector2Int(gridX, gridY);
            UnityEditor.EditorUtility.SetDirty(allCells[i]);
            
            updatedCount++;
        }

        Debug.Log($"✅ Обновлено координат: {updatedCount}");
        Debug.Log($"📐 Сетка: {gridWidth}x{(allCells.Length / gridWidth)}");
        Debug.Log("💾 Не забудьте СОХРАНИТЬ сцену! (Ctrl+S)");
        Debug.Log("═══════════════════════════════════════════════════════════");
        
        ShowAllCoordinates();
    }
#endif
}

