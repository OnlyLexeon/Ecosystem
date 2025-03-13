using UnityEngine;

public class WorldStatsUI : MonoBehaviour
{
    [Header("References")]
    public GameObject worldStatsPrefab;
    public Transform worldStatsPanel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        KillChildren();

        LoadWorldStats();

        ResizePanel();
    }

    //WORLD STATS
    public void LoadWorldStats()
    { 
        foreach (AnimalWorldStatsEntry entry in WorldStats.Instance.animalStatsList)
        {
            GameObject worldStatsEntry = Instantiate(worldStatsPrefab, worldStatsPanel);
            WorldStatsEntry entryScript = worldStatsEntry.GetComponent<WorldStatsEntry>();

            entryScript.UpdateEntry(entry.animalType, entry.stats.count, entry.stats.totalGenes, entry.stats.positiveGenes, entry.stats.negativeGenes, entry.stats.generation);
        }
    }

    public void KillChildren()
    {
        foreach (Transform child in worldStatsPanel)
        {
            Destroy(child.gameObject);
        }
    }

    void ResizePanel()
    {
        worldStatsPanel.gameObject.SetActive(false);
        worldStatsPanel.gameObject.SetActive(true);
    }
}
