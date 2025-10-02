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
        if (!rightClickToRemove)
            return;

        if (cell.IsOccupied())
        {
            cell.RemoveObject();
            
            if (showDebug)
            {
                Debug.Log($"Удален объект из ячейки {cell.gridPosition}");
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
}

