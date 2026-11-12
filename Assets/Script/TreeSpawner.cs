using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TreeType
{
    public string name;
    public GameObject prefab;
    [Tooltip("Más peso = aparece más a menudo")]
    public float weight = 1f;
    [Tooltip("Distancia mínima a otros árboles (en unidades). Sugerido: ancho del sprite / PPU")]
    public float clearance = 1f;
}

[System.Serializable]
public class SpawnZone
{
    public string name = "Zona";
    [Tooltip("BoxCollider2D (Is Trigger) que marca el área de la zona")]
    public BoxCollider2D area;
    [Tooltip("Árboles que pueden salir en ESTA zona")]
    public List<TreeType> allowedTrees = new List<TreeType>();
    public int maxTrees = 10;
    [Range(0f, 1f)] public float initialFill = 0.7f;

    [HideInInspector] public List<SpawnedTree> alive = new List<SpawnedTree>();
    [HideInInspector] public int pendingRespawns;
}

public class TreeSpawner : MonoBehaviour
{
    public List<SpawnZone> zones = new List<SpawnZone>();

    [Header("Reaparición (segundos)")]
    public float respawnMin = 60f;
    public float respawnMax = 180f;

    [Header("Colocación")]
    [Tooltip("Capas que bloquean (jugador, edificios, agua...)")]
    public LayerMask blockLayers;
    public int placementAttempts = 25;

    [Tooltip("Pon un número para que el mapa salga igual siempre. 0 = distinto cada vez")]
    public int seed = 0;

    void Start()
    {
        if (seed != 0) Random.InitState(seed);

        for (int i = 0; i < zones.Count; i++)
        {
            int count = Mathf.RoundToInt(zones[i].maxTrees * zones[i].initialFill);
            for (int n = 0; n < count; n++) TrySpawn(i);
        }
    }

    bool TrySpawn(int zoneIndex)
    {
        SpawnZone zone = zones[zoneIndex];
        if (zone.area == null || zone.allowedTrees.Count == 0) return false;
        if (zone.alive.Count >= zone.maxTrees) return false;

        TreeType type = PickWeighted(zone.allowedTrees);
        Bounds b = zone.area.bounds;

        for (int a = 0; a < placementAttempts; a++)
        {
            Vector2 p = new Vector2(
                Random.Range(b.min.x, b.max.x),
                Random.Range(b.min.y, b.max.y));

            if (!IsFree(p, type.clearance, zone)) continue;

            GameObject go = Instantiate(type.prefab, p, Quaternion.identity, transform);
            SpawnedTree tree = go.GetComponent<SpawnedTree>();
            if (tree == null) tree = go.AddComponent<SpawnedTree>();
            tree.owner = this;
            tree.zoneIndex = zoneIndex;
            zone.alive.Add(tree);
            return true;
        }
        return false; // no encontró hueco, lo intentará la próxima vez
    }

    bool IsFree(Vector2 p, float clearance, SpawnZone zone)
    {
        // Lejos de otros árboles de la zona
        foreach (var t in zone.alive)
            if (t != null && Vector2.Distance(t.transform.position, p) < clearance)
                return false;

        // Lejos de jugador/edificios/etc.
        if (blockLayers.value != 0 && Physics2D.OverlapCircle(p, clearance * 0.5f, blockLayers))
            return false;

        return true;
    }

    TreeType PickWeighted(List<TreeType> list)
    {
        float total = 0f;
        foreach (var t in list) total += t.weight;
        float r = Random.value * total;
        foreach (var t in list)
        {
            r -= t.weight;
            if (r <= 0f) return t;
        }
        return list[list.Count - 1];
    }

    /// <summary>Lo llama SpawnedTree al talarse.</summary>
    public void OnTreeRemoved(SpawnedTree tree)
    {
        SpawnZone zone = zones[tree.zoneIndex];
        zone.alive.Remove(tree);
        zone.pendingRespawns++;
        StartCoroutine(RespawnRoutine(tree.zoneIndex));
    }

    IEnumerator RespawnRoutine(int zoneIndex)
    {
        yield return new WaitForSeconds(Random.Range(respawnMin, respawnMax));

        SpawnZone zone = zones[zoneIndex];
        zone.pendingRespawns--;

        // Si no encuentra hueco, reintenta en 10 s
        while (!TrySpawn(zoneIndex) && zone.alive.Count < zone.maxTrees)
            yield return new WaitForSeconds(10f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.7f, 0.4f, 1f, 0.8f);
        foreach (var z in zones)
            if (z.area != null) Gizmos.DrawWireCube(z.area.bounds.center, z.area.bounds.size);
    }
}
