using UnityEngine;

/// <summary>
/// Описывает одну кнопку-способность (маску ввода) для калькулятора.
///
/// Создать: ПКМ → Create → Battle → AbilityTemplate
///
/// Поле template — строка с символами-заглушками «_».
/// Каждый «_» — это слот, в который игрок вводит значение с клавиатуры.
///
/// Примеры:
///   "log_(_*_)"   →  log₂(8*4)   — сложение логарифмов
///   "log_(_)+log_(_)"  →  отдельные слоты для каждого основания и аргумента
///   "_^_"         →  степень
///   "_/_"         →  деление
///
/// label — текст на кнопке в UI (например "log сложение").
/// displayTemplate — как шаблон выглядит на дисплее (с TMP-тегами sub/sup).
///   Если пусто — генерируется автоматически из template.
/// </summary>
[CreateAssetMenu(fileName = "NewAbilityTemplate", menuName = "Battle/AbilityTemplate")]
public class AbilityTemplate : ScriptableObject
{
    [Header("Кнопка")]
    [Tooltip("Текст на кнопке способности")]
    public string label = "Способность";

    [Header("Шаблон")]
    [Tooltip("Строка с символом _ как слотом. Пример: log_(_*_)")]
    public string template = "_";

    [Tooltip("Как отображать шаблон на дисплее (TMP Rich Text). Пусто = авто из template).\n" +
             "Пример для log_(_*_): log<sub>□</sub>(□*□)")]
    public string displayTemplate = "";
}