using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Система для размещения UI префабов на ромбовидной сетке по клику мыши
/// </summary>
public class GridPlacer : MonoBehaviour, IPointerClickHandler
{
    [Header("Настройки сетки")]
    [Tooltip("Ширина одного ромба в пикселях")]
    [SerializeField] private float diamondWidth = 64f;
    
    [Tooltip("Высота одного ромба в пикселях")]
    [SerializeField] private float diamondHeight = 32f;
    
    [Header("Префаб для размещения")]
    [Tooltip("UI префаб, который будет размещаться по клику")]
    [SerializeField] private GameObject prefabToPlace;
    
    [Header("Контейнер")]
    [Tooltip("Родительский объект для размещаемых префабов")]
    [SerializeField] private RectTransform container;
    
    [Header("Настройки")]
    [Tooltip("Смещение сетки по X")]
    [SerializeField] private float offsetX = 0f;
    
    [Tooltip("Смещение сетки по Y")]
    [SerializeField] private float offsetY = 0f;
    
    [Tooltip("Показывать отладочную информацию")]
    [SerializeField] private bool showDebug = false;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        if (container == null)
        {
            container = rectTransform;
        }
    }

    /// <summary>
    /// Обработчик клика мыши
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (prefabToPlace == null)
        {
            Debug.LogWarning("Префаб для размещения не назначен!");
            return;
        }

        // Получаем локальную позицию клика относительно RectTransform
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );

        // Преобразуем в координаты сетки
        Vector2Int gridCoords = ScreenToGrid(localPoint);
        
        // Получаем центр ромба
        Vector2 diamondCenter = GridToScreen(gridCoords);
        
        // Создаем префаб
        PlacePrefab(diamondCenter, gridCoords);
        
        if (showDebug)
        {
            Debug.Log($"Клик в: {localPoint}, Сетка: {gridCoords}, Центр ромба: {diamondCenter}");
        }
    }

    /// <summary>
    /// Преобразует экранные координаты в координаты сетки
    /// </summary>
    private Vector2Int ScreenToGrid(Vector2 screenPos)
    {
        // Применяем смещение
        float x = screenPos.x - offsetX;
        float y = screenPos.y - offsetY;

        // Для изометрической (ромбовидной) сетки используем преобразование
        // Формулы для изометрической проекции:
        float gridX = (x / (diamondWidth / 2f) + y / (diamondHeight / 2f)) / 2f;
        float gridY = (y / (diamondHeight / 2f) - x / (diamondWidth / 2f)) / 2f;

        // Округляем до ближайшей ячейки
        int cellX = Mathf.RoundToInt(gridX);
        int cellY = Mathf.RoundToInt(gridY);

        return new Vector2Int(cellX, cellY);
    }

    /// <summary>
    /// Преобразует координаты сетки обратно в экранные (центр ромба)
    /// </summary>
    private Vector2 GridToScreen(Vector2Int gridCoords)
    {
        // Обратное преобразование для получения центра ромба
        float x = (gridCoords.x - gridCoords.y) * (diamondWidth / 2f);
        float y = (gridCoords.x + gridCoords.y) * (diamondHeight / 2f);

        // Применяем смещение
        x += offsetX;
        y += offsetY;

        return new Vector2(x, y);
    }

    /// <summary>
    /// Размещает префаб в указанной позиции
    /// </summary>
    private void PlacePrefab(Vector2 position, Vector2Int gridCoords)
    {
        GameObject instance = Instantiate(prefabToPlace, container);
        RectTransform instanceRect = instance.GetComponent<RectTransform>();
        
        if (instanceRect != null)
        {
            instanceRect.anchoredPosition = position;
        }

        // Можно добавить компонент с информацией о позиции в сетке
        var gridInfo = instance.AddComponent<GridItemInfo>();
        gridInfo.gridPosition = gridCoords;
    }

    /// <summary>
    /// Отображение отладочной информации в редакторе
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showDebug || !Application.isPlaying)
            return;

        // Рисуем сетку для визуализации
        Gizmos.color = Color.yellow;
        
        // Получаем размеры RectTransform
        RectTransform rect = GetComponent<RectTransform>();
        if (rect == null) return;

        Vector3 worldPos = rect.position;
        
        // Рисуем несколько ячеек сетки для визуализации
        for (int x = -5; x <= 5; x++)
        {
            for (int y = -5; y <= 5; y++)
            {
                Vector2 center = GridToScreen(new Vector2Int(x, y));
                Vector3 worldCenter = worldPos + new Vector3(center.x, center.y, 0);
                
                // Рисуем маленькую точку в центре каждого ромба
                Gizmos.DrawSphere(worldCenter, 2f);
            }
        }
    }
}

/// <summary>
/// Компонент для хранения информации о позиции объекта в сетке
/// </summary>
public class GridItemInfo : MonoBehaviour
{
    public Vector2Int gridPosition;
}

