using UnityEngine;
using UnityEditor;


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
}
