using UnityEngine;

public class RandomScale : MonoBehaviour
{
    [SerializeField] private float minScale = 1f;
    [SerializeField] private float maxScale = 1.2f;

    void Start()
    {
        // Generate a random scale within the specified range
        float randomScale = Random.Range(minScale, maxScale);
        transform.localScale = new Vector3(randomScale, randomScale, randomScale);
    }
}
