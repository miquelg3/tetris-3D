using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pieza : MonoBehaviour
{
    public GameObject pieza;
    public int columna;
    public int fila;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Pieza(GameObject pieza, int columna, int fila)
    {
        this.pieza = pieza;
        this.columna = columna;
        this.fila = fila;
    }

    public void BajarUnaFila(int numeroLinea)
    {
        Debug.Log($"Eliminando o bajando columna {columna}, fila {fila}");
        if (fila == numeroLinea)
        {
            Destroy(pieza);
        }
        else
        {
            fila--;
            if (pieza != null)
            {
                pieza.transform.position = new Vector3(columna, fila, pieza.transform.position.z);
            }
        }
    }

    public void EliminarPiezaColumna()
    {
        Destroy(pieza);
    }

}
