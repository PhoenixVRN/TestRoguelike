using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Максимально простой тест - просто показывает ЧТО получает клик
/// Добавьте на ЛЮБОЙ UI объект для проверки
/// </summary>
public class SimpleClickTest : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"🟢 МЫШЬ НАВЕДЕНА на {gameObject.name}");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log($"🔴 МЫШЬ УШЛА с {gameObject.name}");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"🎯🎯🎯 КЛИК на {gameObject.name}! Кнопка: {eventData.button}");
    }
}

