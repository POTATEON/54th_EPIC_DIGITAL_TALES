using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuNavigation : MonoBehaviour
{
    [Header("Buttons")]
    public Button startButton;
    public Button settingsButton;
    public Button exitButton;
    public Button backButton;
    public Button logButton;

    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject chaptersPanel;
    public GameObject settingsPanel;

    private PanelNavigator _navigator;

    private void Start()
    {
        _navigator = new PanelNavigator(mainMenuPanel, chaptersPanel, settingsPanel);

        startButton.onClick.AddListener(OnStartButtonClicked);
        settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        exitButton.onClick.AddListener(OnExitButtonClicked);
        backButton.onClick.AddListener(OnBackButtonClicked);
        logButton.onClick.AddListener(OnLogButtonClicked);

        _navigator.ShowPanel(mainMenuPanel);
        UpdateBackButton();
    }

    void OnStartButtonClicked()
    {
        Debug.Log("Start нажата");
        _navigator.ShowPanel(chaptersPanel);
        UpdateBackButton();
    }

    void OnSettingsButtonClicked()
    {
        Debug.Log("Settings нажата");
        _navigator.ShowPanel(settingsPanel);
        UpdateBackButton();
    }

    void OnBackButtonClicked()
    {
        Debug.Log("Back нажата");
        _navigator.GoBack();
        UpdateBackButton();
    }

    void OnLogButtonClicked()
    {
        Debug.Log("Log нажата");
        SceneManager.LoadScene("Gameplay");
    }

    void OnExitButtonClicked()
    {
        Debug.Log("Exit нажата");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void UpdateBackButton()
    {
        if (backButton == null) return;
        // Скрываем кнопку Back на главном меню
        backButton.gameObject.SetActive(_navigator.Current != mainMenuPanel);
    }
}
