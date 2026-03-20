using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelIntroUI : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup canvasGroup;
    public TMP_Text titleText;
    public TMP_Text subtitleText;

    [Header("Настройки")]
    public float displayDuration = 2.5f;
    public float fadeInDuration = 0.4f;
    public float fadeOutDuration = 0.6f;

    [Header("Контент")]
    public string levelTitle = "Уровень: Логарифмы";
    public string levelSubtitle = "Добро пожаловать";

    [Header("Режим запуска")]
    public bool playOnStart = true; // снять галку — запускать только по триггеру

    private Coroutine _currentIntro;

    private void Start()
    {
        Hide();
        if (playOnStart)
            Show();
    }

    // Показать с текущими текстами
    public void Show()
    {
        titleText.text = levelTitle;
        subtitleText.text = levelSubtitle;
        PlayIntroInternal();
    }

    // Показать с кастомным текстом (удобно вызывать из триггера)
    public void Show(string title, string subtitle = "")
    {
        levelTitle = title;
        levelSubtitle = subtitle;
        Show();
    }

    public void Hide()
    {
        if (_currentIntro != null)
            StopCoroutine(_currentIntro);
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    private void PlayIntroInternal()
    {
        if (_currentIntro != null)
            StopCoroutine(_currentIntro);
        gameObject.SetActive(true);
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        _currentIntro = StartCoroutine(PlayIntro());
    }

    private IEnumerator PlayIntro()
    {
        yield return StartCoroutine(Fade(0f, 1f, fadeInDuration));
        yield return new WaitForSeconds(displayDuration);
        yield return StartCoroutine(Fade(1f, 0f, fadeOutDuration));
        gameObject.SetActive(false);
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}