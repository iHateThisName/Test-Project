using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Node", menuName = "Scriptable Objects/Node")]
public class Node : TileBase
{
    private Sprite sprite;
    private Color color;
    private GameObject gameObject;

    private Vector3Int currentGridPosition;

    public void Init(Color color, Sprite sprite, GameObject baseGameObject, Vector3Int gridPosition) {
        this.color = color;
        this.sprite = sprite;
        this.gameObject = baseGameObject;
        this.currentGridPosition = gridPosition;

        this.gameObject.name = $"Generated Node ({gridPosition.x}, {gridPosition.y})";
    }

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        // Tell the tilemap to spawn this game object
        tileData.gameObject = gameObject;
    }

    public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject instancedGameObject)
    {
        if (instancedGameObject != null)
        {
            // Apply the sprite and color to the SpriteRenderer of the instanced gameObject
            SpriteRenderer renderer = instancedGameObject.GetComponentInChildren<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.sprite = sprite;
                renderer.color = color;
            }
        }

        return true;
    }
}
