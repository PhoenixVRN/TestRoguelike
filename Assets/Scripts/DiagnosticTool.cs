using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Инструмент диагностики - добавьте на Grid для проверки
/// </summary>
public class DiagnosticTool : MonoBehaviour
{
    [Header("Запустите и посмотрите логи")]
    [SerializeField] private bool runDiagnostic = true;

    private void Start()
    {
        if (runDiagnostic)
        {
            RunFullDiagnostic();
        }
    }

    [ContextMenu("Запустить диагностику")]
    public void RunFullDiagnostic()
    {
        Debug.Log("═══════════════════════════════════════");
        Debug.Log("ДИАГНОСТИКА СИСТЕМЫ");
        Debug.Log("═══════════════════════════════════════");

        // Проверка 1: EventSystem
        CheckEventSystem();

        // Проверка 2: Canvas и GraphicRaycaster
        CheckCanvas();

        // Проверка 3: GridManager
        CheckGridManager();

        // Проверка 4: GridCells
        CheckGridCells();

        Debug.Log("═══════════════════════════════════════");
        Debug.Log("ДИАГНОСТИКА ЗАВЕРШЕНА");
        Debug.Log("═══════════════════════════════════════");
    }

    private void CheckEventSystem()
    {
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        
        if (eventSystem == null)
        {
            Debug.LogError("❌ EventSystem НЕ НАЙДЕН! Создайте: Hierarchy → UI → Event System");
        }
        else
        {
            Debug.Log("✅ EventSystem найден: " + eventSystem.gameObject.name);
        }
    }

    private void CheckCanvas()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        
        if (canvas == null)
        {
            Debug.LogError("❌ Canvas НЕ НАЙДЕН! Этот объект должен быть внутри Canvas!");
            return;
        }
        
        Debug.Log("✅ Canvas найден: " + canvas.gameObject.name);

        GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
        if (raycaster == null)
        {
            Debug.LogError("❌ GraphicRaycaster НЕ НАЙДЕН на Canvas! Добавьте компонент!");
        }
        else
        {
            Debug.Log("✅ GraphicRaycaster найден на Canvas");
        }
    }

    private void CheckGridManager()
    {
        GridManager gridManager = GetComponent<GridManager>();
        
        if (gridManager == null)
        {
            Debug.LogError("❌ GridManager НЕ НАЙДЕН на этом объекте!");
            return;
        }
        
        Debug.Log("✅ GridManager найден");

        GameObject prefab = gridManager.GetCurrentPrefab();
        if (prefab == null)
        {
            Debug.LogError("❌ ПРЕФАБ НЕ НАЗНАЧЕН! Назначьте 'Prefab To Place' в GridManager!");
        }
        else
        {
            Debug.Log("✅ Префаб назначен: " + prefab.name);
            
            // Проверяем что у префаба есть RectTransform
            RectTransform rect = prefab.GetComponent<RectTransform>();
            if (rect == null)
            {
                Debug.LogWarning("⚠️ У префаба НЕТ RectTransform! Это должен быть UI объект!");
            }
            else
            {
                Debug.Log("✅ У префаба есть RectTransform");
            }
        }

        // Проверяем ячейки
        var cells = gridManager.GetAllCells();
        Debug.Log($"📊 Найдено ячеек: {cells.Count}");
        
        if (cells.Count == 0)
        {
            Debug.LogWarning("⚠️ НЕТ ЯЧЕЕК! Добавьте GridCell как дочерние объекты Grid");
        }
    }

    private void CheckGridCells()
    {
        GridCell[] cells = GetComponentsInChildren<GridCell>();
        
        Debug.Log($"🔍 Проверка {cells.Length} ячеек...");

        int cellsWithImage = 0;
        int cellsWithRaycast = 0;

        foreach (GridCell cell in cells)
        {
            Image image = cell.GetComponent<Image>();
            
            if (image == null)
            {
                Debug.LogWarning($"⚠️ У ячейки '{cell.gameObject.name}' НЕТ Image компонента!");
            }
            else
            {
                cellsWithImage++;
                
                if (!image.raycastTarget)
                {
                    Debug.LogWarning($"⚠️ У ячейки '{cell.gameObject.name}' ВЫКЛЮЧЕН Raycast Target!");
                }
                else
                {
                    cellsWithRaycast++;
                }
            }
        }

        Debug.Log($"✅ Ячеек с Image: {cellsWithImage}/{cells.Length}");
        Debug.Log($"✅ Ячеек с Raycast Target: {cellsWithRaycast}/{cells.Length}");

        if (cellsWithRaycast == 0 && cells.Length > 0)
        {
            Debug.LogError("❌ НИ У ОДНОЙ ЯЧЕЙКИ НЕ ВКЛЮЧЕН Raycast Target! КЛИКИ НЕ БУДУТ РАБОТАТЬ!");
        }
    }
}

