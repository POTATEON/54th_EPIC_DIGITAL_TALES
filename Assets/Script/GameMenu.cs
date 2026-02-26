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

    private PanelNavigator _navigator;

    private void Start()
    {
        _navigator = new PanelNavigator(mainPanel, menuPanel);

        menuButton.onClick.AddListener(OnMenuButtonClicked);
        backButton.onClick.AddListener(OnBackButtonClicked);
        menuSceneButton.onClick.AddListener(OnMenuSceneButtonClicked);

        _navigator.ShowPanel(mainPanel);
        UpdateMenuButton();
    }

    void OnMenuButtonClicked()
    {
        Debug.Log("Кнопка Menu нажата");
        _navigator.ShowPanel(menuPanel);
        UpdateMenuButton();
    }

    void OnBackButtonClicked()
    {
        Debug.Log("Кнопка Back нажата");
        _navigator.GoBack();
        UpdateMenuButton();
    }

    void OnMenuSceneButtonClicked()
    {
        Debug.Log("Кнопка MenuScene нажата");
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Кнопка Menu активна только когда мы НЕ на главной панели.
    /// FIX: было (panelToShow == mainPanel) — логика была перевёрнута.
    /// </summary>
    void UpdateMenuButton()
    {
        menuButton.interactable = (_navigator.Current != menuPanel);
        menuButton.enabled = menuButton.interactable;
        Debug.Log($"{menuButton.didAwake} {menuButton.didStart}");
    }
}
