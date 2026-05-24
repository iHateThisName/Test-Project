using Assets._Scripts.Utilities.Singleton;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Grid))]
public class GridController : Singleton<GridController> {

    [SerializeField] private Vector2Int size;
    [field: SerializeField] public Grid Grid { get; private set; }

    [SerializeField] private Tilemap tilemap;
    [SerializeField] private List<Tile> tiles;

    public Dictionary<Vector2Int, TileEntityBase> TileEntities { get; private set; } = new Dictionary<Vector2Int, TileEntityBase>();
    private Camera cam;

    private bool isGridLandScapeMode = false;
    private bool isScreenLandscape => Screen.width > Screen.height;

    private Bounds bounds;

    protected override void Awake() {
        base.Awake();
        if (Grid == null) Grid = GetComponent<Grid>();
        this.cam = Camera.main;
    }
    void Start() {

        if (!this.isScreenLandscape) {
            // Portrait
            if (!(this.size.x < this.size.y)) {
                int temp = this.size.x;
                this.size.x = this.size.y;
                this.size.y = temp;
            }

            this.isGridLandScapeMode = false;

        } else if (this.isScreenLandscape && this.size.x > this.size.y) {
            // Landscape
            if (this.size.y < this.size.x) {
                int temp = this.size.x;
                this.size.x = this.size.y;
                this.size.y = temp;
            }

            this.isGridLandScapeMode = true;
        }

        ReCalculateGrid();
        GenerateGrid();
        SetCamera();
    }

    private void OnDrawGizmos() {
        if (Grid == null) Grid = GetComponent<Grid>();

        Gizmos.color = Color.green;

        for (int x = 0; x < size.x; x++) {
            for (int y = 0; y < size.y; y++) {
                Vector3Int cell = new Vector3Int(x, y, 0);
                Vector3 center = Grid.GetCellCenterWorld(cell);
                Vector3 cellSize = Grid.cellSize;
                Gizmos.DrawWireCube(center, cellSize);
            }
        }
    }

    public void RefreshGridAndCamera() {
        ReCalculateGrid();
        this.bounds = CalculateBounds();
        SetCamera();
    }

    private void ReCalculateGrid() {
        // Check the device orientation.
        if (this.isScreenLandscape && !this.isGridLandScapeMode) {
            // Landscape
            this.Grid.transform.RotateAround(this.bounds.center, Vector3.forward, 90f);
            this.isGridLandScapeMode = true;

        } else if (!this.isScreenLandscape && this.isGridLandScapeMode) {
            // Portrait
            if (this.Grid.transform.rotation.z != 0) {
                this.Grid.transform.RotateAround(this.bounds.center, Vector3.forward, -90f);
            }
            this.isGridLandScapeMode = false;
        }
    }

    private void SetCamera() {

        bool isOrthographic = cam.orthographic;

        var vertical = this.bounds.size.y;
        var horizontal = this.bounds.size.x * (float)cam.pixelHeight / (float)cam.pixelWidth;
        Vector3 distanceback = Vector3.back * this.size.magnitude;
        this.cam.transform.position = this.bounds.center + distanceback;

        if (isOrthographic) {
            this.cam.orthographicSize = Mathf.Max(horizontal, vertical) * 0.5f;
        } else {
            // Not the best way to do this esier with a orthographic camera.
            this.cam.transform.LookAt(this.bounds.center);
        }
    }

    private void GenerateGrid() {

        Vector3 center = this.Grid.GetCellCenterWorld(new Vector3Int((size.x - 1) / 2, (size.y - 1) / 2, 0));
        this.bounds = new Bounds(center, Vector3.zero);

        HashSet<Vector3Int> occupiedTiles = GetExistingTiles();

        for (int x = 0; x < size.x; x++) {
            for (int y = 0; y < size.y; y++) {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);

                // Check if the current selected tile is empty, if it is, select a random tile from the list of tile
                if (!this.tilemap.HasTile(cellPosition) && !occupiedTiles.Contains(cellPosition)) {

                    Tile randomTile = tiles[Random.Range(0, tiles.Count)];

                    var node = ScriptableObject.CreateInstance<GridTileAsset>();
                    node.Init(prefab: randomTile.TilePrefab, color: randomTile.TileColor);

                    this.tilemap.SetTile(cellPosition, node);
                } else {
                    Debug.Log($"Existing tile at {cellPosition}");
                }

                bounds.Encapsulate(this.Grid.GetCellCenterWorld(cellPosition));
            }
        }

        bounds.Expand(this.Grid.cellSize);
        bounds.Expand(this.Grid.cellSize);
    }

    private Bounds CalculateBounds() {
        if (this.Grid == null) return new Bounds();

        Vector3 center = this.Grid.GetCellCenterWorld(new Vector3Int((size.x - 1) / 2, (size.y - 1) / 2, 0));
        Bounds newBounds = new Bounds(center, Vector3.zero);

        for (int x = 0; x < size.x; x++) {
            for (int y = 0; y < size.y; y++) {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                newBounds.Encapsulate(this.Grid.GetCellCenterWorld(cellPosition));
            }
        }

        newBounds.Expand(this.Grid.cellSize);
        newBounds.Expand(this.Grid.cellSize);

        return newBounds;
    }

    private HashSet<Vector3Int> GetExistingTiles() {
        // Find all pre-existing GameObjects (like those placed by the GameObject brush)
        HashSet<Vector3Int> occupiedCells = new HashSet<Vector3Int>();
        foreach (Transform child in this.tilemap.transform) {
            Vector3Int item = this.Grid.WorldToCell(child.position);
            child.name = child.name + $" ({item.x}, {item.y})";
            occupiedCells.Add(item);
        }

        return occupiedCells;
    }

    public void InteractWithTile(Vector3Int cellPosition) {
        // Add basic interactions here
        if (TileEntities.TryGetValue(new Vector2Int(cellPosition.x, cellPosition.y), out TileEntityBase entity)) {
            Debug.Log($"Interacted with valid tile at Grid coordinates: {cellPosition}");

            entity.OnTileClicked();
        } else {
            Debug.Log("Tapped out of bounds or empty tile!");
        }
    }

    public void RegisterTile(Vector3Int position, TileEntityBase entity) {
        if (entity == null) return;
        TileEntities[new Vector2Int(position.x, position.y)] = entity;
    }

}

[System.Serializable]
public struct Tile {
    public TileEntityBase TilePrefab;
    public Color TileColor;
}
