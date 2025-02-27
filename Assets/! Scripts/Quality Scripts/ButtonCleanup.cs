using UnityEngine;
using UnityEngine.UI;

public class ButtonCleanup : MonoBehaviour
{
    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
    }

    void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            Debug.Log("Cleaned Up Listener");
        }
    }
}
