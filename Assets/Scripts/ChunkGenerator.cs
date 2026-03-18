using System.Collections.Generic;
using UnityEngine;

public class ChunkGenerator : MonoBehaviour
{
    public RoadChunk firstRoadPrefab;
    public List<WeightedChunk> roadPrefabs;
    public Transform player;
    public ChunkPool pool;
    public RoadChunk finalRoadChunk;
    public float endTarget=10000f;
    bool isFinished=false;
    float finalChunkLength;
    public int chunksAhead = 6;
    private List<(RoadChunk chunk, RoadChunk prefab)> activeChunks = new();
    private List<RoadChunk> spawnedChunks = new List<RoadChunk>();
    private Transform lastEndPoint;

    void Start()
    {
        var first = pool.prefabs[0].prefab;
        RoadChunk startChunk = pool.GetChunk(first);
        startChunk.transform.position = Vector3.zero;
        startChunk.transform.rotation = Quaternion.identity;
        lastEndPoint = startChunk.endpoint.transform;
        activeChunks.Add((startChunk, first));
        for (int i = 0; i < chunksAhead; i++)
            SpawnNext();
        
        finalChunkLength = Vector3.Distance(finalRoadChunk.startpoint.transform.position, finalRoadChunk.endpoint.transform.position);
    }

    void Update()
    {
        if (activeChunks.Count < 2) return;

        var firstChunk = activeChunks[0].chunk;
        Transform start = firstChunk.startpoint.transform;
        Transform end = firstChunk.endpoint.transform;

        Vector3 chunkDir = (end.position - start.position).normalized;
        float chunkLength = Vector3.Distance(start.position, end.position);
        Vector3 playerRelative = player.position - start.position;
        float progress = Vector3.Dot(playerRelative, chunkDir);

        // Если игрок продвинулся дальше конца чанка (с небольшим запасом)
        if (progress > chunkLength + 50f  && !isFinished)
        {
            RemoveOldest();
            SpawnNext();
        }
    }

    void SpawnNext()
    {
        if (isFinished) return;

        if(finalRoadChunk != null && lastEndPoint.position.x >= endTarget - finalChunkLength)
        {
            RoadChunk lastChunk = pool.GetChunk(finalRoadChunk);
        lastChunk.transform.rotation = lastEndPoint.rotation;
        lastChunk.transform.position = lastEndPoint.position - 
                                   (lastChunk.startpoint.transform.position - lastChunk.transform.position);

        lastEndPoint = lastChunk.endpoint.transform;
        activeChunks.Add((lastChunk, finalRoadChunk));
        isFinished = true;
        return; // не спавним обычные чанки
        }

        RoadChunk prefab = GetRandomWeightedChunk();
        RoadChunk chunk = pool.GetChunk(prefab);

        chunk.transform.rotation = lastEndPoint.rotation;
        chunk.transform.position = lastEndPoint.position - 
                           (chunk.startpoint.transform.position - chunk.transform.position);


        lastEndPoint = chunk.endpoint.transform;
        activeChunks.Add((chunk, prefab));
    }

    void RemoveOldest()
    {
        var oldest = activeChunks[0];
        pool.ReturnChunk(oldest.chunk, oldest.prefab);
        activeChunks.RemoveAt(0);
    }

    RoadChunk GetRandomWeightedChunk()
    {
        int totalWeight = 0;
        foreach (var wc in pool.prefabs)
            totalWeight += wc.weight;
        

        int randomValue = Random.Range(0, totalWeight);
        int cumulativeWeight = 0;

        foreach (var wc in pool.prefabs)
        {
            cumulativeWeight += wc.weight;
            if (randomValue < cumulativeWeight)
                return wc.prefab;
        }

        return pool.prefabs[0].prefab; // fallback
    }
}
[System.Serializable]
public class WeightedChunk
{
    public RoadChunk prefab;
    public int weight = 1;
}