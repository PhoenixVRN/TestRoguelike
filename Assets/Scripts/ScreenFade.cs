using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

/// <summary>
/// Управление затемнением экрана через DOTween
/// Fade in/out для переходов между волнами
/// </summary>
public class ScreenFade : MonoBehaviour
{
    [Header("Ссылки")]
    [Tooltip("Image для затемнения (чёрная панель на весь экран)")]
    [SerializeField] private Image fadeImage;
    
    [Header("Настройки")]
    [Tooltip("Длительность затемнения (fade to black)")]
    [SerializeField] private float fadeInDuration = 0.5f;
    
    [Tooltip("Длительность развеивания (fade from black)")]
    [SerializeField] private float fadeOutDuration = 0.5f;
    
    [Tooltip("Цвет затемнения")]
    [SerializeField] private Color fadeColor = Color.black;
    
    [Tooltip("Тип анимации затемнения")]
    [SerializeField] private Ease fadeInEase = Ease.InQuad;
    
    [Tooltip("Тип анимации развеивания")]
    [SerializeField] private Ease fadeOutEase = Ease.OutQuad;
    
    [Tooltip("Показывать отладочную информацию")]
    [SerializeField] private bool showDebug = false;

    private bool isFading = false;

    private void Awake()
    {
        if (fadeImage == null)
        {
            fadeImage = GetComponent<Image>();
        }

        if (fadeImage != null)
        {
            // Начинаем с прозрачного экрана
            Color c = fadeColor;
            c.a = 0f;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Затемнить экран (fade to black)
    /// </summary>
    public void FadeIn(System.Action onComplete = null)
    {
        if (fadeImage == null)
        {
            Debug.LogError("Fade Image не назначен!");
            onComplete?.Invoke();
            return;
        }

        if (isFading)
        {
            DOTween.Kill(fadeImage);
        }

        isFading = true;

        if (showDebug)
        {
            Debug.Log($"🌑 Затемнение экрана... ({fadeInDuration}с)");
        }

        fadeImage.gameObject.SetActive(true);
        
        Color targetColor = fadeColor;
        targetColor.a = 1f;

        fadeImage.DOColor(targetColor, fadeInDuration)
            .SetEase(fadeInEase)
            .OnComplete(() =>
            {
                isFading = false;
                
                if (showDebug)
                {
                    Debug.Log("🌑 Экран полностью затемнён!");
                }
                
                onComplete?.Invoke();
            });
    }

    /// <summary>
    /// Развеять затемнение (fade from black)
    /// </summary>
    public void FadeOut(System.Action onComplete = null)
    {
        if (fadeImage == null)
        {
            Debug.LogError("Fade Image не назначен!");
            onComplete?.Invoke();
            return;
        }

        if (isFading)
        {
            DOTween.Kill(fadeImage);
        }

        isFading = true;

        if (showDebug)
        {
            Debug.Log($"☀️ Развеиваем затемнение... ({fadeOutDuration}с)");
        }

        Color targetColor = fadeColor;
        targetColor.a = 0f;

        fadeImage.DOColor(targetColor, fadeOutDuration)
            .SetEase(fadeOutEase)
            .OnComplete(() =>
            {
                isFading = false;
                fadeImage.gameObject.SetActive(false);
                
                if (showDebug)
                {
                    Debug.Log("☀️ Затемнение развеяно!");
                }
                
                onComplete?.Invoke();
            });
    }

    /// <summary>
    /// Fade in → выполнить действие → Fade out
    /// </summary>
    public void FadeInOutWithAction(System.Action action)
    {
        StartCoroutine(FadeInOutCoroutine(action));
    }

    /// <summary>
    /// Корутина: Затемнение → Действие → Развеивание
    /// </summary>
    private IEnumerator FadeInOutCoroutine(System.Action action)
    {
        // Затемняем
        bool fadeInComplete = false;
        FadeIn(() => fadeInComplete = true);
        
        while (!fadeInComplete)
            yield return null;

        // Выполняем действие (например, рестарт)
        action?.Invoke();

        // Небольшая пауза
        yield return new WaitForSeconds(0.1f);

        // Развеиваем
        FadeOut();
    }

    /// <summary>
    /// Мгновенно затемнить
    /// </summary>
    public void SetBlack()
    {
        if (fadeImage != null)
        {
            Color c = fadeColor;
            c.a = 1f;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Мгновенно развеять
    /// </summary>
    public void SetClear()
    {
        if (fadeImage != null)
        {
            Color c = fadeColor;
            c.a = 0f;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Проверить идёт ли fade
    /// </summary>
    public bool IsFading()
    {
        return isFading;
    }

    // ═══════════════════════════════════════════════════════════
    // ТЕСТОВЫЕ МЕТОДЫ
    // ═══════════════════════════════════════════════════════════

    [ContextMenu("🌑 ТЕСТ: Затемнить экран")]
    private void TestFadeIn()
    {
        FadeIn(() => Debug.Log("🌑 Затемнение завершено!"));
    }

    [ContextMenu("☀️ ТЕСТ: Развеять затемнение")]
    private void TestFadeOut()
    {
        FadeOut(() => Debug.Log("☀️ Развеивание завершено!"));
    }

    [ContextMenu("🔄 ТЕСТ: Fade In → Out")]
    private void TestFadeInOut()
    {
        FadeInOutWithAction(() => Debug.Log("🔄 Действие во время затемнения!"));
    }
}

