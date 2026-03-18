using System.Collections.Generic;
using UnityEngine;

public class ChunkPool : MonoBehaviour
{
    public List<WeightedChunk> prefabs;
    public int preloadAmount = 5;
    private Dictionary<RoadChunk, Queue<RoadChunk>> pool = new();

    void Awake()
    {
        foreach(WeightedChunk entry in prefabs)
        {
            Queue<RoadChunk> queue = new Queue<RoadChunk>();
            for(int i = 0; i < preloadAmount; i++)
            {
                RoadChunk chunk = Instantiate(entry.prefab, transform);
                chunk.gameObject.SetActive(false);
                queue.Enqueue(chunk);
            }
            pool.Add(entry.prefab, queue);
        }
    }

    public RoadChunk GetChunk(RoadChunk prefab)
    {
        if(!pool.ContainsKey(prefab))
        {
            Debug.LogWarning("No pool for " + prefab.name);
            return null;
        }

        Queue<RoadChunk> queue = pool[prefab];

        if(queue.Count == 0)
        {
            RoadChunk chunk = Instantiate(prefab, transform);
            chunk.gameObject.SetActive(false);
            queue.Enqueue(chunk);
        }

        RoadChunk result = queue.Dequeue();
        result.gameObject.SetActive(true);   
        result.OnSpawn();
        return result;
    }

    public void ReturnChunk(RoadChunk chunk, RoadChunk prefab)
    {
        chunk.OnDespawn();
        chunk.gameObject.SetActive(false);
        pool[prefab].Enqueue(chunk);
    }
}
