using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartButton : MonoBehaviour
{
    private float velocidad = 1f;

    public Slider sliderDificultad;
    // Start is called before the first frame update
    void Start()
    {
        Dificultad.Instance.dificultad = velocidad;
        sliderDificultad?.onValueChanged.AddListener(CrearDificultad);
    }

    public void Iniciar()
    {
        SceneManager.LoadScene("Juego");
    }

    private void CrearDificultad(float valor)
    {
        switch (valor)
        {
            case 1:
                velocidad = 1f;
                break;
            case 2:
                velocidad = 0.66f;
                break;
            case 3:
                velocidad = 0.50f;
                break;
            case 4:
                velocidad = 0.25f;
                break;
        }
        Dificultad.Instance.dificultad = velocidad;
    }

}
