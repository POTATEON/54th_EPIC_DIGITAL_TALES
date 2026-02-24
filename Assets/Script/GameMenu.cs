using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameMenu : MonoBehaviour
{
    [Header("Buttons")]
    public Button menuButton;
    public Button backButton;
    public Button menuSceneButton;

    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject menuPanel;
    private GameObject currentPanel;

    private Stack<GameObject> panelHistory = new();

    private void Start()
    {
        InitializeButtons();
        ShowMainMenu();
    }

    void InitializeButtons()
    {
        menuButton.onClick.AddListener(OnMenuButtonClicked);
        backButton.onClick.AddListener(OnBackButtonClicked);
        menuSceneButton.onClick.AddListener(OnMenuSceneButtonClicked);
    }

    void ShowMainMenu()
    {
        ShowPanel(mainPanel);
    }

    void ShowPanel(GameObject panelToShow)
    {
        if (panelToShow == null)
        {
            Debug.LogWarning("ShowPanel called with null panel.");
            return;
        }

        // FIX: Don't push if this panel is already on top (prevents duplicate history entries)
        if (panelHistory.Count > 0 && panelHistory.Peek() == panelToShow)
        {
            Debug.Log($"Panel {panelToShow.name} is already on top of the stack, skipping push.");
            return;
        }

        // Hide all panels
        mainPanel.SetActive(false);
        menuPanel.SetActive(false);

        // Show the requested panel
        panelToShow.SetActive(true);
        currentPanel = panelToShow;
        panelHistory.Push(panelToShow);

        // FIX: Compare by reference, not by name
        menuButton.interactable = (panelToShow == mainPanel);

        Debug.Log($"Navigated to: {panelToShow.name} | Stack depth: {panelHistory.Count}");
    }

    void OnMenuButtonClicked()
    {
        Debug.Log("Menu button clicked");
        menuButton.interactable = false;
        ShowPanel(menuPanel);
    }

    void OnBackButtonClicked()
    {
        Debug.Log("Back button clicked");

        if (panelHistory.Count <= 1)
        {
            Debug.LogWarning("Already at root panel, cannot go back.");
            return;
        }

        try
        {
            // Pop the current panel and hide it
            GameObject poppedPanel = panelHistory.Pop();
            poppedPanel.SetActive(false);

            // Reveal the previous panel
            GameObject previousPanel = panelHistory.Peek();
            previousPanel.SetActive(true);
            currentPanel = previousPanel;

            // FIX: Compare by reference, not by name
            menuButton.interactable = (currentPanel == mainPanel);

            Debug.Log($"Went back to: {previousPanel.name} | Stack depth: {panelHistory.Count}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error navigating back: {ex.Message}");
        }
    }

    private void OnMenuSceneButtonClicked()
    {
        Debug.Log("MenuScene Button Clicked");
        SceneManager.LoadScene("MainMenu");
    }
}