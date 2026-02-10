// SmoothCameraFollow.cs - ЭТО ПРАВИЛЬНАЯ ПЛАВНОСТЬ
using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    public Transform target;      // Перетащи сюда игрока
    public float smoothSpeed = 5f; // Чем больше - тем плавнее (5-10)
    public Vector3 offset = new Vector3(0, 0, -10);

    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        // Целевая позиция камеры
        Vector3 targetPosition = target.position + offset;

        // ПЛАВНОЕ движение с SmoothDamp
        transform.position = Vector3.SmoothDamp(
            transform.position,    // Текущая позиция
            targetPosition,        // Целевая позиция  
            ref velocity,          // Скорость (изменяется автоматически)
            1f / smoothSpeed       // Время достижения цели
        );
    }
}