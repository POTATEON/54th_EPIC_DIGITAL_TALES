using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [Header("Скорость (0 = не движется, 1 = как камера)")]
    [SerializeField] private float speed = 0.5f;

    private Camera _mainCamera;
    private Vector2 _lastCamPos;
    private Vector2 _startPos;

    private void Start()
    {
        _mainCamera = Camera.main;

        if (_mainCamera == null)
        {
            Debug.LogError($"[ParallaxLayer] Камера не найдена на {gameObject.name}!");
            enabled = false;
            return;
        }

        _startPos = transform.position;
        _lastCamPos = _mainCamera.transform.position;
    }

    private void LateUpdate()
    {
        if (_mainCamera == null) return;

        Vector2 currentCamPos = _mainCamera.transform.position;
        Vector2 delta = currentCamPos - _lastCamPos;

        transform.position = (Vector2)transform.position + delta * speed;

        _lastCamPos = currentCamPos;
    }

    [ContextMenu("Сбросить позицию")]
    public void ResetPosition()
    {
        transform.position = _startPos;
    }
}