using UnityEngine;

/// <summary>
/// Ponlo en el prefab de CADA árbol (junto a SpriteRenderer y un Collider2D).
/// El spawner lo configura solo. Para talar: llama a Hit() o Cut() desde tu personaje.
/// </summary>
public class SpawnedTree : MonoBehaviour
{
    [Tooltip("Golpes necesarios para talarlo")]
    public int hitsToCut = 3;

    [HideInInspector] public TreeSpawner owner;
    [HideInInspector] public int zoneIndex;

    int hits;

    void Start()
    {
        // Orden de dibujado por altura (Y): los de abajo se dibujan delante.
        // Requiere que el pivote del sprite esté en "Bottom Center".
        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) sr.sortingOrder = -Mathf.RoundToInt(transform.position.y * 100f);
    }

    public void Hit()
    {
        hits++;
        if (hits >= hitsToCut) Cut();
    }

    public void Cut()
    {
        if (owner != null) owner.OnTreeRemoved(this);
        Destroy(gameObject);
    }
}
