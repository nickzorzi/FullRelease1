using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class SoundManagerEditorWindow : ExtendedEditorWindow
{
    private string selectedPropertyPath, selectedSecondaryPath;
    protected SerializedProperty selectedProperty, secondarySelectedProperty, oldProperty;
    
    
    public static void Open(SoundDataStorage dataObject)
    {
        SoundManagerEditorWindow window = GetWindow<SoundManagerEditorWindow>("Sound Data Editor");
        window.serializedObject = new SerializedObject(dataObject);
    }

    private void OnGUI()
    {
        if(serializedObject == null) 
        {
            EditorGUILayout.LabelField("No Sound Dictionary Selected");
            return; 
        }
        currentProperty = serializedObject.FindProperty("AudioList");

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Save & Generate Enums"))
        {
            GenerateEnums();
        }
        EditorGUILayout.EndHorizontal();


        EditorGUILayout.BeginHorizontal();

        // This Draws The array of Buttons for the first List
        EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(150),GUILayout.ExpandHeight(true));
        DrawSideBar(currentProperty, ref selectedPropertyPath,ref selectedProperty, out bool updatedPath);
        if (updatedPath)
        {
            secondarySelectedProperty = null;
            selectedSecondaryPath = null;
        }
        if (GUILayout.Button("Add New Category"))
        {
            currentProperty.arraySize++;
            SerializedProperty newElement = currentProperty.GetArrayElementAtIndex(currentProperty.arraySize - 1);
            newElement.FindPropertyRelative("categoryName").stringValue = "Blank Category";
            newElement.FindPropertyRelative("sounds").ClearArray();
        }

        EditorGUILayout.EndVertical();



        EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));
        if (selectedProperty != null)
        {
            DrawSelectedPropertiesPanel();
        }
        else
        {
            EditorGUILayout.LabelField("Select a Category from the list");
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
        Apply();
    }


    void DrawSelectedPropertiesPanel()
    {
        
        if(secondarySelectedProperty != null) 
        {
            bool isRelative = secondarySelectedProperty.propertyPath.StartsWith(selectedProperty.propertyPath);
            if(!isRelative)
            {
               
                selectedSecondaryPath = null;
            }
        }

        currentProperty = selectedProperty;
        EditorGUILayout.BeginHorizontal("box");
        DrawField("categoryName",true);
        if(GUILayout.Button("Delete Category", GUILayout.MaxWidth(150)))
        {
            RemoveSelectedItem(false);
            EditorGUILayout.EndHorizontal();
            return;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        currentProperty = currentProperty.FindPropertyRelative("sounds");
/*        if (currentProperty.FindPropertyRelative(selectedSecondaryPath) != secondarySelectedProperty)
        {

        }*/
        // This Draws the Next array of Buttons inside each list Item
        EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(175), GUILayout.ExpandHeight(true));
        DrawSideBar(currentProperty, ref selectedSecondaryPath, ref secondarySelectedProperty, out bool trash);

        if (GUILayout.Button("Add New Sound List"))
        {
            currentProperty.arraySize++;
            SerializedProperty newElement = currentProperty.GetArrayElementAtIndex(currentProperty.arraySize - 1);
            newElement.FindPropertyRelative("name").stringValue = "Blank Audio List";
            newElement.FindPropertyRelative("audioClips").ClearArray();
        }
        EditorGUILayout.EndVertical();


        // This Draws the Rest of the Data
        EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));
        if (secondarySelectedProperty != null)
        {
            EditorGUILayout.BeginVertical();
            currentProperty = secondarySelectedProperty;
            EditorGUILayout.BeginHorizontal();
            DrawField("name",true);
            if (GUILayout.Button("Delete Sound List", GUILayout.MaxWidth(150)))
            {
                RemoveSelectedItem(true);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                return;
            }
            EditorGUILayout.EndHorizontal();
            DrawField("audioClips", true);
            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.LabelField("Select an item from the list");
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }

    void RemoveSelectedItem(bool secondary)
    {
        if (secondarySelectedProperty != null && secondary)
        {
            // Remove item from the "sounds" list inside the selected category
            SerializedProperty soundsList = selectedProperty.FindPropertyRelative("sounds");
            RemoveFromList(soundsList, secondarySelectedProperty);

            // Clear selection after removal
            secondarySelectedProperty = null;
            selectedSecondaryPath = null;
        }
        else if (selectedProperty != null && !secondary)
        {
            // Remove the whole category from "AudioList"
            SerializedProperty categoryList = serializedObject.FindProperty("AudioList");
            RemoveFromList(categoryList, selectedProperty);

            // Clear selection after removal
            selectedProperty = null;
            selectedPropertyPath = null;
            secondarySelectedProperty = null;
            selectedSecondaryPath = null;
        }
        else
        {
            Debug.LogWarning("No item selected for removal!");
        }

        serializedObject.ApplyModifiedProperties();
    }

    void RemoveFromList(SerializedProperty list, SerializedProperty item)
    {
        int indexToRemove = -1;

        for (int i = 0; i < list.arraySize; i++)
        {
            if (list.GetArrayElementAtIndex(i).propertyPath == item.propertyPath)
            {
                indexToRemove = i;
                break;
            }
        }

        if (indexToRemove >= 0)
        {
            list.DeleteArrayElementAtIndex(indexToRemove);
        }
        else
        {
            Debug.LogWarning("Item not found in the list!");
        }
    }


    string CleanName(string input)
    {
        if (string.IsNullOrEmpty(input))
            return "Unnamed";

        input = input.Replace(" ", "_");

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        foreach (char c in input)
        {
            if (char.IsLetterOrDigit(c) || c == '_')
                sb.Append(c);
        }

        string result = sb.ToString();

        if (string.IsNullOrEmpty(result))
            result = "Unnamed";

        if (char.IsDigit(result[0]))
            result = "_" + result;

        return result;
    }


    void GenerateEnums()
    {
        serializedObject.ApplyModifiedProperties();

        SoundDataStorage storage = (SoundDataStorage)serializedObject.targetObject;

        if (storage == null)
        {
            Debug.LogError("No SoundDataStorage assigned.");
            return;
        }

        string assetName = CleanName(storage.name);
        string folderPath = "Assets/_Audio";

        if (!AssetDatabase.IsValidFolder(folderPath))
            AssetDatabase.CreateFolder("Assets", "_Audio");

        string filePath = $"{folderPath}/{assetName}Enums.cs";

        // STEP 1: Read existing enum values (if file exists)
        Dictionary<string, int> existingValues = new Dictionary<string, int>();
        int highestValue = -1;

        if (System.IO.File.Exists(filePath))
        {
            string[] lines = System.IO.File.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                string trimmed = line.Trim();

                if (trimmed.Contains("="))
                {
                    // Example: Player__Walk = 3,
                    string[] parts = trimmed.Split('=');
                    if (parts.Length < 2) continue;

                    string name = parts[0].Trim().Trim(',');
                    string numberPart = parts[1].Trim().Trim(',');

                    if (int.TryParse(numberPart, out int value))
                    {
                        if (!existingValues.ContainsKey(name))
                            existingValues.Add(name, value);

                        if (value > highestValue)
                            highestValue = value;
                    }
                }
            }
        }

        int nextValue = highestValue + 1;

        System.Text.StringBuilder builder = new System.Text.StringBuilder();

        builder.AppendLine("// AUTO-GENERATED. DO NOT EDIT.");
        builder.AppendLine("using UnityEngine;");
        builder.AppendLine();
        builder.AppendLine("public static class SoundSystem");
        builder.AppendLine("{");
        builder.AppendLine($"    public enum {assetName}");
        builder.AppendLine("    {");

        HashSet<string> usedNames = new HashSet<string>();

        foreach (var category in storage.AudioList)
        {
            if (string.IsNullOrEmpty(category.categoryName))
                continue;

            string categoryName = CleanName(category.categoryName);

            foreach (var sound in category.sounds)
            {
                if (string.IsNullOrEmpty(sound.name))
                    continue;

                string valueName = CleanName($"{categoryName}__{sound.name}");

                if (usedNames.Contains(valueName))
                    continue;

                usedNames.Add(valueName);

                int assignedValue;

                if (existingValues.TryGetValue(valueName, out int existing))
                {
                    assignedValue = existing; // preserve old value
                }
                else
                {
                    assignedValue = nextValue;
                    nextValue++;
                }

                string displayName = $"{category.categoryName} - {sound.name}";

                builder.AppendLine($"        [InspectorName(\"{displayName}\")]");
                builder.AppendLine($"        {valueName} = {assignedValue},");
            }
        }

        builder.AppendLine("    }");
        builder.AppendLine("}");

        System.IO.File.WriteAllText(filePath, builder.ToString());
        AssetDatabase.Refresh();

        Debug.Log($"{assetName} enum regenerated safely (values preserved).");
    }
}
