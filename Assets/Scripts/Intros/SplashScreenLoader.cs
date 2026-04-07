using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class SplashScreenLoader : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "MainMenu";
    [SerializeField] private float autoLoadDelay = 3.5f;

    private bool _loaded;
    private PlayerControls _controls;

    private void Start()
    {
        _controls = new PlayerControls();
        _controls.Enable();

        // Подписываемся на кнопку Interact (E или Enter)
        _controls.Player.Interact.performed += OnSkipPressed;

        Invoke(nameof(LoadNextScene), autoLoadDelay);
    }

    private void OnSkipPressed(InputAction.CallbackContext context)
    {
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        if (_loaded) return;
        _loaded = true;
        SceneManager.LoadScene(nextSceneName);
    }

    private void OnDestroy()
    {
        if (_controls != null)
        {
            _controls.Player.Interact.performed -= OnSkipPressed;
            _controls.Disable();
            _controls.Dispose();
        }
    }
}