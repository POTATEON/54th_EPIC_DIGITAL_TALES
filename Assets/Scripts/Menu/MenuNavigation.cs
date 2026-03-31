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

    [Header("Кнопки уровней (4 сцены)")]
    public Button scene1Button;  // Основы и свойства
    public Button scene2Button;  // Уравнения
    public Button scene3Button;  // Неравенства
    public Button scene4Button;  // Применение в науках

    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject chaptersPanel;
    public GameObject settingsPanel;

    [Header("Названия сцен")]
    public string scene1Name = "Scene_01_Basics";
    public string scene2Name = "Scene_02_Equations";
    public string scene3Name = "Scene_03_Inequalities";
    public string scene4Name = "Scene_04_Applications";

    private PanelNavigator _navigator;

    private void Start()
    {
        _navigator = new PanelNavigator(mainMenuPanel, chaptersPanel, settingsPanel);

        startButton.onClick.AddListener(OnStartButtonClicked);
        settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        exitButton.onClick.AddListener(OnExitButtonClicked);
        backButton.onClick.AddListener(OnBackButtonClicked);

        // Кнопки сцен
        if (scene1Button != null)
            scene1Button.onClick.AddListener(() => LoadScene(scene1Name));
        if (scene2Button != null)
            scene2Button.onClick.AddListener(() => LoadScene(scene2Name));
        if (scene3Button != null)
            scene3Button.onClick.AddListener(() => LoadScene(scene3Name));
        if (scene4Button != null)
            scene4Button.onClick.AddListener(() => LoadScene(scene4Name));

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

    void OnExitButtonClicked()
    {
        Debug.Log("Exit нажата");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void LoadScene(string sceneName)
    {
        Debug.Log($"Загрузка сцены: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }

    void UpdateBackButton()
    {
        if (backButton == null) return;
        backButton.gameObject.SetActive(_navigator.Current != mainMenuPanel);
    }
}