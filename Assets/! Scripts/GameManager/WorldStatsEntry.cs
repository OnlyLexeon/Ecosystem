using TMPro;
using UnityEngine;

public class WorldStatsEntry : MonoBehaviour
{
    public TextMeshProUGUI animalTypeText;
    public TextMeshProUGUI countText;
    public TextMeshProUGUI totalGenesText;
    public TextMeshProUGUI positiveGenesText;
    public TextMeshProUGUI negativeGenesText;
    public TextMeshProUGUI generationText;

    public void UpdateEntry(AnimalType animalType, int count, int totalGenes, int positiveGenes, int negativeGenes, int generation)
    {
        animalTypeText.text = "Animal Type:" + animalType.animalName.ToString();
        countText.text = "Count:" + count.ToString();
        totalGenesText.text = "Total Genes:" + totalGenes.ToString();
        positiveGenesText.text = "Positive Genes:" + positiveGenes.ToString();
        negativeGenesText.text = "Negative Genes:" + negativeGenes.ToString();
        generationText.text = "Highest Generation:" + generation.ToString();
    }
}
