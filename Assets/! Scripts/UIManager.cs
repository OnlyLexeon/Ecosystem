using UnityEngine;
using TMPro;
using UnityEngine.UI;
using static Stats;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI targetText;
    public TextMeshProUGUI targetAction;
    public TextMeshProUGUI targetAge;
    public TextMeshProUGUI targetGender;

    public GameObject followControls;
    public GameObject freeRoamControls;

    [Header("Family")]
    public Button motherButton;
    public Button fatherButton;
    public Transform childrenButtonsPanel;
    public GameObject childrenButtonsPrefab;

    [Header("Sliders")]
    public GameObject slidersPanel;
    public Slider healthSlider;
    public Slider hungerSlider;
    public Slider thirstSlider;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI hungerText;
    public TextMeshProUGUI thirstText;
    public GameObject personalityPrefab;
    public Transform personalityPanel;

    [Header("Stats")]
    public GameObject statsPanel;
    public TextMeshProUGUI detectionRange;
    public TextMeshProUGUI detectionRadius;
    public TextMeshProUGUI baseSpeed;
    public TextMeshProUGUI runSpeed;
    public TextMeshProUGUI wanderInterval;
    public TextMeshProUGUI wanderMin;
    public TextMeshProUGUI wanderMax;
    public TextMeshProUGUI hungerDepletion;
    public TextMeshProUGUI thirstDepletion;
    public TextMeshProUGUI eatPerSec;
    public TextMeshProUGUI thirstPerSec;
    public TextMeshProUGUI needsInterval;

    [Header("Rabbit Exclusive")]
    public TextMeshProUGUI lookInverval;
    public TextMeshProUGUI lookMinAngle;
    public TextMeshProUGUI lookMaxAngle;
    public TextMeshProUGUI waitInBurrowTime;

    [Header("Seggs")]
    public TextMeshProUGUI baseOffSpring;
    public TextMeshProUGUI maxAdditionalOffSpring;
    public TextMeshProUGUI minPositiveGenes;
    public TextMeshProUGUI maxNegativeGenes;
    public TextMeshProUGUI reproduceCooldownDays;
    public TextMeshProUGUI reproduceDaysLeft;

    [Header("Sleep")]
    public TextMeshProUGUI additionalSleep;

    [Header("Time")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI dayNightText;

    [Header("References")]
    public InputHandler cameraScript;
    public DayNightManager timeScript;

    public static UIManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        UpdateTargetStats();

        UpdateTime();
    }

    public void ShowControls(bool isFollowing)
    {
        if (followControls && freeRoamControls)
        {
            if (isFollowing)
            {
                followControls.SetActive(true);
                freeRoamControls.SetActive(false);
            }
            else
            {
                followControls.SetActive(false);
                freeRoamControls.SetActive(true);
            }

        }
    }

    //Stats = changing variables 
    public void UpdateTargetStats()
    {
        if (cameraScript.target != null && statsPanel.activeSelf)
        {
            Stats statScript = cameraScript.target.GetComponent<Stats>();

            if (statScript != null)
            {
                healthSlider.maxValue = statScript.maxHealth;
                hungerSlider.maxValue = statScript.maxHunger;
                thirstSlider.maxValue = statScript.maxThirst;

                healthSlider.value = statScript.health;
                hungerSlider.value = statScript.hunger;
                thirstSlider.value = statScript.thirst;

                healthText.text = statScript.health.ToString("F2") + "/" + statScript.maxHealth.ToString("F2");
                hungerText.text = statScript.hunger.ToString("F2") + "/" + statScript.maxHunger.ToString("F2");
                thirstText.text = statScript.thirst.ToString("F2") + "/" + statScript.maxThirst.ToString("F2");

                targetAge.text = "Age (Days): " + statScript.agedDays;
                targetGender.text = "Gender: " + statScript.gender.ToString();
            }
            else Debug.LogWarning("No Stats script found!");

            // UPDATING ACTION TEXT
            Rabbit rabbitScript = cameraScript.target.GetComponent<Rabbit>();
            if (rabbitScript != null) targetAction.text = "Action: " + rabbitScript.currentState.ToString();
        }
    }

    //target = personality or target type
    public void UpdateTargetUI()
    {
        foreach (Transform child in personalityPanel.transform)
        {
            Destroy(child.gameObject);
        }

        if (cameraScript.target != null)
        {
            slidersPanel.SetActive(true);
            statsPanel.SetActive(true);

            string targetName = cameraScript.target.gameObject.layer == LayerMask.NameToLayer("Wolf") ? "Wolf" :
                                cameraScript.target.gameObject.layer == LayerMask.NameToLayer("Rabbit") ? "Rabbit" :
                                "Unknown";

            targetText.text = "Target: " + targetName;

            Stats targetStats = cameraScript.target.GetComponent<Stats>();
            if (targetStats)
            {
                //Personality
                List<Genes> genes = targetStats.genes;
                foreach (Genes gene in genes)
                {
                    GameObject newPersonality = Instantiate(personalityPrefab, personalityPanel);
                    PersonalityButton buttonScript = newPersonality.GetComponent<PersonalityButton>();

                    if (buttonScript)
                    {
                        buttonScript.nameText.text = gene.name;
                        buttonScript.descriptionText.text = gene.description;
                        buttonScript.positivity = gene.positivity;
                    }
                }

                //STATS
                hungerDepletion.text = "Hunger Depletion Rate: " + targetStats.hungerDepletionRate.ToString();
                thirstDepletion.text = "Thirst Depletion Rate: " + targetStats.thirstDepletionRate.ToString();

                Rabbit rabbitScript = cameraScript.target.GetComponent<Rabbit>();
                if (rabbitScript != null)
                {
                    UpdateRabbitStats(rabbitScript);
                }

                //Family Buttons
                PopulateFamilyUI(cameraScript.target);
            }
        }
        else
        {
            slidersPanel.SetActive(false);
            statsPanel.SetActive(false);

            targetText.text = "Target: None";
            targetAction.text = "Action: -";
            targetAge.text = "Age: -";
            targetGender.text = "Gender: -";
        }
    }

    public void UpdateTime()
    {
        float time = timeScript.time;
        int minutes = Mathf.FloorToInt(time % 60);
        int hours = Mathf.FloorToInt(time / 60);

        timeText.text = hours.ToString("D2") + ":" + minutes.ToString("D2");

        if (DayNightManager.Instance.isDay)
            dayNightText.text = "DayTime " + "| "  + "Day " + DayNightManager.Instance.dayNumber;
        else dayNightText.text = "NightTime "+ "| " + "Day " + +DayNightManager.Instance.dayNumber;
    }

    public void UpdateRabbitStats(Rabbit rabbitScript)
    {
        detectionRange.text = "Detection Range: " + rabbitScript.stats.detectionDistance.ToString();
        detectionRadius.text = "Detection Angle: " + rabbitScript.stats.detectionAngle.ToString();

        baseSpeed.text = "Base Speed: " + rabbitScript.stats.baseSpeed.ToString();
        runSpeed.text = "Run Speed: " + rabbitScript.stats.runSpeed.ToString();
        wanderInterval.text = "Wander Interval: " + rabbitScript.stats.wanderInterval.ToString();
        wanderMin.text = "Wander Min Distance: " + rabbitScript.stats.wanderDistanceMin.ToString();
        wanderMax.text = "Wander Max Distance: " + rabbitScript.stats.wanderDistanceMax.ToString();

        eatPerSec.text = "Eat/Second: " + rabbitScript.stats.foodEatPerSecond.ToString();
        thirstPerSec.text = "Drink/Second: " + rabbitScript.stats.drinkPerSecond.ToString();
        needsInterval.text = "Needs Deplete Interval: " + rabbitScript.stats.needsInterval.ToString();

        baseOffSpring.text = "Base Offspring Count: " + rabbitScript.stats.baseOffSpringCount.ToString();
        maxAdditionalOffSpring.text = "Max Random Extra OffSpring: " + rabbitScript.stats.maxAdditionalOffSpring.ToString();
        minPositiveGenes.text = "Min Positive Genes: " + rabbitScript.stats.minPositiveGenesPrefered.ToString();
        maxNegativeGenes.text = "Max Negative Genes: " + rabbitScript.stats.maxNegativeGenesPrefered.ToString();
        reproduceCooldownDays.text = "Reproduce Cooldown (Days): " + rabbitScript.stats.reproduceCooldownDays.ToString();
        reproduceDaysLeft.text = "Reproduce Ready (Days Left): " + rabbitScript.stats.reproduceDaysLeft.ToString();

        lookInverval.text = "Look While Eat/Drink Interval: " + rabbitScript.stats.lookWhileEatingInterval.ToString();
        lookMinAngle.text = "Look Min Angle: " + rabbitScript.stats.lookAngleMin.ToString();
        lookMaxAngle.text = "Look Max Angle: " + rabbitScript.stats.lookAngleMax.ToString();

        additionalSleep.text = "Additional Sleep Hours: " + rabbitScript.stats.additionalSleepHours.ToString();

        waitInBurrowTime.text = "Wait in Burrow after Chase Time: " + rabbitScript.stats.waitBeforeLeavingBurrow.ToString();
    }

    public void PopulateFamilyUI(Transform target)
    {
        Rabbit rabbitScript = target.GetComponent<Rabbit>();
        if (rabbitScript != null)
        {

            // Set Mother Button
            if (rabbitScript.mother != null)
            {
                motherButton.gameObject.SetActive(true);
                motherButton.onClick.RemoveAllListeners();
                motherButton.onClick.AddListener(() => InputHandler.Instance.SetTarget(rabbitScript.mother.transform));
            }
            else
            {
                motherButton.gameObject.SetActive(false);
            }

            // Set Father Button
            if (rabbitScript.father != null)
            {
                fatherButton.gameObject.SetActive(true);
                fatherButton.onClick.RemoveAllListeners();
                fatherButton.onClick.AddListener(() => InputHandler.Instance.SetTarget(rabbitScript.father.transform));
            }
            else
            {
                fatherButton.gameObject.SetActive(false);
            }

            // Clear previous children buttons
            foreach (Transform child in childrenButtonsPanel)
            {
                Destroy(child.gameObject);
            }

            if (rabbitScript.children.Count > 0)
            {
                childrenButtonsPanel.gameObject.SetActive(true);
                // Populate Children Buttons
                for (int i = 0; i < rabbitScript.children.Count + 1; i++)
                {
                    GameObject childButtonObj = Instantiate(childrenButtonsPrefab, childrenButtonsPanel);
                    Button childButton = childButtonObj.GetComponent<Button>();
                    TMP_Text buttonText = childButtonObj.GetComponentInChildren<TMP_Text>();

                    buttonText.text = $"Child {i + 1}";
                    childButton.onClick.AddListener(() => InputHandler.Instance.SetTarget(rabbitScript.children[i].transform));
                }
            }
            else
            {
                childrenButtonsPanel.gameObject.SetActive(false);
            }
            
        }
        
    }
}
