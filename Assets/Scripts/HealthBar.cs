using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HP бар над персонажем
/// Автоматически обновляется при изменении здоровья
/// </summary>
public class HealthBar : MonoBehaviour
{
    [Header("Ссылки")]
    [Tooltip("Заполняемая часть HP бара (Fill Image)")]
    [SerializeField] private Image fillImage;
    
    [Tooltip("Фоновое изображение HP бара (опционально)")]
    [SerializeField] private Image backgroundImage;
    
    [Tooltip("Текст с числами HP (опционально)")]
    [SerializeField] private TMPro.TextMeshProUGUI hpText;
    
    [Header("Настройки")]
    [Tooltip("Цвет при полном HP")]
    [SerializeField] private Color fullHealthColor = Color.green;
    
    [Tooltip("Цвет при среднем HP")]
    [SerializeField] private Color midHealthColor = Color.yellow;
    
    [Tooltip("Цвет при низком HP")]
    [SerializeField] private Color lowHealthColor = Color.red;
    
    [Tooltip("Порог низкого HP (0-1)")]
    [SerializeField] private float lowHealthThreshold = 0.3f;
    
    [Tooltip("Скрыть HP бар когда HP = 100%")]
    [SerializeField] private bool hideWhenFull = false;
    
    [Tooltip("Плавное изменение HP бара")]
    [SerializeField] private bool smoothTransition = true;
    
    [Tooltip("Скорость плавного изменения")]
    [SerializeField] private float transitionSpeed = 5f;
    
    private float currentFillAmount = 1f;
    private float targetFillAmount = 1f;
    private int currentHP;
    private int maxHP;

    private void Awake()
    {
        if (fillImage == null)
        {
            fillImage = GetComponentInChildren<Image>();
        }
    }

    private void Update()
    {
        if (smoothTransition && Mathf.Abs(currentFillAmount - targetFillAmount) > 0.001f)
        {
            currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, Time.deltaTime * transitionSpeed);
            UpdateVisuals();
        }
    }

    /// <summary>
    /// Установить HP (от 0 до maxHP)
    /// </summary>
    public void SetHealth(int current, int max)
    {
        currentHP = Mathf.Clamp(current, 0, max);
        maxHP = max;
        
        targetFillAmount = maxHP > 0 ? (float)currentHP / maxHP : 0f;
        
        if (!smoothTransition)
        {
            currentFillAmount = targetFillAmount;
        }
        
        UpdateVisuals();
    }

    /// <summary>
    /// Установить HP в процентах (0-1)
    /// </summary>
    public void SetHealthPercent(float percent)
    {
        targetFillAmount = Mathf.Clamp01(percent);
        
        if (!smoothTransition)
        {
            currentFillAmount = targetFillAmount;
        }
        
        UpdateVisuals();
    }

    /// <summary>
    /// Обновить визуальное отображение
    /// </summary>
    private void UpdateVisuals()
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = currentFillAmount;
            fillImage.color = GetHealthColor(currentFillAmount);
        }

        // Обновляем текст HP
        if (hpText != null)
        {
            hpText.text = $"{currentHP}/{maxHP}";
        }

        // Скрываем/показываем HP бар
        if (hideWhenFull)
        {
            gameObject.SetActive(currentFillAmount < 0.99f);
        }
    }

    /// <summary>
    /// Получить цвет в зависимости от уровня HP
    /// </summary>
    private Color GetHealthColor(float healthPercent)
    {
        if (healthPercent <= lowHealthThreshold)
        {
            return lowHealthColor;
        }
        else if (healthPercent <= 0.6f)
        {
            // Плавный переход от среднего к низкому
            float t = (healthPercent - lowHealthThreshold) / (0.6f - lowHealthThreshold);
            return Color.Lerp(lowHealthColor, midHealthColor, t);
        }
        else
        {
            // Плавный переход от полного к среднему
            float t = (healthPercent - 0.6f) / 0.4f;
            return Color.Lerp(midHealthColor, fullHealthColor, t);
        }
    }

    /// <summary>
    /// Показать HP бар
    /// </summary>
    public void Show()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Скрыть HP бар
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}

