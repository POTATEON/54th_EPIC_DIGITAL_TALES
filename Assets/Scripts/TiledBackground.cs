using UnityEngine;

public class TiledBackground : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private Sprite tileSprite;
    [SerializeField] private Vector2 worldSize = new Vector2(20f, 10f);
    [SerializeField] private float tileScale = 1f;

    private void Start()
    {
        CreateTiledBackground();
    }

    private void CreateTiledBackground()
    {
        if (tileSprite == null) return;

        float tileWidth = tileSprite.bounds.size.x * tileScale;
        float tileHeight = tileSprite.bounds.size.y * tileScale;

        int tilesX = Mathf.CeilToInt(worldSize.x / tileWidth);
        int tilesY = Mathf.CeilToInt(worldSize.y / tileHeight);

        float startX = -worldSize.x / 2f;
        float startY = -worldSize.y / 2f;

        for (int x = 0; x < tilesX; x++)
        {
            for (int y = 0; y < tilesY; y++)
            {
                var tile = new GameObject($"Tile_{x}_{y}");
                tile.transform.parent = transform;
                tile.transform.position = new Vector3(
                    startX + x * tileWidth + tileWidth / 2f,
                    startY + y * tileHeight + tileHeight / 2f,
                    0
                );

                var sr = tile.AddComponent<SpriteRenderer>();
                sr.sprite = tileSprite;
                sr.drawMode = SpriteDrawMode.Simple;
                sr.size = new Vector2(tileWidth, tileHeight);
                sr.sortingOrder = -10;
            }
        }
    }
}