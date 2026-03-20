using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Универсальная система навигации по панелям.
/// Используется в GameMenu и MenuNavigation, чтобы не дублировать логику стека.
/// </summary>
public class PanelNavigator
{
    private readonly Stack<GameObject> _history = new();
    private readonly GameObject[] _allPanels;

    public GameObject Current => _history.Count > 0 ? _history.Peek() : null;
    public bool CanGoBack => _history.Count > 1;

    public PanelNavigator(params GameObject[] panels)
    {
        _allPanels = panels;
    }

    /// <summary>Показать панель и добавить её в историю.</summary>
    public void ShowPanel(GameObject panel)
    {
        if (panel == null)
        {
            Debug.LogWarning("ShowPanel: передана null-панель.");
            return;
        }

        // Не добавляем в стек, если эта панель уже наверху
        if (_history.Count > 0 && _history.Peek() == panel)
        {
            Debug.Log($"Панель '{panel.name}' уже активна, пропуск.");
            return;
        }

        // Скрываем все панели
        foreach (var p in _allPanels)
            p.SetActive(false);

        panel.SetActive(true);
        _history.Push(panel);

        Debug.Log($"Переход к: {panel.name} | Глубина стека: {_history.Count}");
    }

    /// <summary>Вернуться к предыдущей панели. Возвращает панель, к которой вернулись.</summary>
    public GameObject GoBack()
    {
        if (!CanGoBack)
        {
            Debug.LogWarning("Уже на корневой панели, назад нельзя.");
            return null;
        }

        _history.Pop().SetActive(false);
        var previous = _history.Peek();
        previous.SetActive(true);

        Debug.Log($"Возврат к: {previous.name} | Глубина стека: {_history.Count}");
        return previous;
    }
}
