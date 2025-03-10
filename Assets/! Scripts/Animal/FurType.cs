using UnityEngine;

[CreateAssetMenu(fileName = "NewFurType", menuName = "Animals/Fur Type")]
public class FurType : ScriptableObject
{
    public string furName;   // e.g., White, Black, Beige
    public GameObject model; // The model associated with this fur

    public override string ToString()
    {
        return furName;
    }

    private void OnValidate()
    {
        name = furName; // Ensures the asset name matches `furName`
    }
}
