using UnityEngine;

public class RandomRotation : MonoBehaviour
{
    void Start()
    {
        int randomYRotation = Random.Range(0, 360); // Generate a random angle between 0 and 359
        transform.rotation = Quaternion.Euler(0, randomYRotation, 0);
    }
}
