using TMPro;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(FloorTile))]
[CanEditMultipleObjects]
public class TileEditor : Editor
{
    private SerializedProperty tileType;

    private void OnEnable()
    {
        tileType = serializedObject.FindProperty("type");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        TypeUI();

        serializedObject.ApplyModifiedProperties();
        AssignTileMaterials();
    }

    void TypeUI()
    {
        TileType type = (TileType)tileType.enumValueFlag;
        type = (TileType)EditorGUILayout.EnumPopup("Tile Type ", type);
        tileType.enumValueFlag = (int)type;
    }


    void AssignTileMaterials()
    {
        GameObject[] tiles = GameObject.FindGameObjectsWithTag("PathfindTile");
        foreach (GameObject t in tiles)
        {
            FloorTile tile = t.GetComponent<FloorTile>();
            MeshRenderer rend = t.GetComponent<MeshRenderer>();
            if (tile != null && rend != null)
            {
                switch (tile.type)
                {
                    default:
                    case TileType.Normal:
                        rend.material = tile.normalMaterial;
                        break;
                    case TileType.Start:
                        rend.material = tile.startMaterial;
                        break;
                    case TileType.Finish:
                        rend.material = tile.finishMaterial;
                        break;
                    case TileType.Wall:
                        rend.material = tile.wallMaterial;
                        break;
                }
            }
        }
    }
}
