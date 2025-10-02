using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Простой тест кликов - добавьте на GridCell для проверки
/// </summary>
public class ClickTest : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"🎯 КЛИК ЗАРЕГИСТРИРОВАН на {gameObject.name}! Кнопка: {eventData.button}");
    }
}

