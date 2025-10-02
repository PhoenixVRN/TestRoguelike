using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Полная диагностика UI системы
/// </summary>
public class FullDiagnostic : MonoBehaviour
{
    [ContextMenu("Полная проверка системы")]
    public void CheckEverything()
    {
        Debug.Log("╔═══════════════════════════════════════╗");
        Debug.Log("║   ПОЛНАЯ ДИАГНОСТИКА UI СИСТЕМЫ       ║");
        Debug.Log("╚═══════════════════════════════════════╝");
        
        CheckCamera();
        CheckCanvas();
        CheckEventSystem();
        CheckGraphicRaycaster();
        CheckGridCells();
        
        Debug.Log("╔═══════════════════════════════════════╗");
        Debug.Log("║   ДИАГНОСТИКА ЗАВЕРШЕНА               ║");
        Debug.Log("╚═══════════════════════════════════════╝");
    }

    private void CheckCamera()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogError("❌ MAIN CAMERA НЕ НАЙДЕНА!");
            Debug.LogError("   Создайте камеру с тегом 'MainCamera'");
        }
        else
        {
            Debug.Log($"✅ Main Camera найдена: {mainCam.gameObject.name}");
            Debug.Log($"   Position: {mainCam.transform.position}");
            Debug.Log($"   Rotation: {mainCam.transform.eulerAngles}");
        }
    }

    private void CheckCanvas()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("❌ CANVAS НЕ НАЙДЕН!");
            return;
        }

        Debug.Log($"✅ Canvas найден: {canvas.gameObject.name}");
        Debug.Log($"   Render Mode: {canvas.renderMode}");
        
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            if (canvas.worldCamera == null)
            {
                Debug.LogError("❌ У Canvas НЕ НАЗНАЧЕНА КАМЕРА!");
                Debug.LogError("   Canvas → Render Camera → перетащите Main Camera");
                Debug.LogError("   ИЛИ измените Render Mode на 'Screen Space - Overlay'");
            }
            else
            {
                Debug.Log($"   ✅ World Camera: {canvas.worldCamera.name}");
            }
        }
        else if (canvas.renderMode == RenderMode.WorldSpace)
        {
            Debug.LogWarning("⚠️ Canvas в режиме World Space!");
            Debug.LogWarning("   Рекомендуется 'Screen Space - Overlay' для UI");
        }
        else
        {
            Debug.Log("   ✅ Режим Screen Space - Overlay");
        }

        // Проверяем положение
        RectTransform rect = canvas.GetComponent<RectTransform>();
        Debug.Log($"   Position: {rect.position}");
        Debug.Log($"   Size: {rect.sizeDelta}");
    }

    private void CheckEventSystem()
    {
        EventSystem es = FindObjectOfType<EventSystem>();
        if (es == null)
        {
            Debug.LogError("❌ EVENT SYSTEM НЕ НАЙДЕН!");
        }
        else
        {
            Debug.Log($"✅ EventSystem найден: {es.gameObject.name}");
        }
    }

    private void CheckGraphicRaycaster()
    {
        GraphicRaycaster[] raycasters = FindObjectsOfType<GraphicRaycaster>();
        
        if (raycasters.Length == 0)
        {
            Debug.LogError("❌ НЕТ GraphicRaycaster!");
        }
        else
        {
            Debug.Log($"✅ Найдено GraphicRaycaster: {raycasters.Length}");
            
            foreach (var raycaster in raycasters)
            {
                Debug.Log($"   - {raycaster.gameObject.name}");
                Debug.Log($"     Ignore Reversed Graphics: {raycaster.ignoreReversedGraphics}");
                Debug.Log($"     Blocking Objects: {raycaster.blockingObjects}");
                
                Canvas canvas = raycaster.GetComponent<Canvas>();
                if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == null)
                {
                    Debug.LogError("     ❌ Canvas нужна камера!");
                }
            }
        }
    }

    private void CheckGridCells()
    {
        GridCell[] cells = FindObjectsOfType<GridCell>();
        
        Debug.Log($"📊 GridCell найдено: {cells.Length}");
        
        if (cells.Length > 0)
        {
            GridCell firstCell = cells[0];
            Debug.Log($"   Проверяем первую ячейку: {firstCell.gameObject.name}");
            
            Image img = firstCell.GetComponent<Image>();
            if (img == null)
            {
                Debug.LogError("   ❌ Нет Image компонента!");
            }
            else
            {
                Debug.Log($"   ✅ Image есть");
                Debug.Log($"      Raycast Target: {img.raycastTarget}");
                Debug.Log($"      Enabled: {img.enabled}");
                Debug.Log($"      Color: {img.color}");
            }

            RectTransform rect = firstCell.GetComponent<RectTransform>();
            Debug.Log($"   Position: {rect.position}");
            Debug.Log($"   Anchored Position: {rect.anchoredPosition}");
            Debug.Log($"   Size: {rect.sizeDelta}");
            
            Canvas canvas = firstCell.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("   ❌ GridCell НЕ ВНУТРИ Canvas!");
            }
            else
            {
                Debug.Log($"   ✅ Внутри Canvas: {canvas.gameObject.name}");
            }
        }
    }

    private void Start()
    {
        CheckEverything();
    }
}

