using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System;
using static UnityEngine.EventSystems.EventTrigger;

public class GeneUI : MonoBehaviour
{
    public GameObject GeneMenu;
    public Transform geneDefaultListContainer;
    public Transform geneCustomListContainer;
    public GameObject geneEntryPrefab; // A UI prefab to display genes

    [Header("Inputs")]
    public TMP_InputField nameInput;
    public TMP_InputField descriptionInput;
    public int positivity = 0;
    public Button addModifierButton;
    public Button addGeneButton;

    public TextMeshProUGUI errorText;

    public GameObject dropdownWithInputPrefab;
    public Transform dropdownContainer; // Parent object for dropdowns

    private List<TMP_Dropdown> statModifierDropdowns = new List<TMP_Dropdown>();
    private List<TMP_InputField> valueInputs = new List<TMP_InputField>();

    public static GeneUI Instance;

    private void Start()
    {
        Instance = this;

        addGeneButton.onClick.AddListener(AddGene);
        addModifierButton.onClick.AddListener(CreateNewDropdown);

        DisplayGenes();
    }


    public void SetPositivity(int value)
    {
        positivity = value;
    }

    private void DisplayGenes()
    {
        ResetInputs();

        foreach (Transform child in geneDefaultListContainer)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in geneCustomListContainer)
        {
            Destroy(child.gameObject);
        }

        List<Genes> allDefaultGenes = GeneManager.Instance.GetAllDefaultGenes();
        foreach (Genes gene in allDefaultGenes)
        {
            GameObject entry = Instantiate(geneEntryPrefab, geneDefaultListContainer);

            //Change prefab's UI to display the gene info
            SetPersonalityButtonDisplay(entry, gene);
        }

        List<Genes> allCustomGenes = GeneManager.Instance.GetAllCustomGenes();
        foreach (Genes gene in allCustomGenes)
        {
            GameObject entry = Instantiate(geneEntryPrefab, geneCustomListContainer);

            //Change prefab's UI to display the gene info
            SetPersonalityButtonDisplay(entry, gene);
        }

        // Force Layout Update
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(dropdownContainer as RectTransform);
    }

    public void ResetInputs()
    {
        nameInput.text = string.Empty;
        descriptionInput.text = string.Empty;
        positivity = 0;

        // Clear all dropdowns and input fields
        foreach (var dropdown in statModifierDropdowns)
        {
            Destroy(dropdown.transform.parent.gameObject);
        }
        statModifierDropdowns.Clear();
        valueInputs.Clear();

        // Create a fresh dropdown after clearing
        CreateNewDropdown();
    }


    public void SetPersonalityButtonDisplay(GameObject entry, Genes gene)
    {
        //Change prefab's UI to display the gene info
        PersonalityButton buttonScript = entry.GetComponent<PersonalityButton>();

        if (buttonScript)
        {
            buttonScript.nameText.text = gene.name;
            buttonScript.descriptionText.text = gene.description;
            buttonScript.positivity = gene.positivity;
        }
    }

    private void CreateNewDropdown()
    {
        // Instantiate dropdown + input field + delete button from prefab
        GameObject dropdownObj = Instantiate(dropdownWithInputPrefab, dropdownContainer);

        TMP_Dropdown newDropdown = dropdownObj.GetComponentInChildren<TMP_Dropdown>();
        TMP_InputField valueInput = dropdownObj.GetComponentInChildren<TMP_InputField>();
        Button deleteButton = dropdownObj.GetComponentInChildren<Button>(); // Find the Delete Button in the prefab

        statModifierDropdowns.Add(newDropdown);
        valueInputs.Add(valueInput);

        // Populate dropdown with "None" as the first option
        newDropdown.options.Clear();
        newDropdown.options.Add(new TMP_Dropdown.OptionData("None")); // Default option that does nothing

        foreach (StatType stat in Enum.GetValues(typeof(StatType)))
        {
            newDropdown.options.Add(new TMP_Dropdown.OptionData(stat.ToString()));
        }

        newDropdown.onValueChanged.AddListener(delegate { OnDropdownValueChanged(newDropdown); });

        // Assign delete button functionality
        deleteButton.onClick.AddListener(() => RemoveDropdown(dropdownObj));

        // Force Layout Update
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(dropdownContainer as RectTransform);
    }

    private void OnDropdownValueChanged(TMP_Dropdown changedDropdown)
    {
        int lastIndex = statModifierDropdowns.IndexOf(changedDropdown);

        if (changedDropdown.value == 0) // "None" option selected
        {
            RemoveDropdown(changedDropdown.transform.parent.gameObject);
            return;
        }

        // Remove any dropdowns created after this one
        for (int i = statModifierDropdowns.Count - 1; i > lastIndex; i--)
        {
            Destroy(statModifierDropdowns[i].transform.parent.gameObject);
            statModifierDropdowns.RemoveAt(i);
            valueInputs.RemoveAt(i);
        }
    }

    private void RemoveDropdown(GameObject dropdownObj)
    {
        TMP_Dropdown dropdown = dropdownObj.GetComponentInChildren<TMP_Dropdown>();

        int index = statModifierDropdowns.IndexOf(dropdown);
        if (index >= 0)
        {
            statModifierDropdowns.RemoveAt(index);
            valueInputs.RemoveAt(index);
        }

        Destroy(dropdownObj);
    }

    private List<StatModifier> GetSelectedStatModifiers()
    {
        List<StatModifier> modifiers = new List<StatModifier>();

        for (int i = 0; i < statModifierDropdowns.Count; i++)
        {
            TMP_Dropdown dropdown = statModifierDropdowns[i];
            TMP_InputField valueInput = valueInputs[i];

            if (Enum.TryParse(dropdown.options[dropdown.value].text, out StatType selectedType))
            {
                float modifierValue = float.TryParse(valueInput.text, out float val) ? val : 0f;
                modifiers.Add(new StatModifier(selectedType, modifierValue));

                //Debug.Log($"Added Modifier: {selectedType} with value {modifierValue}");
            }
            else
            {
                //Debug.LogWarning($"Failed to parse stat type from dropdown: {dropdown.options[dropdown.value].text}");
            }
        }

        //Debug.Log($"Total modifiers collected: {modifiers.Count}");
        return modifiers;
    }


    private void AddGene()
    {
        string name = nameInput.text;
        string description = descriptionInput.text;

        // Check if name is empty
        if (string.IsNullOrEmpty(name))
        {
            errorText.text = "Error: Gene name cannot be empty!";
            return;
        }

        // Check if description is empty
        if (string.IsNullOrEmpty(description))
        {
            errorText.text = "Error: Gene description cannot be empty!";
            return;
        }

        // Check if a gene with the same name already exists
        List<Genes> allCustomGenes = GeneManager.Instance.GetAllCustomGenes();
        foreach (Genes gene in allCustomGenes)
        {
            if (gene.name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                errorText.text = "Error: A custom gene with that name already exists!";
                return;
            }
        }

        List<StatModifier> selectedModifiers = GetSelectedStatModifiers();

        // Check if modifiers are empty
        if (selectedModifiers.Count == 0)
        {
            errorText.text = "Error: At least one stat modifier must be selected!";
            return;
        }

        GeneManager.Instance.AddNewGene(name, description, positivity, selectedModifiers);
        DisplayGenes();

        errorText.text = "";
    }
}
