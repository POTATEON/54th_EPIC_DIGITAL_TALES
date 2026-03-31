using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Header("Слои фона (от дальнего к ближнему)")]
    [SerializeField] public Transform[] backgroundLayers;
    [SerializeField] public float[] parallaxSpeeds;
    [SerializeField] private float smoothing = 2f;

    [Header("Ограничение движения")]
    [SerializeField] private bool useBounds = false;
    [SerializeField] private float minX = -20f;
    [SerializeField] private float maxX = 20f;

    private Transform _camera;
    private Vector3 _previousCameraPosition;
    private Vector3[] _layerStartPositions;

    private void Start()
    {
        _camera = Camera.main.transform;
        _previousCameraPosition = _camera.position;

        // Сохраняем начальные позиции слоёв
        _layerStartPositions = new Vector3[backgroundLayers.Length];
        for (int i = 0; i < backgroundLayers.Length; i++)
        {
            _layerStartPositions[i] = backgroundLayers[i].position;
        }
    }

    private void LateUpdate()
    {
        Vector3 delta = _camera.position - _previousCameraPosition;

        for (int i = 0; i < backgroundLayers.Length; i++)
        {
            if (backgroundLayers[i] == null) continue;

            float speed = parallaxSpeeds.Length > i ? parallaxSpeeds[i] : 1f;
            Vector3 targetPos = backgroundLayers[i].position + delta * speed;

            // Плавное движение
            backgroundLayers[i].position = Vector3.Lerp(
                backgroundLayers[i].position,
                targetPos,
                smoothing * Time.deltaTime
            );

            // Ограничение если нужно
            if (useBounds)
            {
                Vector3 clamped = backgroundLayers[i].position;
                clamped.x = Mathf.Clamp(clamped.x, minX, maxX);
                backgroundLayers[i].position = clamped;
            }
        }

        _previousCameraPosition = _camera.position;
    }

    // Сброс всех слоёв в начальные позиции
    public void ResetPositions()
    {
        for (int i = 0; i < backgroundLayers.Length; i++)
        {
            if (backgroundLayers[i] != null)
                backgroundLayers[i].position = _layerStartPositions[i];
        }
    }
}