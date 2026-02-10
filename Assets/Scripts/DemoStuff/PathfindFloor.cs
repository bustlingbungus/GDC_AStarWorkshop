using System.Collections.Generic;
using Unity.VisualScripting;
// using UnityEditor.iOS;
using UnityEngine;

public class PathfindFloor : MonoBehaviour
{
    [HideInInspector]
    [SerializeField]
    // distance between each tile
    private float tileSeparation = 0.5f;

    [HideInInspector]
    [SerializeField]
    // how high the camera is above the tiles
    private float cameraHeight = 20f;
    
    [HideInInspector]
    [SerializeField]
    // how high the npc above the tiles
    private float NPCHeight = 0.5f;


    [SerializeField]
    GameObject pathfindingNPC = null;
    // reference to instantiated npc prefab
    GameObject NPC = null;


    // Start tile NPC will be spawned at
    FloorTile startTile = null;

    // array of all tiles in the floor
    List<FloorTile> tiles = null;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RefactorFloor();
    }

    // Update is called once per frame
    void Update()
    {
        // set npc height, and make sure npc has tiles
        if (NPC != null &  tiles.Count > 0)
        {
            Vector3 npcpos = NPC.transform.position;
            npcpos.y = tiles[0].transform.position.y + NPCHeight + NPC.transform.lossyScale.y + tiles[0].transform.lossyScale.y;
            NPC.transform.position = npcpos;
            GiveTilesToNPC();
        }
    }



    // gives the pathfinding npc a list of tiles as gameobjects, 
    // and tells it how far apart tile centers are
    void GiveTilesToNPC()
    {
        if (NPC != null)
        {
            PathfindingNPC script = NPC.GetComponent<PathfindingNPC>();
            if (script != null)
            {
                // give npc all tiles
                if (script.pathfinding_tiles == null || script.pathfinding_tiles.Count == 0) 
                    script.pathfinding_tiles = TilesAsObjects();

                // give npc distance between tile centers
                if (script.tile_separation == -1f && tiles.Count > 0)
                    script.tile_separation = tiles[0].transform.localScale.x + tileSeparation;
            }
        }
    }


    // converts tiles to a gameobject array 
    List<GameObject> TilesAsObjects()
    {
        List<GameObject> tile_objs = new List<GameObject>();
        foreach (FloorTile tile in tiles)
        {
            tile_objs.Add(tile.gameObject);
        }
        return tile_objs;
    }


    // converts children to array of floor tile scripts
    void GatherFloorTiles()
    {
        tiles = new List<FloorTile>();

        for (int i = 0; i < transform.childCount; ++i)
        {
            Transform child = transform.GetChild(i);

            // validate current child is a floor tile with the proper component
            FloorTile tile = child.GetComponent<FloorTile>();
            if (tile != null) tiles.Add(tile);
        }
    }


    // locates the start tile. if there is none, randomly makes a tile the start
    // if there are multiple, chooses one randomly to be the sole start tile
    FloorTile FindStartTile()
    {
        FloorTile start = null;
        if (tiles.Count > 0)
        {
            for (int i = 0; i < tiles.Count; ++i) 
            {
                FloorTile t = tiles[i];
                if (t.type == TileType.Start)
                {
                    if (start == null) start = t; // start tile found
                    else // multiple start tiles, make all others normal
                    {
                        Debug.Log("There can only be one starting tile!");
                        t.type = TileType.Normal;
                    }
                }
            }

            // if there was no starting tile, try to make the first tile start
            if (start == null)
            {
                Debug.Log("No starting tile found! Replacing random tile with start");
                start = tiles[0];
                start.type = TileType.Start;
            }
        }

        return start;
    }
    
    

    // locates the end tile. if there is none, randomly makes a tile the end
    // if there are multiple, chooses one randomly to be the sole end tile
    FloorTile FindEndTile()
    {
        FloorTile end = null;
        if (tiles.Count > 0)
        {
            for (int i = 0; i < tiles.Count; ++i) 
            {
                FloorTile t = tiles[i];
                if (t.type == TileType.Start)
                {
                    if (end == null) end = t; // end tile found
                    else // multiple end tiles, make all others normal
                    {
                        Debug.Log("There can only be one finish tile!");
                        t.type = TileType.Normal;
                    }
                }
            }

            // if there was no ending tile, try to make the first tile end
            if (end == null)
            {
                Debug.Log("No finish tile found! Replacing random tile with finish");
                end = tiles[0];
                end.type = TileType.Finish;
            }
        }

        return end;
    }


    // Finds the tile closest to the scene's camera
    Transform FindOriginTile()
    {
        Transform closest = null;
        float best = float.PositiveInfinity;

        Vector3 origin = Camera.main.transform.position;

        foreach(FloorTile tile in tiles)
        {
            Transform t = tile.transform;

            float dist = (origin - t.position).magnitude;
            if (dist <= best)
            {
                closest = t;
                best = dist;
            }
        }

        return closest;
    }


    // fixes position of all floor tiles. Fixes/spawns pathfinding NPC
    // Gives the NPC array of all floor tiles, and calls the NPC's pathfinding function
    public void RefactorFloor()
    {
        // get array of floor tiles
        if (tiles == null || tiles.Count == 0) GatherFloorTiles();
        if (tiles.Count > 0)
        {
            // find floor center and fix tile positions
            Transform origin = FindOriginTile();
            if (origin != null) FixTileTransforms(origin);

            // get start/end tiles tile
            startTile = FindStartTile();
            FindEndTile();

            // prepare npc and pathfind
            SpawnPathfindingNPC();
            GiveTilesToNPC();
            if (NPC != null)
            {
                PathfindingNPC script = NPC.GetComponent<PathfindingNPC>();
                if (script != null) script.Pathfind();
            }
        }

        // set camera height
        Vector3 cpos = Camera.main.transform.position;
        cpos.y = cameraHeight;
        Camera.main.transform.position = cpos;
    }


    // assigns all tiles to be along a grid relative to the origin tile
    void FixTileTransforms(Transform referenceTile)
    {
        // distances between tile centers along the x and z axes
        float x_step = referenceTile.localScale.x + tileSeparation;
        float z_step = referenceTile.localScale.z + tileSeparation;

        for (int i = 0; i < tiles.Count; ++i)
        {
            FloorTile t = tiles[i];

            // conform tile scale
            t.transform.localScale = referenceTile.localScale;

            // find x and z coordinates along a grid with x and z step sizes
            float fixed_x = (Utils.closestIntegerFactor(t.transform.position.x - referenceTile.position.x, x_step) * x_step) + referenceTile.position.x;
            float fixed_z = (Utils.closestIntegerFactor(t.transform.position.z - referenceTile.position.z, z_step) * z_step) + referenceTile.position.z;

            // assign new positon on grid
            t.transform.position = new Vector3(fixed_x, referenceTile.position.y, fixed_z);
        }
    }


    // spawns npc if it does not exist
    // fixes NPC location to starting tile
    void SpawnPathfindingNPC()
    {
        if (startTile != null && pathfindingNPC != null)
        {
            Vector3 npcpos = startTile.transform.position + new Vector3(0f, NPCHeight + pathfindingNPC.transform.lossyScale.y + startTile.transform.lossyScale.y, 0f);
            if (NPC == null) NPC = Instantiate(pathfindingNPC, npcpos, Quaternion.identity);
            else NPC.transform.position = npcpos;
        }
    }
}
