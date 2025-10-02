using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Визуализатор сетки для помощи в настройке параметров
/// Показывает центры ромбов и границы ячеек
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class GridVisualizer : MonoBehaviour
{
    [Header("Настройки сетки")]
    [Tooltip("Ширина одного ромба в пикселях")]
    [SerializeField] private float diamondWidth = 64f;
    
    [Tooltip("Высота одного ромба в пикселях")]
    [SerializeField] private float diamondHeight = 32f;
    
    [Header("Визуализация")]
    [Tooltip("Показывать визуализацию")]
    [SerializeField] private bool showVisualization = true;
    
    [Tooltip("Количество ячеек по X")]
    [SerializeField] private int gridSizeX = 10;
    
    [Tooltip("Количество ячеек по Y")]
    [SerializeField] private int gridSizeY = 10;
    
    [Tooltip("Цвет точек центров")]
    [SerializeField] private Color centerColor = Color.green;
    
    [Tooltip("Цвет линий сетки")]
    [SerializeField] private Color gridLineColor = Color.yellow;
    
    [Tooltip("Размер точек центров")]
    [SerializeField] private float centerDotSize = 5f;
    
    [Tooltip("Показывать линии сетки")]
    [SerializeField] private bool showGridLines = true;
    
    [Tooltip("Показывать координаты")]
    [SerializeField] private bool showCoordinates = false;
    
    [Header("Смещение")]
    [Tooltip("Смещение сетки по X")]
    [SerializeField] private float offsetX = 0f;
    
    [Tooltip("Смещение сетки по Y")]
    [SerializeField] private float offsetY = 0f;

    private RectTransform rectTransform;
    private Canvas canvas;
    private GameObject visualizationContainer;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    private void Start()
    {
        if (showVisualization)
        {
            CreateVisualization();
        }
    }

    private void OnValidate()
    {
        // Пересоздаем визуализацию при изменении параметров в инспекторе
        if (Application.isPlaying && showVisualization)
        {
            DestroyVisualization();
            CreateVisualization();
        }
    }

    /// <summary>
    /// Создает визуализацию сетки
    /// </summary>
    private void CreateVisualization()
    {
        DestroyVisualization();

        visualizationContainer = new GameObject("GridVisualization");
        visualizationContainer.transform.SetParent(transform, false);
        
        RectTransform containerRect = visualizationContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.sizeDelta = rectTransform.sizeDelta;
        containerRect.anchoredPosition = Vector2.zero;

        // Создаем точки в центрах ромбов
        for (int x = -gridSizeX; x <= gridSizeX; x++)
        {
            for (int y = -gridSizeY; y <= gridSizeY; y++)
            {
                Vector2Int gridCoords = new Vector2Int(x, y);
                Vector2 centerPos = GridToScreen(gridCoords);
                
                CreateCenterDot(centerPos, gridCoords);
            }
        }

        // Создаем линии сетки если нужно
        if (showGridLines)
        {
            CreateGridLines();
        }
    }

    /// <summary>
    /// Создает точку в центре ромба
    /// </summary>
    private void CreateCenterDot(Vector2 position, Vector2Int gridCoords)
    {
        GameObject dot = new GameObject($"Dot_{gridCoords.x}_{gridCoords.y}");
        dot.transform.SetParent(visualizationContainer.transform, false);
        
        Image dotImage = dot.AddComponent<Image>();
        dotImage.color = centerColor;
        
        RectTransform dotRect = dot.GetComponent<RectTransform>();
        dotRect.anchorMin = new Vector2(0.5f, 0.5f);
        dotRect.anchorMax = new Vector2(0.5f, 0.5f);
        dotRect.sizeDelta = new Vector2(centerDotSize, centerDotSize);
        dotRect.anchoredPosition = position;

        // Добавляем текст с координатами если нужно
        if (showCoordinates)
        {
            GameObject textObj = new GameObject("Coordinates");
            textObj.transform.SetParent(dot.transform, false);
            
            Text coordText = textObj.AddComponent<Text>();
            coordText.text = $"{gridCoords.x},{gridCoords.y}";
            coordText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            coordText.fontSize = 10;
            coordText.alignment = TextAnchor.MiddleCenter;
            coordText.color = Color.white;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.sizeDelta = new Vector2(50, 20);
            textRect.anchoredPosition = new Vector2(0, 15);
        }
    }

    /// <summary>
    /// Создает линии сетки
    /// </summary>
    private void CreateGridLines()
    {
        // Вертикальные линии
        for (int x = -gridSizeX; x <= gridSizeX; x++)
        {
            Vector2 start = GridToScreen(new Vector2Int(x, -gridSizeY));
            Vector2 end = GridToScreen(new Vector2Int(x, gridSizeY));
            CreateLine(start, end, $"VLine_{x}");
        }

        // Горизонтальные линии
        for (int y = -gridSizeY; y <= gridSizeY; y++)
        {
            Vector2 start = GridToScreen(new Vector2Int(-gridSizeX, y));
            Vector2 end = GridToScreen(new Vector2Int(gridSizeX, y));
            CreateLine(start, end, $"HLine_{y}");
        }
    }

    /// <summary>
    /// Создает линию между двумя точками
    /// </summary>
    private void CreateLine(Vector2 start, Vector2 end, string name)
    {
        GameObject lineObj = new GameObject(name);
        lineObj.transform.SetParent(visualizationContainer.transform, false);
        
        Image lineImage = lineObj.AddComponent<Image>();
        lineImage.color = gridLineColor;
        
        RectTransform lineRect = lineObj.GetComponent<RectTransform>();
        lineRect.anchorMin = new Vector2(0.5f, 0.5f);
        lineRect.anchorMax = new Vector2(0.5f, 0.5f);
        
        Vector2 dir = end - start;
        float distance = dir.magnitude;
        
        lineRect.sizeDelta = new Vector2(distance, 1f);
        lineRect.anchoredPosition = (start + end) / 2f;
        lineRect.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
    }

    /// <summary>
    /// Преобразует координаты сетки в экранные
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
    /// Уничтожает визуализацию
    /// </summary>
    private void DestroyVisualization()
    {
        if (visualizationContainer != null)
        {
            if (Application.isPlaying)
                Destroy(visualizationContainer);
            else
                DestroyImmediate(visualizationContainer);
        }
    }

    /// <summary>
    /// Включить/выключить визуализацию
    /// </summary>
    public void ToggleVisualization(bool show)
    {
        showVisualization = show;
        
        if (show)
            CreateVisualization();
        else
            DestroyVisualization();
    }

    private void OnDestroy()
    {
        DestroyVisualization();
    }
}

