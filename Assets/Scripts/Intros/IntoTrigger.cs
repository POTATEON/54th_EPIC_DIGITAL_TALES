using UnityEngine;

public class IntroTrigger : MonoBehaviour
{
    public LevelIntroUI introUI;

    [Header("Текст для этого триггера")]
    public string title = "Уровень: Производные";
    public string subtitle = "Глава 2";

    [Header("Срабатывает только один раз")]
    public bool oneShot = true;

    private bool _triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (oneShot && _triggered) return;

        _triggered = true;
        introUI.Show(title, subtitle);
    }
}