using UnityEngine;

public class Dificultad : MonoBehaviour
{
    public static Dificultad Instance { get; private set; }

    public float dificultad = 1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
