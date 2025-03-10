using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HistoryEvent : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI eventText;
    [SerializeField] public Button eventButton;

    private System.Action currentAction;

    public void SetHistory(string historyText, System.Action onButtonClick = null)
    {
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
