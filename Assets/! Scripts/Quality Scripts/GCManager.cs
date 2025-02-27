using System;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Scripting;

public class GCManager : MonoBehaviour
{
    const long kCollectAfterAllocating = 16 * 1024 * 1024; // Increased threshold to 16MB
    const long kHighWater = 256 * 1024 * 1024; // Increased high memory limit to 256MB

    long lastFrameMemory = 0;
    long nextCollectAt = 0;
    float lastGCCheckTime = 0f;
    public float gcCheckInterval = 60f; // Check memory usage every 1 second

    void Start()
    {
        GarbageCollector.GCMode = GarbageCollector.Mode.Manual;
        Debug.Log("GC Mode set to Manual");
    }

    void Update()
    {
        if (Time.time - lastGCCheckTime < gcCheckInterval)
            return; // Only check memory usage every 1 second

        lastGCCheckTime = Time.time;

        long mem = Profiler.GetMonoUsedSizeLong();

        if (mem > kHighWater)
        {
            Debug.LogWarning("High Memory Usage! Triggering Full GC.");
            System.GC.Collect(); // Perform full GC
            nextCollectAt = mem + kCollectAfterAllocating;
        }
        else if (mem >= nextCollectAt)
        {
            Debug.Log("Performing Incremental GC");
            GarbageCollector.CollectIncremental();
            nextCollectAt = mem + kCollectAfterAllocating;
        }

        lastFrameMemory = mem;
    }
}
