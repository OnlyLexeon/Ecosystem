using System;
using UnityEngine;

public enum RabbitTypes
{
    White,
    Beige,
    Brown,
    Gray,
    LightGray,
    Orange,
}

public class Rabbit : MonoBehaviour
{
    [Header("Models")]
    public GameObject white;
    public GameObject beige;
    public GameObject brown;
    public GameObject gray;
    public GameObject lightgray;
    public GameObject orange;

    [Header("Prefabs")]
    public GameObject rabbitPrefab;

    public static Rabbit Instance;

    private void Awake()
    {
        Instance = this;

        // Ensure rabbitPrefab is loaded only ONCE
        if (rabbitPrefab == null)
        {
            rabbitPrefab = Resources.Load<GameObject>("Prefabs/Rabbit"); // Adjust the path
            if (rabbitPrefab) Debug.Log("Rabbit prefab loaded successfully.");
        }
    }

    public GameObject GetRabbitModel(RabbitTypes type)
    {
        switch(type)
        {
            case RabbitTypes.White:
                return white;
            case RabbitTypes.Gray:
                return gray;
            case RabbitTypes.LightGray:
                return lightgray;
            case RabbitTypes.Orange:
                return orange;
            case RabbitTypes.Brown:
                return brown;
            case RabbitTypes.Beige:
                return beige;
            default:
                return white;
        }
    }
}
