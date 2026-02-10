using UnityEngine;


public enum TileType
{
    Start,
    Normal,
    Wall,
    Finish,
}


public class FloorTile : MonoBehaviour
{
    [HideInInspector]
    [SerializeField]
    public TileType type = TileType.Normal;

    [HideInInspector]
    public Material startMaterial = null;
    [HideInInspector]
    public Material normalMaterial = null;
    [HideInInspector]
    public Material wallMaterial = null;
    [HideInInspector]
    public Material finishMaterial = null;
}
