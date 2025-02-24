using UnityEngine;

public enum RabbitTypes
{
    White,
    Gray,
    Orange,
    LightGray,
    Brown,
    Beige,
}

public class Rabbit : MonoBehaviour
{
    public GameObject white;
    public GameObject gray;
    public GameObject orange;
    public GameObject lightgray;
    public GameObject brown;
    public GameObject beige;

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
