//dropped, not used, but kept for future use

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class mapgen : MonoBehaviour
{
    [Header("Streaming")]
    public Transform player;
    public int seed = 2026;
    [Min(1)] public int chunkWidth = 36;
    [Min(1)] public int chunkHeight = 28;
    [Min(1)] public int loadedChunkRadius = 1;
    [Min(0.1f)] public float tileSize = 1f;

    [Header("Palette")]
    public TileBase[] palette;
    [Range(0f, 0.25f)] public float propDensity = 0.055f;
    public int sortingOrder;

    private readonly Dictionary<Vector2Int, GameObject> loadedChunks = new Dictionary<Vector2Int, GameObject>();
    private readonly List<TileBase> groundTiles = new List<TileBase>();
    private readonly List<TileBase> walls = new List<TileBase>();
    private readonly List<TileBase> gates = new List<TileBase>();
    private readonly List<TileBase> props = new List<TileBase>();
    private readonly List<TileBase> transparentProps = new List<TileBase>();
    private readonly List<TileBase> corners = new List<TileBase>();
    private TileBase grassTile;
    private Vector2Int lastChunk = new Vector2Int(int.MinValue, int.MinValue);

    private static Sprite fallbackFloor;
    private static Sprite fallbackWall;

    private enum Biome
    {
        GrassyField,
        Room,
        Storage,

    }

    private void Start()
    {
        if (player == null)
            player = transform;
        ClassifyPalette();
        UpdateVisibleChunks(true);
    }

    private void Update()
    {
        UpdateVisibleChunks(false);
    }

    private void ClassifyPalette()
    {
        groundTiles.Clear();
        walls.Clear();
        gates.Clear();
        props.Clear();
        transparentProps.Clear();
        corners.Clear();
        grassTile = null;

        if (palette == null)
            return;

        foreach (TileBase tile in palette)
        {
            if (tile == null)
                continue;

            string name = tile.name.ToLowerInvariant();
            if (name.StartsWith("tilesheet_"))
                continue;

            if (name.Contains("no_bg") || name.Contains("nobg"))
            {
                transparentProps.Add(tile);
                continue;
            }

            if (name == "grass" || name.StartsWith("grass_"))
            {
                if (grassTile == null)
                    grassTile = tile;
                continue;
            }

            if (name == "blank" || name == "squares")
            {
                groundTiles.Add(tile);
                continue;
            }

            if (name.Contains("gate"))
            {
                gates.Add(tile);
                continue;
            }

            if (name.Contains("wall") || name.Contains("corner"))
            {
                if (IsCornerTile(name))
                    corners.Add(tile);
                else
                    walls.Add(tile);
                continue;
            }

            props.Add(tile);
        }
    }

    private void UpdateVisibleChunks(bool force)
    {
        Vector2Int currentChunk = WorldToChunk(player.position);
        if (!force && currentChunk == lastChunk)
            return;
        lastChunk = currentChunk;

        for (int y = -loadedChunkRadius; y <= loadedChunkRadius; y++)
        {
            for (int x = -loadedChunkRadius; x <= loadedChunkRadius; x++)
            {
                Vector2Int coordinate = currentChunk + new Vector2Int(x, y);
                if (!loadedChunks.ContainsKey(coordinate))
                    loadedChunks.Add(coordinate, CreateChunk(coordinate));
            }
        }

        List<Vector2Int> chunksToRemove = new List<Vector2Int>();
        foreach (Vector2Int coordinate in loadedChunks.Keys)
        {
            if (Mathf.Abs(coordinate.x - currentChunk.x) > loadedChunkRadius ||
                Mathf.Abs(coordinate.y - currentChunk.y) > loadedChunkRadius)
                chunksToRemove.Add(coordinate);
        }

        foreach (Vector2Int coordinate in chunksToRemove)
        {
            Destroy(loadedChunks[coordinate]);
            loadedChunks.Remove(coordinate);
        }
    }

    private GameObject CreateChunk(Vector2Int coordinate)
    {
        GameObject chunk = new GameObject("Chunk_" + coordinate.x + "_" + coordinate.y);
        chunk.transform.SetParent(transform);

        System.Random random = new System.Random(StableHash(coordinate.x, coordinate.y, seed));
        int worldX = coordinate.x * chunkWidth;
        int worldY = coordinate.y * chunkHeight;
        Biome biome = (Biome)random.Next(3);
        TileBase ground = PickGround(biome, random);

        for (int y = 0; y < chunkHeight; y++)
        {
            for (int x = 0; x < chunkWidth; x++)
            {
                CreateSprite(chunk, ground, new Vector3((worldX + x) * tileSize, (worldY + y) * tileSize, 1f), "Ground", false, sortingOrder);
            }
        }

        if (biome != Biome.GrassyField)
            CreateRoomWalls(chunk, random, worldX, worldY);

        PlaceProps(chunk, random, biome, worldX, worldY);
        return chunk;
    }

    private TileBase PickGround(Biome biome, System.Random random)
    {
        if (biome == Biome.GrassyField && grassTile != null)
            return grassTile;

        if (biome == Biome.Storage)
        {
            foreach (TileBase tile in groundTiles)
            {
                if (tile.name.ToLowerInvariant() == "squares")
                    return tile;
            }
        }

        foreach (TileBase tile in groundTiles)
        {
            if (tile.name.ToLowerInvariant() == "blank")
                return tile;
        }

        return groundTiles.Count > 0 ? groundTiles[random.Next(groundTiles.Count)] : null;
    }

    private void CreateRoomWalls(GameObject chunk, System.Random random, int worldX, int worldY)
    {
        int middleX = chunkWidth / 2;
        int middleY = chunkHeight / 2;
        for (int x = 1; x < chunkWidth - 1; x++)
        {
            if (x != middleX)
            {
                CreateWall(chunk, random, worldX + x, worldY);
                CreateWall(chunk, random, worldX + x, worldY + chunkHeight - 1);
            }
        }

        for (int y = 1; y < chunkHeight - 1; y++)
        {
            if (y != middleY)
            {
                CreateWall(chunk, random, worldX, worldY + y);
                CreateWall(chunk, random, worldX + chunkWidth - 1, worldY + y);
            }
        }

        PlaceGate(chunk, random, worldX + middleX, worldY);
        PlaceGate(chunk, random, worldX + middleX, worldY + chunkHeight - 1);
        PlaceGate(chunk, random, worldX, worldY + middleY);
        PlaceGate(chunk, random, worldX + chunkWidth - 1, worldY + middleY);

        CreateCorner(chunk, random, worldX, worldY, "lb");
        CreateCorner(chunk, random, worldX + chunkWidth - 1, worldY, "rb");
        CreateCorner(chunk, random, worldX, worldY + chunkHeight - 1, "lt");
        CreateCorner(chunk, random, worldX + chunkWidth - 1, worldY + chunkHeight - 1, "rt");
    }

    private void CreateWall(GameObject chunk, System.Random random, int x, int y)
    {
        CreateSprite(chunk, Pick(walls, random), new Vector3(x * tileSize, y * tileSize, 0f), "Wall", true, sortingOrder + 1);
    }

    private void CreateCorner(GameObject chunk, System.Random random, int x, int y, string direction)
    {
        TileBase cornerTile = PickCornerTile(direction, random);
        if (cornerTile != null)
            CreateSprite(chunk, cornerTile, new Vector3(x * tileSize, y * tileSize, 0f), "Corner", true, sortingOrder + 1);
    }

    private TileBase PickCornerTile(string direction, System.Random random)
    {
        foreach (TileBase corner in corners)
        {
            if (HasDirectionSuffix(corner.name, direction))
                return corner;
        }

        return corners.Count > 0 ? corners[random.Next(corners.Count)] : null;
    }

    private void PlaceGate(GameObject chunk, System.Random random, int x, int y)
    {
        TileBase gate = PickPassableGate(random);
        if (gate != null)
            CreateSprite(chunk, gate, new Vector3(x * tileSize, y * tileSize, 0f), "Gate", false, sortingOrder + 1);
    }

    private TileBase PickPassableGate(System.Random random)
    {
        List<TileBase> passableGates = new List<TileBase>();
        foreach (TileBase gate in gates)
        {
            string name = gate.name.ToLowerInvariant();
            if (!name.Contains("closed"))
                passableGates.Add(gate);
        }
        return passableGates.Count == 0 ? null : passableGates[random.Next(passableGates.Count)];
    }

    private void PlaceProps(GameObject chunk, System.Random random, Biome biome, int worldX, int worldY)
    {
        List<TileBase> availableProps = new List<TileBase>(props);
        availableProps.AddRange(transparentProps);
        if (availableProps.Count == 0)
            return;

        float density = propDensity;
        if (biome == Biome.GrassyField)
            density *= 0.35f;
        else if (biome == Biome.Room)
            density *= 0.65f;
        else
            density = Mathf.Min(0.25f, density * 2.5f);

        int attempts = Mathf.RoundToInt((chunkWidth - 2) * (chunkHeight - 2) * density);
        HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();
        for (int i = 0; i < attempts; i++)
        {
            int x = random.Next(2, Mathf.Max(3, chunkWidth - 2));
            int y = random.Next(2, Mathf.Max(3, chunkHeight - 2));
            Vector2Int position = new Vector2Int(x, y);
            if (!occupied.Add(position))
                continue;

            TileBase prop = availableProps[random.Next(availableProps.Count)];
            CreateSprite(chunk, prop, new Vector3((worldX + x) * tileSize, (worldY + y) * tileSize, -1f), "Prop", true, sortingOrder + 2);
        }
    }

    private GameObject CreateSprite(GameObject parent, TileBase tile, Vector3 position, string objectName, bool collider, int order)
    {
        Sprite sprite = GetSprite(tile);
        if (sprite == null)
            sprite = objectName == "Wall" || objectName == "Corner" ? FallbackWall() : FallbackFloor();

        GameObject item = new GameObject(objectName);
        item.transform.SetParent(parent.transform);
        item.transform.position = position;
        item.transform.localScale = Vector3.one * tileSize;
        SpriteRenderer renderer = item.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = order;
        if (collider)
        {
            BoxCollider2D box = item.AddComponent<BoxCollider2D>();
            box.size = Vector2.one;
        }
        return item;
    }

    private static Sprite GetSprite(TileBase tile)
    {
        Tile regularTile = tile as Tile;
        return regularTile == null ? null : regularTile.sprite;
    }

    private static TileBase Pick(List<TileBase> tiles, System.Random random)
    {
        return tiles.Count == 0 ? null : tiles[random.Next(tiles.Count)];
    }

    private Vector2Int WorldToChunk(Vector3 position)
    {
        return new Vector2Int(Mathf.FloorToInt(position.x / (chunkWidth * tileSize)),
            Mathf.FloorToInt(position.y / (chunkHeight * tileSize)));
    }

    private static int StableHash(int x, int y, int value)
    {
        unchecked
        {
            int hash = value;
            hash = hash * 31 + x;
            hash = hash * 31 + y;
            return hash;
        }
    }

    private static Sprite FallbackFloor()
    {
        return fallbackFloor ?? (fallbackFloor = CreateFallback("Generated Floor", new Color(0.18f, 0.23f, 0.2f)));
    }

    private static Sprite FallbackWall()
    {
        return fallbackWall ?? (fallbackWall = CreateFallback("Generated Wall", new Color(0.08f, 0.1f, 0.1f)));
    }

    private static Sprite CreateFallback(string name, Color color)
    {
        Texture2D texture = new Texture2D(1, 1) { filterMode = FilterMode.Point, name = name + " Texture" };
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    private static bool IsCornerTile(string tileName)
    {
        if (tileName.Contains("corner"))
            return true;

        string[] directionTokens = { "_lt", "_tl", "_rt", "_tr", "_lb", "_bl", "_rb", "_br" };
        foreach (string token in directionTokens)
        {
            if (tileName.EndsWith(token))
                return true;
        }
        return false;
    }

    private static bool HasDirectionSuffix(string tileName, string direction)
    {
        string name = tileName.ToLowerInvariant();
        string target = "_" + direction.ToLowerInvariant();
        if (name.EndsWith(target))
            return true;

        string reverse = "_" + direction[1] + direction[0];
        return name.EndsWith(reverse);
    }
}
