using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;


public class AssetHandler
{
    [OnOpenAsset]
    public static bool OpenEditor(int instanceId, int line)
    {
        SoundDataStorage obj = EditorUtility.EntityIdToObject(instanceId) as SoundDataStorage;
        if (obj != null)
        {
            SoundManagerEditorWindow.Open(obj);
            return true;
        }
        return false;
    }
}



[CustomEditor(typeof(SoundDataStorage))]
public class SoundManagerCustomEditor : Editor
{
    

    public override void OnInspectorGUI()
    {

        if (GUILayout.Button("Open Sound Editor"))
        {
            SoundManagerEditorWindow.Open((SoundDataStorage)target);
        }
    }

}
