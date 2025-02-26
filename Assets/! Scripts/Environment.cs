using UnityEngine;

public enum BushTypes
{
    Empty,
    Ready,
    Full,
}

public class Environment : MonoBehaviour
{
    [Header("Models")]
    public GameObject bushEmpty;
    public GameObject bushReady;
    public GameObject bushFull;

    [Header("Prefabs")]
    public GameObject burrowPrefab;

    public static Environment Instance;

    private void Awake()
    {
        Instance = this;
    }

    public GameObject GetBushModel(BushTypes type)
    {
        switch (type)
        {
            case BushTypes.Empty:
                return bushEmpty;
            case BushTypes.Ready:
                return bushReady;
            case BushTypes.Full:
                return bushFull;
            default:
                return bushReady;
        }
    }
}
