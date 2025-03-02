using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DayNightManager : MonoBehaviour
{
    public static DayNightManager Instance { get; private set; }

    [Header("Lighting Settings")]
    public Light sunLight;
    public Material skyboxMaterial;

    [Header("Time Control")]
    public TextMeshProUGUI speedText;
    public float currentTimeSpeed = 1f;
    public float timeIncrement = 0.5f;
    public float maxTimeScale = 10f;
    public float minTimeScale = 0f;

    [Header("Time Settings")]
    public float time = 720f; // Starts at noon (12:00 PM)
    public int dayNumber = 1;
    private const float dayDuration = 1440f; // 24 hours * 60 minutes

    [Header("Day/Night")]
    public bool isDay;
    public bool isNight;
    public float dayTime = 360f;
    public float nightTime = 1200f;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
        time += Time.deltaTime;
        if (time >= dayDuration)
        {
            time = 0f;
            dayNumber++;

            //New Day
            System.GC.Collect();
        }

        //Set day/night bools
        isDay = time >= 360f && time < 1200f;
        isNight = !isDay;

        UpdateLighting();
        UpdateSkybox();
    }

    void UpdateLighting()
    {
        // Rotate Sun (90° = directly above at noon, -90° at midnight)
        float sunAngle = Mathf.Lerp(-90f, 270f, time / dayDuration);
        sunLight.transform.rotation = Quaternion.Euler(sunAngle, 0, 0);

        // Adjust light intensity (brighter during the day, dim at night)
        float lightIntensity = Mathf.Clamp01(Mathf.Sin((time / dayDuration) * Mathf.PI));
        sunLight.intensity = Mathf.Lerp(0.1f, 1f, lightIntensity);

        // Adjust ambient lighting
        RenderSettings.ambientIntensity = sunLight.intensity * 0.6f;
    }

    void UpdateSkybox()
    {
        if (skyboxMaterial == null) return;

        // Atmosphere thickness control based on time (thicker at sunrise & sunset)
        float atmosphereThickness = Mathf.Lerp(0.3f, 1.2f, Mathf.Sin((time / dayDuration) * Mathf.PI));
        skyboxMaterial.SetFloat("_AtmosphereThickness", atmosphereThickness);

        // Adjust exposure based on time (brighter at noon, dim at midnight)
        float exposure = Mathf.Lerp(0.25f, 1.25f, Mathf.Sin((time / dayDuration) * Mathf.PI));
        skyboxMaterial.SetFloat("_Exposure", exposure);
    }

    public void UpSpeed() //for buttons
    {
        currentTimeSpeed = Mathf.Min(currentTimeSpeed + timeIncrement, maxTimeScale);
        Time.timeScale = currentTimeSpeed;
        UpdateSpeedText();
    }
    public void DownSpeed() //for buttons
    {
        currentTimeSpeed = Mathf.Max(currentTimeSpeed - timeIncrement, minTimeScale);
        Time.timeScale = currentTimeSpeed;
        UpdateSpeedText();
    }
    public void UpSpeed(float value)
    {
        currentTimeSpeed = Mathf.Clamp(currentTimeSpeed + value, minTimeScale, maxTimeScale);
        Time.timeScale = currentTimeSpeed;
        UpdateSpeedText();
    }
    public void DownSpeed(float value)
    {
        currentTimeSpeed = Mathf.Clamp(currentTimeSpeed - value, minTimeScale, maxTimeScale);
        Time.timeScale = currentTimeSpeed;
        UpdateSpeedText();
    }

    public void UpdateSpeedText()
    {
        if (Time.timeScale == 0)
        {
            speedText.text = "Paused";
            speedText.fontSize = 20;
        }
        else
        {
            speedText.text = "x" + Time.timeScale.ToString("F1");
            speedText.fontSize = 26;
        }
    }
    public void SetSpeed(float value)
    {
        currentTimeSpeed = 1;
        Time.timeScale = currentTimeSpeed;
        UpdateSpeedText();
    }

    public string GetTimeString()
    {
        int minutes = Mathf.FloorToInt(time % 60);
        int hours = Mathf.FloorToInt(time / 60);

        string timeString = "";
        timeString = hours.ToString("D2") + ":" + minutes.ToString("D2");

        return timeString;
    }
}
