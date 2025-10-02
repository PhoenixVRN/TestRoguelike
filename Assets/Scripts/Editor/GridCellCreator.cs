using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// Инструмент для создания сетки ячеек в редакторе
/// </summary>
public class GridCellCreator : EditorWindow
{
    private GameObject gridContainer;
    private int gridWidth = 10;
    private int gridHeight = 10;
    private float cellWidth = 64f;
    private float cellHeight = 32f;
    private float offsetX = 0f;
    private float offsetY = 0f;
    private Color cellColor = new Color(0, 1, 0, 0.3f);
    private float cellMarkerSize = 20f;
    private bool createAsIsometric = true;

    [MenuItem("Tools/Grid Cell Creator")]
    public static void ShowWindow()
    {
        GetWindow<GridCellCreator>("Grid Cell Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Создание сетки ячеек", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "Этот инструмент создаст пустышки-ячейки для размещения объектов.\n" +
            "1. Выберите контейнер (Canvas или Image с сеткой)\n" +
            "2. Настройте параметры\n" +
            "3. Нажмите 'Создать сетку'",
            MessageType.Info
        );

        GUILayout.Space(10);

        // Выбор контейнера
        GUILayout.Label("Контейнер:", EditorStyles.boldLabel);
        gridContainer = (GameObject)EditorGUILayout.ObjectField(
            "Grid Container",
            gridContainer,
            typeof(GameObject),
            true
        );

        GUILayout.Space(10);

        // Параметры сетки
        GUILayout.Label("Параметры сетки:", EditorStyles.boldLabel);
        
        createAsIsometric = EditorGUILayout.Toggle("Изометрическая сетка", createAsIsometric);
        
        gridWidth = EditorGUILayout.IntField("Ширина сетки (ячеек)", gridWidth);
        gridHeight = EditorGUILayout.IntField("Высота сетки (ячеек)", gridHeight);
        
        GUILayout.Space(5);
        
        cellWidth = EditorGUILayout.FloatField("Ширина ромба (px)", cellWidth);
        cellHeight = EditorGUILayout.FloatField("Высота ромба (px)", cellHeight);
        
        GUILayout.Space(5);
        
        offsetX = EditorGUILayout.FloatField("Смещение X", offsetX);
        offsetY = EditorGUILayout.FloatField("Смещение Y", offsetY);
        
        GUILayout.Space(10);
        
        // Визуализация
        GUILayout.Label("Визуализация:", EditorStyles.boldLabel);
        cellColor = EditorGUILayout.ColorField("Цвет маркеров", cellColor);
        cellMarkerSize = EditorGUILayout.FloatField("Размер маркеров", cellMarkerSize);

        GUILayout.Space(20);

        // Кнопки
        if (GUILayout.Button("Создать сетку", GUILayout.Height(40)))
        {
            CreateGrid();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Очистить сетку", GUILayout.Height(30)))
        {
            ClearGrid();
        }

        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            $"Будет создано {gridWidth * gridHeight} ячеек",
            MessageType.None
        );
    }

    private void CreateGrid()
    {
        if (gridContainer == null)
        {
            EditorUtility.DisplayDialog(
                "Ошибка",
                "Выберите контейнер для сетки!",
                "OK"
            );
            return;
        }

        // Очищаем старые ячейки
        ClearGrid();

        int totalCells = gridWidth * gridHeight;
        int created = 0;

        // Создаем ячейки
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                // Вычисляем позицию ячейки
                Vector2 position = CalculateCellPosition(x, y);
                
                // Создаем ячейку
                GameObject cell = CreateCell(x, y, position);
                
                created++;
                
                // Показываем прогресс
                EditorUtility.DisplayProgressBar(
                    "Создание сетки",
                    $"Создано {created}/{totalCells} ячеек",
                    (float)created / totalCells
                );
            }
        }

        EditorUtility.ClearProgressBar();

        // Добавляем GridManager если его нет
        if (gridContainer.GetComponent<GridManager>() == null)
        {
            gridContainer.AddComponent<GridManager>();
        }

        EditorUtility.DisplayDialog(
            "Готово!",
            $"Создано {created} ячеек сетки!",
            "OK"
        );

        Debug.Log($"Сетка создана: {gridWidth}x{gridHeight} = {created} ячеек");
    }

    private GameObject CreateCell(int x, int y, Vector2 position)
    {
        // Создаем GameObject
        GameObject cellObj = new GameObject($"Cell_{x}_{y}");
        cellObj.transform.SetParent(gridContainer.transform, false);

        // Добавляем RectTransform
        RectTransform rectTransform = cellObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(cellMarkerSize, cellMarkerSize);
        rectTransform.anchoredPosition = position;

        // Добавляем Image для визуализации
        Image image = cellObj.AddComponent<Image>();
        image.color = cellColor;
        image.raycastTarget = true;

        // Добавляем компонент GridCell
        GridCell gridCell = cellObj.AddComponent<GridCell>();
        gridCell.gridPosition = new Vector2Int(x, y);

        return cellObj;
    }

    private Vector2 CalculateCellPosition(int x, int y)
    {
        if (createAsIsometric)
        {
            // Изометрическая проекция (ромбовидная сетка)
            float posX = (x - y) * (cellWidth / 2f);
            float posY = (x + y) * (cellHeight / 2f);
            
            return new Vector2(posX + offsetX, posY + offsetY);
        }
        else
        {
            // Прямоугольная сетка
            float posX = x * cellWidth;
            float posY = y * cellHeight;
            
            return new Vector2(posX + offsetX, posY + offsetY);
        }
    }

    private void ClearGrid()
    {
        if (gridContainer == null)
            return;

        // Находим все GridCell компоненты
        GridCell[] cells = gridContainer.GetComponentsInChildren<GridCell>();
        
        if (cells.Length == 0)
            return;

        bool confirm = EditorUtility.DisplayDialog(
            "Очистка сетки",
            $"Удалить {cells.Length} ячеек?",
            "Да",
            "Отмена"
        );

        if (!confirm)
            return;

        foreach (GridCell cell in cells)
        {
            DestroyImmediate(cell.gameObject);
        }

        Debug.Log($"Удалено {cells.Length} ячеек");
    }
}

