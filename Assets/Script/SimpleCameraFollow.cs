using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    public Transform target;
    public float followSmoothSpeed = 5f; // FIX: переименовано с smoothSpeed во избежание путаницы с полем в PlayerController
    public Vector3 offset = new Vector3(0, 0, -10);

    // Опционально: ограничение камеры по границам уровня
    [Header("Camera Bounds (optional)")]
    public bool useBounds = false;
    public float minX, maxX, minY, maxY;

    private Vector3 _velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = target.position + offset;

        // Зажимаем позицию камеры в пределах карты (если включено)
        if (useBounds)
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);
        }

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref _velocity,
            1f / followSmoothSpeed
        );
    }
}
