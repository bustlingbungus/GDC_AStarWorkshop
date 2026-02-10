using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class PathfindingNPC : MonoBehaviour
{
    // prefab to highlight movement tiles
    [SerializeField] GameObject tileHighlight = null;




    // array of tile objects to use in pathfinding
    [HideInInspector]
    public List<GameObject> pathfinding_tiles = null;
    // distance between each tile center
    [HideInInspector]
    public float tile_separation = -1f;


    // list of tile objects the npc will move to next!
    List<GameObject> my_next_movements;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Pathfind();
    }



    public void Pathfind()
    {
        // cannot perform pathfinding with no tiles
        if (pathfinding_tiles == null || pathfinding_tiles.Count == 0) return;


        // find start and end points
        GameObject start = findStartTile();
        GameObject end = findEndTile();
        
        my_next_movements = AStar.findPath(pathfinding_tiles, tile_separation, start, end, 500);
    
        HighlightMovementTiles();
    }


    // just find the tile im closest to
    GameObject findStartTile()
    {
        GameObject best = null;
        float least_dist = float.PositiveInfinity;

        foreach (GameObject t in pathfinding_tiles)
        {
            float dist = (transform.position - t.transform.position).magnitude;
            if (dist < least_dist)
            {
                best = t;
                least_dist = dist;
            }
        }

        return best;
    }


    // for this demo, just the tile marked as end
    GameObject findEndTile()
    {
        foreach (GameObject t in pathfinding_tiles)
        {
            FloorTile tile = t.GetComponent<FloorTile>();
            if (tile != null && tile.type == TileType.Finish) return t;
        }

        return pathfinding_tiles[0];
    }




    // highlights all tiles on the discovered movement path
    void HighlightMovementTiles()
    {
        DestroyExistingHighlights();

        // spawn a highlight for each tile in the path, 
        // skip the first tile (that npc is on)
        for (int i = 1; i < my_next_movements.Count; ++i)
        {
            GameObject tile = my_next_movements[i]; 
            
            Vector3 pos = new Vector3(
                tile.transform.position.x,
                tile.transform.position.y + ((tile.transform.lossyScale.y + tileHighlight.transform.lossyScale.y) / 2),
                tile.transform.position.z
            );

            Instantiate(tileHighlight, pos, Quaternion.identity);
        }
    }


    // clears existing path highligh
    void DestroyExistingHighlights()
    {
        GameObject[] highlights = GameObject.FindGameObjectsWithTag("TileHighlight");
        foreach (GameObject obj in highlights) DestroyImmediate(obj);
    }
}
