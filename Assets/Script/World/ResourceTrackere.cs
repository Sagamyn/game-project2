using UnityEngine;

/// <summary>
/// Automatically added to spawned resources
/// Notifies spawn zone when resource is destroyed
/// </summary>
public class ResourceTracker : MonoBehaviour
{
    [HideInInspector]
    public ResourceSpawnZone spawnZone;
    
    [HideInInspector]
    public SpawnedResource resource;

    void OnDestroy()
    {
        // Notify spawn zone that this resource was destroyed
        if (spawnZone != null && resource != null)
        {
            spawnZone.NotifyResourceDestroyed(resource);
        }
    }
}