using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class PersonalityButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Elements")]
    [SerializeField] public GameObject tooltip; // Tooltip to show on hover
    [SerializeField] private RectTransform tooltipRect; // RectTransform for precise positioning

    public TextMeshProUGUI nameText; // UI text for personality name
    public TextMeshProUGUI descriptionText; // UI text for personality description
    public int positivity = 1;
    public Color positive;
    public Color neutral;
    public Color negative;

    private RectTransform buttonRect;

    private void Start()
    {
        buttonRect = GetComponent<RectTransform>(); // Get the button's RectTransform
        if (tooltip != null)
        {
            tooltip.SetActive(false); // Hide initially
        }

        Image colorImage = GetComponent<Image>();

        switch (positivity)
        {
            case 0:
                colorImage.color = negative;
                break;
            case 1:
                colorImage.color = neutral;
                break;
            case 2:
                colorImage.color = positive;
                break;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltip != null)
        {
            tooltip.SetActive(true);

            // Ensure pivot is set to the left so the tooltip expands rightward
            tooltipRect.pivot = new Vector2(0f, 0.5f);

            // Align tooltip's left edge to the button's right edge
            Vector3 newPosition = buttonRect.position;
            newPosition.x += buttonRect.rect.width / 2; // Right edge of the button
            newPosition.y = buttonRect.position.y; // Keep vertical position unchanged

            tooltip.transform.position = newPosition;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltip != null)
        {
            tooltip.SetActive(false);
        }
    }
}
