using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns resources (trees, rocks) randomly in a zone
/// Regenerates destroyed resources on new day
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class ResourceSpawnZone : MonoBehaviour
{
    [Header("Zone Settings")]
    public string zoneName = "Forest Zone";
    
    [Header("Resources to Spawn")]
    public GameObject[] treePrefabs;
    public GameObject[] rockPrefabs;
    
    [Header("Spawn Amounts")]
    [Tooltip("How many trees to spawn")]
    public int minTrees = 5;
    public int maxTrees = 10;
    
    [Tooltip("How many rocks to spawn")]
    public int minRocks = 3;
    public int maxRocks = 8;
    
    [Header("Spawn Settings")]
    [Tooltip("Minimum distance between resources")]
    public float minDistanceBetweenResources = 2f;
    
    [Tooltip("Try this many times to find valid spawn position")]
    public int maxSpawnAttempts = 30;
    
    [Header("Regeneration")]
    [Tooltip("Should destroyed resources respawn next day?")]
    public bool regenerateOnNewDay = true;
    
    [Header("Initial Spawn")]
    [Tooltip("Spawn resources when scene starts")]
    public bool spawnOnStart = true;
    
    [Header("Debug")]
    public bool showGizmos = true;
    public Color zoneColor = new Color(0f, 1f, 0f, 0.2f);
    
    // Tracking
    private List<SpawnedResource> allSpawnedResources = new List<SpawnedResource>();
    private BoxCollider2D spawnZone;
    private int currentDay = 0;

    void Awake()
    {
        spawnZone = GetComponent<BoxCollider2D>();
        spawnZone.isTrigger = true;
    }

    void Start()
    {
        if (spawnOnStart)
        {
            SpawnInitialResources();
        }
        
        // Register with day/night manager
        DayNightManager dayManager = FindObjectOfType<DayNightManager>();
        if (dayManager != null)
        {
            dayManager.OnNewDay += OnNewDay;
            Debug.Log($"ResourceSpawnZone '{zoneName}' registered with DayNightManager");
        }
    }

    void OnDestroy()
    {
        // Unregister from day/night manager
        DayNightManager dayManager = FindObjectOfType<DayNightManager>();
        if (dayManager != null)
        {
            dayManager.OnNewDay -= OnNewDay;
        }
    }

    // Called when new day starts
    void OnNewDay(int day)
    {
        currentDay = day;
        Debug.Log($"[{zoneName}] New day: {day}");
        
        if (regenerateOnNewDay)
        {
            RegenerateDestroyedResources();
        }
    }

    [ContextMenu("Spawn Resources Now")]
    public void SpawnInitialResources()
    {
        Debug.Log($"[{zoneName}] Spawning initial resources...");
        
        // Clear existing tracking
        allSpawnedResources.Clear();
        
        // Spawn trees
        int treeCount = Random.Range(minTrees, maxTrees + 1);
        for (int i = 0; i < treeCount; i++)
        {
            SpawnResource(treePrefabs, ResourceType.Tree);
        }
        
        // Spawn rocks
        int rockCount = Random.Range(minRocks, maxRocks + 1);
        for (int i = 0; i < rockCount; i++)
        {
            SpawnResource(rockPrefabs, ResourceType.Rock);
        }
        
        Debug.Log($"[{zoneName}] Spawned {treeCount} trees and {rockCount} rocks");
    }

    void SpawnResource(GameObject[] prefabs, ResourceType type)
    {
        if (prefabs == null || prefabs.Length == 0)
        {
            Debug.LogWarning($"No prefabs set for {type}!");
            return;
        }

        Vector2 spawnPos = GetRandomSpawnPosition();
        
        if (spawnPos == Vector2.zero)
        {
            Debug.LogWarning($"Could not find valid spawn position for {type}");
            return;
        }

        // Pick random prefab
        GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
        
        // Spawn it
        GameObject instance = Instantiate(prefab, new Vector3(spawnPos.x, spawnPos.y, 0), Quaternion.identity);
        instance.transform.SetParent(transform); // Organize under zone
        instance.name = $"{type}_{allSpawnedResources.Count}";
        
        // Track it
        SpawnedResource resource = new SpawnedResource
        {
            gameObject = instance,
            type = type,
            spawnPosition = spawnPos,
            isDestroyed = false,
            spawnedOnDay = currentDay
        };
        
        allSpawnedResources.Add(resource);
        
        // Add tracker component
        ResourceTracker tracker = instance.AddComponent<ResourceTracker>();
        tracker.spawnZone = this;
        tracker.resource = resource;
    }

    Vector2 GetRandomSpawnPosition()
    {
        Bounds bounds = spawnZone.bounds;
        
        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            // Random position in zone
            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomY = Random.Range(bounds.min.y, bounds.max.y);
            Vector2 position = new Vector2(randomX, randomY);
            
            // Check if too close to existing resources
            bool tooClose = false;
            foreach (var resource in allSpawnedResources)
            {
                if (!resource.isDestroyed && resource.gameObject != null)
                {
                    float distance = Vector2.Distance(position, resource.spawnPosition);
                    if (distance < minDistanceBetweenResources)
                    {
                        tooClose = true;
                        break;
                    }
                }
            }
            
            if (!tooClose)
            {
                return position;
            }
        }
        
        return Vector2.zero; // Failed to find position
    }

    void RegenerateDestroyedResources()
    {
        int treesRegenerated = 0;
        int rocksRegenerated = 0;
        
        List<SpawnedResource> resourcesToRemove = new List<SpawnedResource>();
        
        foreach (var resource in allSpawnedResources)
        {
            // If destroyed, respawn it in new location
            if (resource.isDestroyed || resource.gameObject == null)
            {
                // Spawn new one
                if (resource.type == ResourceType.Tree)
                {
                    SpawnResource(treePrefabs, ResourceType.Tree);
                    treesRegenerated++;
                }
                else if (resource.type == ResourceType.Rock)
                {
                    SpawnResource(rockPrefabs, ResourceType.Rock);
                    rocksRegenerated++;
                }
                
                resourcesToRemove.Add(resource);
            }
        }
        
        // Clean up destroyed resource entries
        foreach (var resource in resourcesToRemove)
        {
            allSpawnedResources.Remove(resource);
        }
        
        if (treesRegenerated > 0 || rocksRegenerated > 0)
        {
            Debug.Log($"[{zoneName}] Regenerated {treesRegenerated} trees and {rocksRegenerated} rocks");
        }
    }

    // Called by ResourceTracker when resource is destroyed
    public void NotifyResourceDestroyed(SpawnedResource resource)
    {
        resource.isDestroyed = true;
        resource.destroyedOnDay = currentDay;
        Debug.Log($"[{zoneName}] {resource.type} destroyed on day {currentDay}");
    }

    [ContextMenu("Clear All Resources")]
    void ClearAllResources()
    {
        foreach (var resource in allSpawnedResources)
        {
            if (resource.gameObject != null)
            {
                DestroyImmediate(resource.gameObject);
            }
        }
        allSpawnedResources.Clear();
        Debug.Log($"[{zoneName}] All resources cleared");
    }

    void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        if (spawnZone == null)
            spawnZone = GetComponent<BoxCollider2D>();
        
        if (spawnZone == null) return;
        
        // Draw zone area
        Gizmos.color = zoneColor;
        Gizmos.DrawCube(spawnZone.bounds.center, spawnZone.bounds.size);
        
        // Draw border
        Gizmos.color = new Color(zoneColor.r, zoneColor.g, zoneColor.b, 1f);
        DrawBorder(spawnZone.bounds);
    }

    void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        if (spawnZone == null)
            spawnZone = GetComponent<BoxCollider2D>();
        
        if (spawnZone == null) return;
        
        // Draw label
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.green;
        style.fontSize = 12;
        style.fontStyle = FontStyle.Bold;
        
        string label = $"{zoneName}\n";
        label += $"Trees: {minTrees}-{maxTrees}\n";
        label += $"Rocks: {minRocks}-{maxRocks}\n";
        
        if (Application.isPlaying)
        {
            int activeResources = 0;
            foreach (var r in allSpawnedResources)
            {
                if (!r.isDestroyed && r.gameObject != null)
                    activeResources++;
            }
            label += $"Active: {activeResources}";
        }
        
        UnityEditor.Handles.Label(
            spawnZone.bounds.center + Vector3.up * (spawnZone.bounds.size.y / 2 + 1f),
            label,
            style
        );
#endif
    }

    void DrawBorder(Bounds bounds)
    {
        Vector3 topLeft = new Vector3(bounds.min.x, bounds.max.y, 0);
        Vector3 topRight = new Vector3(bounds.max.x, bounds.max.y, 0);
        Vector3 bottomLeft = new Vector3(bounds.min.x, bounds.min.y, 0);
        Vector3 bottomRight = new Vector3(bounds.max.x, bounds.min.y, 0);
        
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }
}

[System.Serializable]
public class SpawnedResource
{
    public GameObject gameObject;
    public ResourceType type;
    public Vector2 spawnPosition;
    public bool isDestroyed;
    public int spawnedOnDay;
    public int destroyedOnDay;
}

public enum ResourceType
{
    Tree,
    Rock
}