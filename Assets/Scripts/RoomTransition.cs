using UnityEngine;
using UnityEngine.InputSystem;

public class RoomTransition : MonoBehaviour
{
    [Header("¬изуал")]
    [SerializeField] private GameObject roomSprite;      // спрайт комнаты
    [SerializeField] private GameObject blackBackground; // чЄрный фон (SpriteRenderer)

    [Header("ѕодсказка")]
    [SerializeField] private GameObject interactHint;

    private bool _isInside = false;
    private bool _playerInRange = false;
    private PlayerControls _controls;

    private void Awake()
    {
        _controls = new PlayerControls();

        // —крываем комнату и чЄрный фон
        if (roomSprite != null) roomSprite.SetActive(false);
        if (blackBackground != null) blackBackground.SetActive(false);
    }

    private void OnEnable() => _controls.Player.Enable();
    private void OnDisable() => _controls.Player.Disable();

    private void Update()
    {
        if (_playerInRange && _controls.Player.Interact.WasPressedThisFrame())
        {
            ToggleRoom();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInRange = true;
        if (interactHint != null) interactHint.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInRange = false;
        if (interactHint != null) interactHint.SetActive(false);
    }

    private void ToggleRoom()
    {
        _isInside = !_isInside;

        if (_isInside)
        {
            // ¬ходим Ч показываем чЄрный фон и спрайт комнаты
            if (blackBackground != null) blackBackground.SetActive(true);
            if (roomSprite != null) roomSprite.SetActive(true);
        }
        else
        {
            // ¬ыходим Ч убираем
            if (blackBackground != null) blackBackground.SetActive(false);
            if (roomSprite != null) roomSprite.SetActive(false);
        }
    }
}