using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Ячейка сетки - маркер для размещения объектов
/// Размещайте эти пустышки вручную в центрах ромбов на сцене
/// </summary>
public class GridCell : MonoBehaviour, IPointerClickHandler
{
    [Header("Настройки")]
    [Tooltip("Координаты ячейки в сетке (для справки)")]
    public Vector2Int gridPosition;
    
    [Tooltip("Смещение позиции размещаемых героев (X, Y)")]
    [SerializeField] private Vector2 placementOffset = Vector2.zero;
    
    [Tooltip("Размещенный объект в этой ячейке")]
    private GameObject placedObject;
    
    [Header("Визуализация (опционально)")]
    [Tooltip("Показывать маркер в игре")]
    [SerializeField] private bool showMarkerInGame = false;
    
    [Tooltip("Цвет свободной ячейки")]
    [SerializeField] private Color freeColor = new Color(0, 1, 0, 0.3f);
    
    [Tooltip("Цвет занятой ячейки")]
    [SerializeField] private Color occupiedColor = new Color(1, 0, 0, 0.3f);
    
    private UnityEngine.UI.Image markerImage;
    private GridManager gridManager;

    private void Awake()
    {
        markerImage = GetComponent<UnityEngine.UI.Image>();
        gridManager = GetComponentInParent<GridManager>();
    }

    private void Start()
    {
        UpdateVisuals();
    }

    /// <summary>
    /// Обработчик клика по ячейке
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (gridManager == null)
        {
            Debug.LogWarning("GridManager не найден!");
            return;
        }

        // ЛКМ - размещение
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            gridManager.OnCellClicked(this, eventData);
        }
        // ПКМ - удаление
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            gridManager.OnCellRightClicked(this, eventData);
        }
    }

    /// <summary>
    /// Разместить объект в ячейке
    /// </summary>
    public bool PlaceObject(GameObject prefab)
    {
        if (placedObject != null)
        {
            Debug.LogWarning($"Ячейка {gridPosition} уже занята!");
            return false;
        }

        // Находим Grid (родителя) для размещения там, а не в GridCell
        Transform gridParent = transform.parent;
        if (gridParent == null)
        {
            gridParent = transform;
        }

        // Создаем префаб как дочерний объект Grid (не GridCell!)
        placedObject = Instantiate(prefab, gridParent);
        
        RectTransform placedRect = placedObject.GetComponent<RectTransform>();
        if (placedRect != null)
        {
            // Сохраняем исходный Scale из префаба
            Vector3 originalScale = placedRect.localScale;
            
            // Копируем мировую позицию этой ячейки + применяем офсет
            RectTransform myRect = GetComponent<RectTransform>();
            placedRect.position = myRect.position;
            
            // Применяем смещение
            placedRect.anchoredPosition += placementOffset;
            
            // Восстанавливаем исходный Scale (не перезаписываем!)
            placedRect.localScale = originalScale;
            placedRect.localRotation = Quaternion.identity; // Всегда вертикально!
        }

        UpdateVisuals();
        return true;
    }

    /// <summary>
    /// Удалить объект из ячейки
    /// </summary>
    public void RemoveObject()
    {
        if (placedObject != null)
        {
            Destroy(placedObject);
            placedObject = null;
            UpdateVisuals();
        }
    }

    /// <summary>
    /// Обновить позицию размещенного объекта (если GridCell двигается)
    /// </summary>
    private void LateUpdate()
    {
        // ОТКЛЮЧАЕМ синхронизацию если бой начался (персонажи должны свободно двигаться!)
        GridManager manager = GetComponentInParent<GridManager>();
        if (manager != null && manager.IsPlacementLocked())
        {
            // Бой идёт - не трогаем позицию персонажа!
            return;
        }
        
        // Синхронизируем позицию размещенного объекта с ячейкой (только в режиме расстановки)
        if (placedObject != null)
        {
            RectTransform placedRect = placedObject.GetComponent<RectTransform>();
            RectTransform myRect = GetComponent<RectTransform>();
            
            if (placedRect != null && myRect != null)
            {
                placedRect.position = myRect.position;
                // Применяем смещение при обновлении позиции
                placedRect.anchoredPosition += placementOffset;
            }
        }
    }
    
    /// <summary>
    /// Установить смещение для размещения
    /// </summary>
    public void SetPlacementOffset(Vector2 offset)
    {
        placementOffset = offset;
    }
    
    /// <summary>
    /// Получить текущее смещение
    /// </summary>
    public Vector2 GetPlacementOffset()
    {
        return placementOffset;
    }

    /// <summary>
    /// Проверка, занята ли ячейка
    /// </summary>
    public bool IsOccupied()
    {
        return placedObject != null;
    }

    /// <summary>
    /// Получить размещенный объект
    /// </summary>
    public GameObject GetPlacedObject()
    {
        return placedObject;
    }

    /// <summary>
    /// Обновить визуальное отображение
    /// </summary>
    private void UpdateVisuals()
    {
        if (markerImage != null)
        {
            // Image ВСЕГДА включен (для кликов), но меняем видимость через прозрачность
            markerImage.enabled = true;
            
            Color targetColor = IsOccupied() ? occupiedColor : freeColor;
            
            // В игре скрываем маркер если нужно (делаем прозрачным)
            if (Application.isPlaying && !showMarkerInGame)
            {
                targetColor.a = 0; // Полностью прозрачный, но клики работают!
            }
            
            markerImage.color = targetColor;
        }
    }

    /// <summary>
    /// Показать/скрыть маркер
    /// </summary>
    public void SetMarkerVisible(bool visible)
    {
        showMarkerInGame = visible;
        UpdateVisuals();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Показываем координаты в редакторе
        UnityEditor.Handles.Label(transform.position, $"[{gridPosition.x},{gridPosition.y}]");
    }
#endif
}

