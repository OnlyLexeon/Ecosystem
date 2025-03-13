using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TerminationConditionUI : MonoBehaviour
{
    [Header("References")]
    public GameObject contentPanel;

    [Header("Dropdowns")]
    public TMP_Dropdown conditionDropdown;
    public TMP_Dropdown animalTypeDropdown;

    [Header("Conditional Input Field")]
    public GameObject intValuePanel; // Panel containing input field
    public TMP_InputField intValueInput;
    public TextMeshProUGUI errorText;

    private TerminationCondition currentCondition = new TerminationCondition();
    private List<AnimalType> availableAnimalTypes;

    private void Start()
    {
        // Populate condition dropdown
        conditionDropdown.ClearOptions();
        conditionDropdown.AddOptions(new List<string>(System.Enum.GetNames(typeof(TerminationConditionType))));
        conditionDropdown.onValueChanged.AddListener(OnConditionChanged);

        // Populate animal type dropdown
        availableAnimalTypes = LoadAnimalTypes();
        animalTypeDropdown.ClearOptions();
        animalTypeDropdown.AddOptions(availableAnimalTypes.ConvertAll(a => a.animalName));
        animalTypeDropdown.onValueChanged.AddListener(OnAnimalTypeChanged);

        errorText.gameObject.SetActive(false);
        intValuePanel.SetActive(false);
    }

    private void OnEnable()
    {
        errorText.gameObject.SetActive(false);

        LoadTerminationConditionFromManager();
    }

    //TERMINATION CONDITION
    void LoadTerminationConditionFromManager()
    {
        if (TerminationManager.Instance != null && TerminationManager.Instance.terminationCondition != null)
        {
            // Create a new instance instead of assigning reference
            TerminationCondition managerCondition = TerminationManager.Instance.terminationCondition;
            currentCondition = new TerminationCondition
            {
                conditionType = managerCondition.conditionType,
                targetAnimalType = managerCondition.targetAnimalType,
                generationReachedThreshold = managerCondition.generationReachedThreshold,
                numberOfAnimalsThreshold = managerCondition.numberOfAnimalsThreshold
            };

            // Set dropdown values
            conditionDropdown.value = (int)currentCondition.conditionType;
            if (managerCondition.targetAnimalType != null) animalTypeDropdown.value = availableAnimalTypes.FindIndex(a => a == currentCondition.targetAnimalType);
            
            // Set int value input if needed
            if (currentCondition.conditionType == TerminationConditionType.GenerationReached_ ||
                currentCondition.conditionType == TerminationConditionType.NumberOfAnimals_)
            {
                intValuePanel.SetActive(true);
                intValueInput.text = currentCondition.conditionType == TerminationConditionType.GenerationReached_
                    ? currentCondition.generationReachedThreshold.ToString()
                    : currentCondition.numberOfAnimalsThreshold.ToString();
            }
            else
            {
                intValuePanel.SetActive(false);
            }
        }
    }

    void OnConditionChanged(int index)
    {
        TerminationConditionType selectedType = (TerminationConditionType)index;
        currentCondition.conditionType = selectedType;

        // Show or hide int input field based on condition
        bool needsIntValue = selectedType == TerminationConditionType.GenerationReached_ || selectedType == TerminationConditionType.NumberOfAnimals_;
        intValuePanel.SetActive(needsIntValue);

        ResizePanel();
    }
    void OnAnimalTypeChanged(int index)
    {
        currentCondition.targetAnimalType = availableAnimalTypes[index];
    }
    private bool ValidateIntInput()
    {
        if (int.TryParse(intValueInput.text, out int value) && value > 0)
        {
            errorText.gameObject.SetActive(false);
            if (currentCondition.conditionType == TerminationConditionType.GenerationReached_)
                currentCondition.generationReachedThreshold = value;
            else if (currentCondition.conditionType == TerminationConditionType.NumberOfAnimals_)
                currentCondition.numberOfAnimalsThreshold = value;
            return true;
        }
        else
        {
            errorText.text = "Please enter a valid positive integer.";
            errorText.gameObject.SetActive(true);
            return false;
        }
    }
    private List<AnimalType> LoadAnimalTypes()
    {
        return new List<AnimalType>(Resources.LoadAll<AnimalType>("AnimalTypes")); // Load from Resources folder
    }
    public void ApplyChangesToTerminationManager()
    {
        TextMeshProUGUI text = errorText.GetComponent<TextMeshProUGUI>();

        if (TerminationManager.Instance != null)
        {
            if (currentCondition.conditionType == TerminationConditionType.GenerationReached_ || currentCondition.conditionType == TerminationConditionType.NumberOfAnimals_)
            {
                if (!ValidateIntInput())
                {
                    errorText.gameObject.SetActive(true);
                    text.color = Color.red;
                    text.text = "Incorrect Value input!";

                    return; // Stop if validation fails
                }
            }

            errorText.gameObject.SetActive(true);
            text.color = Color.green;
            text.text = "Success!";

            TerminationManager.Instance.terminationCondition = currentCondition;
            Debug.Log("Termination condition updated.");
        }
        else
        {
            Debug.LogError("TerminationManager instance not found!");
        }
    }

    void ResizePanel()
    {
        contentPanel.SetActive(false);
        contentPanel.SetActive(true);
    }
}
