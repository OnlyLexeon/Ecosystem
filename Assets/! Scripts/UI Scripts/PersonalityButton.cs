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
    public Positivity positivity;
    public Color extremelyPositive;
    public Color positive;
    public Color neutral;
    public Color negative;
    public Color extremelyNegative;

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
            case Positivity.ExtremelyNegative:
                colorImage.color = extremelyNegative;
                break;
            case Positivity.Negative:
                colorImage.color = negative;
                break;
            case Positivity.Neutral:
                colorImage.color = neutral;
                break;
            case Positivity.Positive:
                colorImage.color = positive;
                break;
            case Positivity.ExtremelyPositive:
                colorImage.color = extremelyPositive;
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
