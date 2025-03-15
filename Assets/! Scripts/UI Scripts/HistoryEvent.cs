using UnityEngine;
using TMPro;
using UnityEngine.UI;

public enum HistoryType
{
    Default,
    Death,
    Mating,
    Birth,
    Mutation,
}

public class HistoryEvent : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI eventText;
    [SerializeField] public Button eventButton;
    [SerializeField] public HistoryType historyType;

    private System.Action currentAction;

    public void SetHistory(string historyText, System.Action onButtonClick = null, HistoryType type = HistoryType.Default)
    {
        historyType = type;
        eventText.text = historyText;

        // Avoid RemoveAllListeners() and use a local reference instead
        if (currentAction != null)
        {
            eventButton.onClick.RemoveListener(currentAction.Invoke);
        }

        currentAction = onButtonClick;

        if (currentAction != null)
        {
            eventButton.onClick.AddListener(currentAction.Invoke);
        }
        else // is null
        {
            eventButton.interactable = false;
            eventButton.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (currentAction != null)
        {
            eventButton.onClick.RemoveListener(currentAction.Invoke);
            Debug.LogWarning("Listener Removed");
        }
    }
}
