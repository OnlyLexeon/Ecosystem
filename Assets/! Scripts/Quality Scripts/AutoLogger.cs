using System.Collections;
using System.IO;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;

public class PerformanceLogger : MonoBehaviour
{
    public float logInterval = 5f;
    private string filePath;
    private StreamWriter writer;
    private float timer = 0f;

    private int frameCount = 0;
    private float elapsedTime = 0f;
    private float fps = 0f;

    private ProfilerRecorder mainThreadRecorder;

    void Start()
    {
        string folderPath = Application.dataPath + "/PerformanceLogs";
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        filePath = folderPath + "/PerformanceLog_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
        writer = new StreamWriter(filePath);
        writer.WriteLine("Time (s),FPS,CPU Usage (%) (MainThreadTime),Total Allocated Memory (MB),Reserved Memory (MB),Mono Memory (MB)");

        // Start recording Main Thread Time
        mainThreadRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 1000);
    }

    void Update()
    {
        frameCount++;
        elapsedTime += Time.deltaTime;

        if (elapsedTime >= 1f)
        {
            fps = frameCount / elapsedTime;
            frameCount = 0;
            elapsedTime = 0f;
        }

        timer += Time.deltaTime;
        if (timer >= logInterval)
        {
            LogPerformance();
            timer = 0f;
        }
    }

    void LogPerformance()
    {
        float timeInSeconds = Time.time;

        // Memory stats (converted to MB)
        float totalAllocated = Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f);
        float totalReserved = Profiler.GetTotalReservedMemoryLong() / (1024f * 1024f);
        float monoMemory = Profiler.GetMonoUsedSizeLong() / (1024f * 1024f);

        // Get Main Thread Time (nanoseconds)
        float mainThreadTimeMs = 0f;
        if (mainThreadRecorder.Count > 0)
        {
            mainThreadTimeMs = mainThreadRecorder.LastValue / 1000000f; // Convert from ns to ms
        }

        // Estimate CPU Usage
        float cpuUsagePercent = Mathf.Clamp01(mainThreadTimeMs / (1000f / fps)) * 100f; // 1000 ms in 1 second

        writer.WriteLine($"{timeInSeconds:F2},{fps:F2},{cpuUsagePercent:F2},{totalAllocated:F2},{totalReserved:F2},{monoMemory:F2}");
        writer.Flush();
    }

    void OnApplicationQuit()
    {
        mainThreadRecorder.Dispose();

        if (writer != null)
        {
            writer.Close();
            Debug.Log("Performance logging finished. File saved to: " + filePath);
        }
    }
}
