using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuNavigation : MonoBehaviour
{
    [Header("Buttons")]
    public Button startButton;
    public Button settingsButton;
    public Button exitButton;
    public Button backButton;

    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject chaptersPanel;
    public GameObject settingsPanel;

    private Stack<GameObject> panelHistory = new();

    private void Start()
    {
        InitializeButtons();
        ShowMainMenu();
        UpdateBackButtonVisibility();
    }

    void InitializeButtons()
    {
        startButton.onClick.AddListener(OnStartButtonClicked);
        settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        exitButton.onClick.AddListener(OnExitButtonClicked);
        backButton.onClick.AddListener(OnBackButtonClicked);
    }

    void ShowPanel(GameObject panelToShow)
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        chaptersPanel.SetActive(false);

        if (panelToShow != null)
            
            panelToShow.SetActive(true);
            UpdateBackButtonVisibility();
            panelHistory.Push(panelToShow);
            int index = 0;
            foreach (GameObject panel in panelHistory)
            {
                Debug.Log($"[{index}] {panel.name}");
                index++;
            }
    }

    void ShowMainMenu()
    {
        ShowPanel(mainMenuPanel);
    }

    void OnStartButtonClicked()
    {
        Debug.Log("Start button clicked");
        ShowPanel(chaptersPanel);
    }

    void OnSettingsButtonClicked()
    {
        Debug.Log("Settings button clicked");
        ShowPanel(settingsPanel);

    }

    void OnBackButtonClicked()
    {
        Debug.Log("Back button clicked");

        // Проверяем, можно ли вообще вернуться
        if (panelHistory.Count <= 1)
        {
            Debug.LogWarning("Нельзя вернуться - уже в корневом меню");
            return;
        }

        try
        {
            // 1. Скрываем текущую панель (убираем из стека)
            GameObject currentPanel = panelHistory.Pop();

            // 2. Показываем предыдущую панель (не убираем её!)
            GameObject previousPanel = panelHistory.Peek(); // Peek, не Pop!
            ShowPanel(previousPanel);

            Debug.Log($"Вернулись с {currentPanel.name} на {previousPanel.name}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Ошибка при возврате: {ex.Message}");
            // Можно восстановить состояние
            ShowMainMenu();
        }
    }

    private void UpdateBackButtonVisibility()
    {
        if (backButton == null) return;

        // Проверяем, главное ли сейчас меню
        bool isMainMenu = false;

        if (panelHistory.Count > 0)
        {
            GameObject currentPanel = panelHistory.Peek(); // ВЕРХНИЙ элемент!
            if (currentPanel != null)
            {
                // Сравниваем текущую (верхнюю) панель
                isMainMenu = (currentPanel.name == "MainPanel");
                Debug.Log($"Текущая панель: {currentPanel.name}, Главное меню: {isMainMenu}");
            }
        }
        else
        {
            // Если стек пустой - показываем главное меню по умолчанию
            isMainMenu = true;
        }

        // Скрываем кнопку на главном меню
        backButton.gameObject.SetActive(!isMainMenu);

        Debug.Log($"Кнопка назад: {(isMainMenu ? "скрыта" : "видна")}");
    }
    void OnExitButtonClicked()
    {
        Debug.Log("Exit button clicked");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }
}





