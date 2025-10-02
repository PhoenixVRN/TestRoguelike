using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Пример расширения для размещения нескольких типов префабов
/// Переключение между префабами на клавиши 1-9
/// </summary>
public class MultiPrefabPlacer : MonoBehaviour
{
    [Header("Привязка к Grid Placer")]
    [SerializeField] private AdvancedGridPlacer gridPlacer;
    
    [Header("Префабы для размещения")]
    [Tooltip("Список префабов (переключение на 1-9)")]
    [SerializeField] private GameObject[] prefabs;
    
    [Header("UI")]
    [Tooltip("Текст для отображения текущего префаба")]
    [SerializeField] private Text currentPrefabText;
    
    [Tooltip("Индикаторы префабов (опционально)")]
    [SerializeField] private Image[] prefabIndicators;
    
    [Header("Настройки")]
    [Tooltip("Цвет активного индикатора")]
    [SerializeField] private Color activeColor = Color.green;
    
    [Tooltip("Цвет неактивного индикатора")]
    [SerializeField] private Color inactiveColor = Color.gray;
    
    private int currentPrefabIndex = 0;

    private void Start()
    {
        if (gridPlacer == null)
        {
            gridPlacer = GetComponent<AdvancedGridPlacer>();
        }

        if (prefabs.Length > 0)
        {
            SelectPrefab(0);
        }
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
            gridPlacer.ClearAll();
            Debug.Log("Сетка очищена!");
        }
    }

    /// <summary>
    /// Выбрать префаб по индексу
    /// </summary>
    public void SelectPrefab(int index)
    {
        if (index < 0 || index >= prefabs.Length)
        {
            Debug.LogWarning($"Некорректный индекс префаба: {index}");
            return;
        }

        currentPrefabIndex = index;
        
        // Устанавливаем текущий префаб в gridPlacer
        if (gridPlacer != null)
        {
            gridPlacer.SetCurrentPrefab(prefabs[index]);
        }
        
        Debug.Log($"Выбран префаб [{index + 1}]: {prefabs[index].name}");
        
        UpdateUI();
    }

    /// <summary>
    /// Получить текущий выбранный префаб
    /// </summary>
    public GameObject GetCurrentPrefab()
    {
        if (currentPrefabIndex >= 0 && currentPrefabIndex < prefabs.Length)
        {
            return prefabs[currentPrefabIndex];
        }
        return null;
    }

    /// <summary>
    /// Обновить UI индикаторы
    /// </summary>
    private void UpdateUI()
    {
        // Обновляем текст
        if (currentPrefabText != null)
        {
            currentPrefabText.text = $"Префаб: {prefabs[currentPrefabIndex].name} ({currentPrefabIndex + 1}/{prefabs.Length})";
        }

        // Обновляем индикаторы
        if (prefabIndicators != null && prefabIndicators.Length > 0)
        {
            for (int i = 0; i < prefabIndicators.Length; i++)
            {
                if (prefabIndicators[i] != null)
                {
                    prefabIndicators[i].color = (i == currentPrefabIndex) ? activeColor : inactiveColor;
                }
            }
        }
    }

    /// <summary>
    /// Добавить префаб в список
    /// </summary>
    public void AddPrefab(GameObject prefab)
    {
        GameObject[] newPrefabs = new GameObject[prefabs.Length + 1];
        prefabs.CopyTo(newPrefabs, 0);
        newPrefabs[prefabs.Length] = prefab;
        prefabs = newPrefabs;
    }

    /// <summary>
    /// Получить информацию о текущем выборе
    /// </summary>
    public string GetCurrentSelectionInfo()
    {
        if (prefabs.Length == 0)
            return "Нет доступных префабов";
        
        return $"Префаб {currentPrefabIndex + 1}/{prefabs.Length}: {prefabs[currentPrefabIndex].name}";
    }

    private void OnGUI()
    {
        // Показываем подсказки на экране
        if (prefabs.Length > 0)
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 16;
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.UpperLeft;

            string help = "УПРАВЛЕНИЕ:\n";
            help += $"1-{Mathf.Min(9, prefabs.Length)} - выбор префаба\n";
            help += "← → - переключение префабов\n";
            help += "ЛКМ - разместить\n";
            help += "ПКМ - удалить\n";
            help += "Delete - очистить всё\n";
            help += $"\nТекущий: {GetCurrentSelectionInfo()}";

            GUI.Label(new Rect(10, 10, 400, 200), help, style);
        }
    }
}

