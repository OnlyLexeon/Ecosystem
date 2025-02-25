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
    public GameObject white;
    public GameObject beige;
    public GameObject brown;
    public GameObject gray;
    public GameObject lightgray;
    public GameObject orange;

    public static Rabbit Instance;

    private void Awake()
    {
        Instance = this;
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
