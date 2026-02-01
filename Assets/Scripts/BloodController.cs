using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BloodController : MonoBehaviour
{
    List<ParticleCollisionEvent> events = new();
    ParticleSystem ps;

    public Tilemap bloodTilemap;
    public List<TileBase> bloodTiles;

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
            worldPos.y -= 0.25f;
            // worldPos is the collision point in world space

            if (bloodTilemap != null && bloodTiles != null && bloodTiles.Count > 0)
            {
                Vector3Int cellPos = bloodTilemap.WorldToCell(worldPos);
                bloodTilemap.SetTile(cellPos, bloodTiles[0]);
            }
        }


    }
}
