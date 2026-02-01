using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BloodController : MonoBehaviour
{
    List<ParticleCollisionEvent> events = new();
    ParticleSystem ps;

    public Tilemap bloodTilemap;
    public List<TileBase> bloodTiles;
    public float yOffset = 0.25f;

    private Dictionary<Vector3Int, int> bloodLevels = new();

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void OnParticleCollision(GameObject other)
    {
        int count = ps.GetCollisionEvents(other, events);
        Debug.Log("Número de colisões: " + count);
        if (count > 0)
        {
            Vector3 worldPos = events[0].intersection;
            worldPos.y -= yOffset;
            // worldPos is the collision point in world space

            if (bloodTilemap != null && bloodTiles != null && bloodTiles.Count > 0)
            {
                Vector3Int cellPos = bloodTilemap.WorldToCell(worldPos);
                if (!bloodLevels.ContainsKey(cellPos))
                {
                    bloodLevels[cellPos] = 0;
                }
                int currentLevel = bloodLevels[cellPos];
                if (currentLevel < bloodTiles.Count)
                {
                    bloodLevels[cellPos] = currentLevel + 1;
                    bloodTilemap.SetTile(cellPos, bloodTiles[currentLevel]);
                }
            }
        }


    }
}
