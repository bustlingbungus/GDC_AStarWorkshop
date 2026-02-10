using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(PathfindFloor))]
[CanEditMultipleObjects]
public class FloorEditor : Editor
{
    private SerializedProperty tileSeparation;
    private float tileSepMin = 0f;
    private float tileSepMax = 5f;
    
    
    private SerializedProperty cameraHeight;
    private float camHeightMin = 5f;
    private float camHeightMax = 40f;
    
    private SerializedProperty npcHeight;
    private float npcMin = 0f;
    private float npcMax = 2f;


    private void OnEnable()
    {
        tileSeparation = serializedObject.FindProperty("tileSeparation");
        cameraHeight = serializedObject.FindProperty("cameraHeight");
        npcHeight = serializedObject.FindProperty("NPCHeight");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        TileSeparationUI();
        CameraHeightUI();
        NPCHeightUI();

        serializedObject.ApplyModifiedProperties();
        RefactorFloor();
    }


    private void TileSeparationUI()
    {
        if (tileSeparation != null)
        {
            float val = tileSeparation.floatValue;
            val = EditorGUILayout.Slider("Tile Separation", val, tileSepMin, tileSepMax);
            tileSeparation.floatValue = val;
        }
        else
        {
            EditorGUILayout.LabelField("Property 'tileSeparation' not found.");
        }
    }



    private void CameraHeightUI()
    {
        if (cameraHeight != null)
        {
            float val = cameraHeight.floatValue;
            val = EditorGUILayout.Slider("Camera Height", val, camHeightMin, camHeightMax);
            cameraHeight.floatValue = val;
        }
        else
        {
            EditorGUILayout.LabelField("Property 'cameraHeight' not found.");
        }
    }
    
    private void NPCHeightUI()
    {
        if (npcHeight != null)
        {
            // float val = npcHeight.floatValue;
            float val = npcHeight.floatValue;
            val = EditorGUILayout.Slider("NPC Height", val, npcMin, npcMax);
            npcHeight.floatValue = val;
        }
        else
        {
            EditorGUILayout.LabelField("Property 'NPCHeight' not found.");
        }
    }


    public static void RefactorFloor()
    {
        GameObject floor = GameObject.FindGameObjectWithTag("MainFloor");
        if (floor != null)
        {
            PathfindFloor comp = floor.GetComponent<PathfindFloor>();
            if (comp != null) comp.RefactorFloor();
        }
    }
}
