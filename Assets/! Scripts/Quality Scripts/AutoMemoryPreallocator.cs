using UnityEngine;

public class AutoMemoryPreallocator : MonoBehaviour
{
    [SerializeField] private int gbToAllocate = 1; // Set this in the Inspector
    private byte[] preallocatedMemory;

    void Start()
    {
        try
        {
            int allocationSize = gbToAllocate * 1024 * 1024 * 1024; // Convert GB to bytes
            preallocatedMemory = new byte[allocationSize];

            Debug.Log($"[Success] Preallocated {gbToAllocate}GB of memory at startup.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Failure] Memory allocation failed: {e.Message}");
        }
    }
}
