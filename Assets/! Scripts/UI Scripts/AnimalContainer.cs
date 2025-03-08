using UnityEngine;

public class AnimalContainer : MonoBehaviour
{
    public bool isAnimalOverHeadUIEnabled = false;

    public static AnimalContainer Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void ToggleAnimalOverHeadUI(bool state)
    {
        isAnimalOverHeadUIEnabled = state;

        foreach (Transform child in transform)
        {
            Animal childAnimalScript = child.GetComponent<Animal>();
            if (childAnimalScript != null)
            {
                childAnimalScript.ToggleUI(state);
            }
        }

    }
}
