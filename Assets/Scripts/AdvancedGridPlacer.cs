using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// Расширенная система размещения с проверкой занятости ячеек и удалением объектов
/// </summary>
public class AdvancedGridPlacer : MonoBehaviour, IPointerClickHandler
{
    [Header("Настройки сетки")]
    [Tooltip("Ширина одного ромба в пикселях")]
    [SerializeField] private float diamondWidth = 64f;
    
    [Tooltip("Высота одного ромба в пикселях")]
    [SerializeField] private float diamondHeight = 32f;
    
    [Header("Префаб для размещения")]
    [Tooltip("UI префаб, который будет размещаться по клику")]
    [SerializeField] private GameObject prefabToPlace;
    
    /// <summary>Текущий активный префаб для размещения</summary>
    private GameObject currentPrefab;
    
    [Header("Контейнер")]
    [Tooltip("Родительский объект для размещаемых префабов")]
    [SerializeField] private RectTransform container;
    
    [Header("Настройки")]
    [Tooltip("Смещение сетки по X")]
    [SerializeField] private float offsetX = 0f;
    
    [Tooltip("Смещение сетки по Y")]
    [SerializeField] private float offsetY = 0f;
    
    [Tooltip("Разрешить размещение только в свободных ячейках")]
    [SerializeField] private bool checkOccupancy = true;
    
    [Tooltip("ПКМ удаляет объект из ячейки")]
    [SerializeField] private bool rightClickToRemove = true;
    
    [Tooltip("Показывать отладочную информацию")]
    [SerializeField] private bool showDebug = false;

    [Header("Визуализация")]
    [Tooltip("Показывать подсветку ячейки при наведении")]
    [SerializeField] private bool showHoverHighlight = true;
    
    [Tooltip("Префаб для подсветки ячейки")]
    [SerializeField] private GameObject highlightPrefab;

    private RectTransform rectTransform;
    private Dictionary<Vector2Int, GameObject> occupiedCells = new Dictionary<Vector2Int, GameObject>();
    private GameObject currentHighlight;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        if (container == null)
        {
            container = rectTransform;
        }
        
        currentPrefab = prefabToPlace;
    }

    /// <summary>
    /// Обработчик клика мыши
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        // Получаем локальную позицию клика
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );

        // Преобразуем в координаты сетки
        Vector2Int gridCoords = ScreenToGrid(localPoint);
        
        // ЛКМ - размещение
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            HandleLeftClick(gridCoords);
        }
        // ПКМ - удаление
        else if (eventData.button == PointerEventData.InputButton.Right && rightClickToRemove)
        {
            HandleRightClick(gridCoords);
        }
    }

    /// <summary>
    /// Обработка левого клика (размещение)
    /// </summary>
    private void HandleLeftClick(Vector2Int gridCoords)
    {
        GameObject prefab = currentPrefab ?? prefabToPlace;
        
        if (prefab == null)
        {
            Debug.LogWarning("Префаб для размещения не назначен!");
            return;
        }

        // Проверяем, занята ли ячейка
        if (checkOccupancy && occupiedCells.ContainsKey(gridCoords))
        {
            if (showDebug)
            {
                Debug.Log($"Ячейка {gridCoords} уже занята!");
            }
            return;
        }

        // Получаем центр ромба
        Vector2 diamondCenter = GridToScreen(gridCoords);
        
        // Создаем префаб
        GameObject instance = PlacePrefab(prefab, diamondCenter, gridCoords);
        
        // Отмечаем ячейку как занятую
        if (checkOccupancy)
        {
            occupiedCells[gridCoords] = instance;
        }
        
        if (showDebug)
        {
            Debug.Log($"Размещен объект в ячейке {gridCoords}, позиция: {diamondCenter}");
        }
    }

    /// <summary>
    /// Обработка правого клика (удаление)
    /// </summary>
    private void HandleRightClick(Vector2Int gridCoords)
    {
        if (occupiedCells.TryGetValue(gridCoords, out GameObject obj))
        {
            occupiedCells.Remove(gridCoords);
            Destroy(obj);
            
            if (showDebug)
            {
                Debug.Log($"Удален объект из ячейки {gridCoords}");
            }
        }
    }

    /// <summary>
    /// Преобразует экранные координаты в координаты сетки
    /// </summary>
    private Vector2Int ScreenToGrid(Vector2 screenPos)
    {
        float x = screenPos.x - offsetX;
        float y = screenPos.y - offsetY;

        float gridX = (x / (diamondWidth / 2f) + y / (diamondHeight / 2f)) / 2f;
        float gridY = (y / (diamondHeight / 2f) - x / (diamondWidth / 2f)) / 2f;

        int cellX = Mathf.RoundToInt(gridX);
        int cellY = Mathf.RoundToInt(gridY);

        return new Vector2Int(cellX, cellY);
    }

    /// <summary>
    /// Преобразует координаты сетки обратно в экранные (центр ромба)
    /// </summary>
    private Vector2 GridToScreen(Vector2Int gridCoords)
    {
        float x = (gridCoords.x - gridCoords.y) * (diamondWidth / 2f);
        float y = (gridCoords.x + gridCoords.y) * (diamondHeight / 2f);

        x += offsetX;
        y += offsetY;

        return new Vector2(x, y);
    }

    /// <summary>
    /// Размещает префаб в указанной позиции
    /// </summary>
    private GameObject PlacePrefab(GameObject prefab, Vector2 position, Vector2Int gridCoords)
    {
        GameObject instance = Instantiate(prefab, container);
        RectTransform instanceRect = instance.GetComponent<RectTransform>();
        
        if (instanceRect != null)
        {
            instanceRect.anchoredPosition = position;
        }

        var gridInfo = instance.AddComponent<GridItemInfo>();
        gridInfo.gridPosition = gridCoords;

        return instance;
    }
    
    /// <summary>
    /// Устанавливает текущий префаб для размещения
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
    /// Получает текущий префаб для размещения
    /// </summary>
    public GameObject GetCurrentPrefab()
    {
        return currentPrefab ?? prefabToPlace;
    }

    /// <summary>
    /// Проверяет, занята ли ячейка
    /// </summary>
    public bool IsCellOccupied(Vector2Int gridCoords)
    {
        return occupiedCells.ContainsKey(gridCoords);
    }

    /// <summary>
    /// Получает объект в указанной ячейке
    /// </summary>
    public GameObject GetObjectAtCell(Vector2Int gridCoords)
    {
        occupiedCells.TryGetValue(gridCoords, out GameObject obj);
        return obj;
    }

    /// <summary>
    /// Очищает все размещенные объекты
    /// </summary>
    public void ClearAll()
    {
        foreach (var obj in occupiedCells.Values)
        {
            if (obj != null)
                Destroy(obj);
        }
        occupiedCells.Clear();
    }
}

