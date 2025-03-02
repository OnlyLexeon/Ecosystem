using UnityEngine;

public class AutoMemoryPreallocator : MonoBehaviour
{
    [SerializeField] private int gbToAllocate = 1; // Set this in the Inspector
    private byte[] preallocatedMemory;

    void Start()
    {
        int allocationSize = gbToAllocate * 1024 * 1024 * 1024; // Convert GB to bytes
        preallocatedMemory = new byte[allocationSize];

        Debug.Log($"Preallocated {gbToAllocate}GB of memory at startup.");
    }
}
