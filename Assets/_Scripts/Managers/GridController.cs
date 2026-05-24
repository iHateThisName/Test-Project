using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Grid))]
public class GridController : MonoBehaviour {

    [SerializeField] private Vector2Int size;
    [SerializeField] private Grid grid;
    [SerializeField] private Tilemap tilemap;

    [SerializeField] private GameObject tilePrefab;

    [SerializeField] private List<Tile> tiles;

    private Camera cam;


    private void Awake() {
        if (grid == null) grid = GetComponent<Grid>();
        this.cam = Camera.main;
    }
    void Start() {
        Bounds bounds = GenerateGrid();

        SetCamera(bounds);
    }

    private Bounds GenerateGrid() {
        Vector3 center = this.grid.GetCellCenterWorld(new Vector3Int((size.x - 1) / 2, (size.y - 1) / 2, 0));
        Bounds bounds = new Bounds(center, Vector3.zero);

        HashSet<Vector3Int> occupiedTiles = GetExistingTiles();

        for (int x = 0; x < size.x; x++) {
            for (int y = 0; y < size.y; y++) {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);

                // Check if the current selected tile is empty, if it is, select a random tile from the list of tile
                if (!this.tilemap.HasTile(cellPosition) && !occupiedTiles.Contains(cellPosition)) {

                    Tile randomTile = tiles[Random.Range(0, tiles.Count)];

                    var b = ScriptableObject.CreateInstance<Node>();
                    b.Init(color: randomTile.TileColor, sprite: randomTile.TileSprite, baseGameObject: tilePrefab, gridPosition: cellPosition);
                    this.tilemap.SetTile(cellPosition, b);
                } else {
                    Debug.Log($"Existing tile at {cellPosition}");
                }

                bounds.Encapsulate(this.grid.GetCellCenterWorld(cellPosition));
            }
        }

        bounds.Expand(this.grid.cellSize);
        return bounds;
    }

    private HashSet<Vector3Int> GetExistingTiles() {
        // Find all pre-existing GameObjects (like those placed by the GameObject brush)
        HashSet<Vector3Int> occupiedCells = new HashSet<Vector3Int>();
        foreach (Transform child in this.tilemap.transform) {
            Vector3Int item = this.grid.WorldToCell(child.position);
            child.name = child.name + $" ({item.x}, {item.y})";
            occupiedCells.Add(item);
        }

        return occupiedCells;
    }

    private void SetCamera(Bounds bounds) {
        bounds.Expand(this.grid.cellSize); // Add some padding to the bounds so the tiles are not at the very edge of the screen

        bool isOrthographic = cam.orthographic;

        var vertical = bounds.size.y;
        var horizontal = bounds.size.x * (float)cam.pixelHeight / (float)cam.pixelWidth;
        Vector3 distanceback = Vector3.back * this.size.magnitude;
        this.cam.transform.position = bounds.center + distanceback;

        if (isOrthographic) {
            this.cam.orthographicSize = Mathf.Max(horizontal, vertical) * 0.5f;
        } else {
            // Not the best way to do this esier with a orthographic camera.
            this.cam.transform.LookAt(bounds.center);
        }
    }

}

[System.Serializable]
struct Tile {
    public Sprite TileSprite;
    public Color TileColor;
}
