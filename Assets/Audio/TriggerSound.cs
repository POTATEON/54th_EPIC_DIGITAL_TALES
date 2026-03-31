using UnityEngine;

public class TriggerSound : MonoBehaviour
{
    public AudioClip triggerSound;     // Звук при активации
    public float volume = 1f;          // Громкость звука
    public bool playOnce = true;        // Воспроизвести только один раз
    public bool destroyAfterTrigger = false; // Удалить объект после активации

    private AudioSource audioSource;
    private bool hasTriggered = false;   // Был ли уже активирован

    void Start()
    {
        // Добавляем AudioSource если его нет
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Настраиваем AudioSource для триггера
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 1f;   // 3D звук (зависит от позиции)
        audioSource.volume = volume;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Проверяем, что это игрок (по тегу "Player")
        if (other.CompareTag("Player"))
        {
            // Проверяем можно ли воспроизвести
            if (!playOnce || !hasTriggered)
            {
                PlaySound();
                hasTriggered = true;

                // Если нужно удалить объект после триггера
                if (destroyAfterTrigger)
                {
                    Destroy(gameObject, triggerSound.length); // Удаляем после проигрывания
                }
            }
        }
    }

    void PlaySound()
    {
        if (triggerSound != null)
        {
            // Вариант 1: Через AudioSource на объекте
            audioSource.clip = triggerSound;
            audioSource.Play();

            // Вариант 2: Проиграть в точке (без AudioSource)
            // AudioSource.PlayClipAtPoint(triggerSound, transform.position, volume);
        }
        else
        {
            Debug.LogWarning("Звук не назначен в TriggerSound на объекте " + gameObject.name);
        }
    }
}