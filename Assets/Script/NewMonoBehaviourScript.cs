// UpdatedInputCreator.cs - РАБОЧАЯ ВЕРСИЯ
#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;
using System.IO;

public class UpdatedInputCreator : MonoBehaviour
{
    [MenuItem("Tools/Create Player Input (Fixed)")]
    static void CreatePlayerInputFixed()
    {
        // Создаем папку если нет
        string folderPath = "Assets/Input";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            AssetDatabase.Refresh();
        }

        // Создаем правильный JSON для Input Actions
        string jsonContent = @"{
    ""m_Name"": ""PlayerControls"",
    ""maps"": [
        {
            ""m_Name"": ""Player"",
            ""actions"": [
                {
                    ""m_Name"": ""Move"",
                    ""type"": 1,
                    ""bindings"": [
                        {
                            ""path"": ""<Keyboard>/w"",
                            ""name"": ""up"",
                            ""id"": ""c4c55f26-37b1-4e5a-9c5a-4f5c6d7e8f9a"",
                            ""isComposite"": false,
                            ""isPartOfComposite"": true
                        },
                        {
                            ""path"": ""<Keyboard>/s"",
                            ""name"": ""down"",
                            ""id"": ""d5d66f37-48c2-5f6b-8d6b-5g6d7e8f9a0b"",
                            ""isComposite"": false,
                            ""isPartOfComposite"": true
                        },
                        {
                            ""path"": ""<Keyboard>/a"",
                            ""name"": ""left"",
                            ""id"": ""e6e77f48-59d3-6f7c-9e7c-6h7e8f9a0b1c"",
                            ""isComposite"": false,
                            ""isPartOfComposite"": true
                        },
                        {
                            ""path"": ""<Keyboard>/d"",
                            ""name"": ""right"",
                            ""id"": ""f7f88f59-6ae4-7f8d-0f8d-7i8f9a0b1c2d"",
                            ""isComposite"": false,
                            ""isPartOfComposite"": true
                        },
                        {
                            ""path"": ""2DVector"",
                            ""name"": ""2D Vector"",
                            ""id"": ""a1a22b13-25c0-1e2b-3b2b-1c2d3e4f5a6b"",
                            ""isComposite"": true,
                            ""isPartOfComposite"": false
                        }
                    ]
                },
                {
                    ""m_Name"": ""Jump"",
                    ""type"": 0,
                    ""bindings"": [
                        {
                            ""path"": ""<Keyboard>/space"",
                            ""interactions"": """",
                            ""processors"": """",
                            ""id"": ""b2b33c24-36d1-2f3c-4c3c-2d3e4f5a6b7c""
                        }
                    ]
                },
                {
                    ""m_Name"": ""Run"",
                    ""type"": 0,
                    ""bindings"": [
                        {
                            ""path"": ""<Keyboard>/leftShift"",
                            ""interactions"": """",
                            ""processors"": """",
                            ""id"": ""c3c44d35-47e2-3g4d-5d4d-3e4f5a6b7c8d""
                        }
                    ]
                }
            ]
        }
    ],
    ""controlSchemes"": []
}";

        // Сохраняем как текстовый файл с правильным расширением
        string filePath = folderPath + "/PlayerControls.inputactions";
        File.WriteAllText(filePath, jsonContent);

        // Импортируем ассет
        AssetDatabase.ImportAsset(filePath);

        // Настраиваем ассет
        InputActionAsset asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(filePath);
        if (asset != null)
        {
            // Включаем генерацию C# класса
            SerializedObject serializedObject = new SerializedObject(asset);
            serializedObject.FindProperty("m_GenerateCSFile").boolValue = true;
            serializedObject.ApplyModifiedProperties();

            Debug.Log("Input Actions успешно созданы! Файл: " + filePath);

            // Выделяем созданный файл в Project окне
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
        else
        {
            Debug.LogError("Не удалось создать Input Actions!");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
#endif