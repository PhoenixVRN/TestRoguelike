using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Детальная диагностика кликов - покажет ЧТО именно получает клик
/// </summary>
public class ClickDebugger : MonoBehaviour
{
    private GraphicRaycaster raycaster;
    private EventSystem eventSystem;

    private void Start()
    {
        raycaster = FindObjectOfType<GraphicRaycaster>();
        eventSystem = FindObjectOfType<EventSystem>();
    }

    private void Update()
    {
        // При каждом клике показываем что под мышкой
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("════════════════════════════════════");
            Debug.Log("🖱️ КЛИК ЛКМ! Проверяем что под курсором...");
            CheckWhatIsUnderMouse();
        }
    }

    private void CheckWhatIsUnderMouse()
    {
        if (raycaster == null || eventSystem == null)
        {
            Debug.LogError("❌ Нет GraphicRaycaster или EventSystem!");
            return;
        }

        PointerEventData pointerData = new PointerEventData(eventSystem);
        pointerData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerData, results);

        Debug.Log($"📊 Найдено объектов под курсором: {results.Count}");

        if (results.Count == 0)
        {
            Debug.LogWarning("⚠️ НЕТ UI ОБЪЕКТОВ под курсором!");
            Debug.LogWarning("Возможно кликаете МИМО ячеек!");
        }
        else
        {
            for (int i = 0; i < results.Count; i++)
            {
                GameObject obj = results[i].gameObject;
                Debug.Log($"[{i}] 🎯 {obj.name} (компоненты: {GetComponents(obj)})");
                
                // Проверяем есть ли GridCell
                if (obj.GetComponent<GridCell>() != null)
                {
                    Debug.Log($"    ✅ Это GridCell! Должен работать!");
                }
                
                // Проверяем Raycast Target
                Image img = obj.GetComponent<Image>();
                if (img != null && !img.raycastTarget)
                {
                    Debug.Log($"    ⚠️ У Image ВЫКЛЮЧЕН Raycast Target");
                }
            }
        }

        Debug.Log("════════════════════════════════════");
    }

    private string GetComponents(GameObject obj)
    {
        Component[] components = obj.GetComponents<Component>();
        string result = "";
        foreach (var comp in components)
        {
            if (comp is Transform || comp is RectTransform || comp is CanvasRenderer)
                continue;
            result += comp.GetType().Name + ", ";
        }
        return result.TrimEnd(',', ' ');
    }
}

