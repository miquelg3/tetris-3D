using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Tetris : MonoBehaviour
{
    [Range(4, 20)]
    public int columnas = 10;
    [Range(10, 22)]
    public int altura = 20;
    [Range(0,1)]
    public float tiempoMovimiento = 1f;
    public bool profundidad;
    public KeyCode derecha = KeyCode.RightArrow;
    public KeyCode izquierda = KeyCode.LeftArrow;
    public KeyCode abajo = KeyCode.DownArrow;
    public KeyCode transportar = KeyCode.UpArrow;
    public KeyCode rotar = KeyCode.Space;
    public Color[] colores = new Color[4];
    public AudioClip move;
    public AudioClip clear;
    public AudioClip rotate;
    public AudioClip full;
    public delegate void puntuacion(int n);
    public static event puntuacion puntuacionActualizada;
    public Image gameOverImage;
    public GameObject botones;

    
    bool[,] posiciones;
    GameObject[] piezas = new GameObject[4];
    List<Pieza> todasLasPiezas = new List<Pieza>();
    private int colorNum;
    bool encontradaLineaLlena;
    int cantidadLineasLlenas;
    bool moviendo;
    int numPieza;
    bool gameOver;
    int puntuacionPartida;
    int numPiezaEspecial = 1;
    int proximaPieza = -1;
    GameObject[] siguientesPiezas = new GameObject[4];
    int proximoColor;
    System.Random random;
    bool pulsadoDerecha;
    bool pulsadoIzquierda;
    bool pulsadoAbajo;
    bool pulsadoRotar;
    bool pulsandoBajarTodo;
    // Start is called before the first frame update
    void Start()
    {
        random = new System.Random();
        if (columnas < 12) Camera.main.transform.Translate((Vector3.left * (12 - columnas) / 2) - new Vector3(-3, 0, 25));
        if (columnas > 12) Camera.main.transform.Translate((Vector3.right * (columnas - 12) / 2) - new Vector3(-3, 0, 25));

        if (colores[0] == null) colores[0] = Color.blue;
        if (colores[1] == null) colores[1] = Color.yellow;
        if (colores[2] == null) colores[2] = Color.green;
        if (colores[3] == null) colores[3] = Color.red;

        /*Vector3 posicionEnMundo = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, 0, 10f));

        botones.transform.position = posicionEnMundo;*/

        RecibirDificultad();
        for (int x = 0; x < 4; x++)
        {
            piezas[x] = GameObject.CreatePrimitive(PrimitiveType.Cube);
            siguientesPiezas[x] = GameObject.CreatePrimitive(PrimitiveType.Cube);
        }
        posiciones = new bool[columnas, altura];
        // Tablero
        for (int y = altura - 1; y > - 2; y--)
        {
            GameObject cuboTablero = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cuboTablero.transform.position = new Vector3(-1, y, 0);
            GameObject cuboTableroFinal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cuboTableroFinal.transform.position = new Vector3(columnas, y, 0);
            if (y == -1)
            {
                for (int x = -1; x < columnas; x++)
                {
                    GameObject cuboLineaFinal = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cuboLineaFinal.transform.position = new Vector3(x, y, 0);
                }
            }
        }

        // Llamar a una nueva pieza
        SpawnPieza();
    }

    public void RecibirDificultad()
    {
        tiempoMovimiento = Dificultad.Instance.dificultad;
    }

    // Update is called once per frame
    void Update()
    {
        // Para que no traspase la pared ni las demás piezas
        GameObject piezaAbajo = null;
        float posicionMasAbajo = altura;

        // Recoger pieza de cada extremo
        foreach (var pieza in piezas)
        {
            if (pieza.transform.position.y < posicionMasAbajo)
            {
                posicionMasAbajo = pieza.transform.position.y;
                piezaAbajo = pieza;
            }
        }
        // Mover derecha
        if (Input.GetKeyUp(derecha) || pulsadoDerecha && PoderIrDerecha() && !moviendo)
        {
            if (pulsadoDerecha) pulsadoDerecha = false;
            AudioSource.PlayClipAtPoint(move, Camera.main.transform.position);
            foreach (var pieza in piezas)
            {
                pieza.transform.position = new Vector3(pieza.transform.position.x + 1, pieza.transform.position.y, pieza.transform.position.z);
            }
        }
        // Mover a izquierda
        if (Input.GetKeyUp(izquierda) || pulsadoIzquierda && PoderIrIzquierda() && !moviendo)
        {
            if (pulsadoIzquierda) pulsadoIzquierda = false;
            AudioSource.PlayClipAtPoint(move, Camera.main.transform.position);
            foreach (var pieza in piezas)
            {
                pieza.transform.position = new Vector3(pieza.transform.position.x - 1, pieza.transform.position.y, pieza.transform.position.z);
            }
        }
        // Mover abajo
        if (Input.GetKeyUp(abajo) || pulsadoAbajo && PoderIrAbajoTecla(piezaAbajo) && PoderIrAbajo() && !moviendo)
        {
            if (pulsadoAbajo) pulsadoAbajo = false;
            foreach (var pieza in piezas)
            {
                pieza.transform.position = new Vector3(pieza.transform.position.x, pieza.transform.position.y - 1, pieza.transform.position.z);
            }
        }
        // Mover hasta el final
        if (Input.GetKeyUp(transportar) || pulsandoBajarTodo && !moviendo)
        {
            if (pulsandoBajarTodo) pulsandoBajarTodo = false;
            moviendo = true;
            while (PoderIrAbajo())
            {
                foreach (var pieza in piezas)
                {
                    pieza.transform.position = new Vector3(pieza.transform.position.x, pieza.transform.position.y - 1, pieza.transform.position.z);
                }
            }
            moviendo = false;
        }
        // Rotar pieza
        if (Input.GetKeyUp(rotar) || pulsadoRotar && !moviendo && numPieza != 4)
        {
            if (pulsadoRotar) pulsadoRotar = false;
            Vector3[] posicionesAlRotar = RotarPieza();
            if (PoderRotarPieza(posicionesAlRotar))
            {
                AudioSource.PlayClipAtPoint(rotate, Camera.main.transform.position);
                for (int i = 0; i < posicionesAlRotar.Length; i++)
                {
                    piezas[i].transform.position = posicionesAlRotar[i];
                }
            }
        }
    }

    // Crear pieza
    void SpawnPieza()
    {
        // Primera vez que se ejecuta
        if (proximaPieza == -1)
        {
            colorNum = UnityEngine.Random.Range(0, colores.Length);
            numPiezaEspecial = UnityEngine.Random.Range(0, 10);
            if (numPiezaEspecial == 0)
            {
                SpawnSpecial();
            }
            else
            {
                numPieza = UnityEngine.Random.Range(0, 7);
                switch (numPieza)
                {
                    case 0:
                        SpawnS();
                        break;
                    case 1:
                        SpawnI();
                        break;
                    case 2:
                        SpawnL();
                        break;
                    case 3:
                        SpawnT();
                        break;
                    case 4:
                        SpawnO();
                        break;
                    case 5:
                        SpawnLReversed();
                        break;
                    case 6:
                        SpawnSReversed();
                        break;
                }
            }
            proximaPieza = UnityEngine.Random.Range(0, 7);
            proximoColor = UnityEngine.Random.Range(0, colores.Length);
        }
        else
        {
            colorNum = proximoColor;
            numPieza = proximaPieza;
            numPiezaEspecial = UnityEngine.Random.Range(0, 10);
            if (numPiezaEspecial == 0)
            {
                SpawnSpecial();
            }
            else
            {
                switch (numPieza)
                {
                    case 0:
                        SpawnS();
                        break;
                    case 1:
                        SpawnI();
                        break;
                    case 2:
                        SpawnL();
                        break;
                    case 3:
                        SpawnT();
                        break;
                    case 4:
                        SpawnO();
                        break;
                    case 5:
                        SpawnLReversed();
                        break;
                    case 6:
                        SpawnSReversed();
                        break;
                }
            }
            proximaPieza = UnityEngine.Random.Range(0, 7);
            proximoColor = UnityEngine.Random.Range(0, colores.Length);
        }
        MostrarProximasPiezas();
        if (!gameOver)
        {
            foreach (var pieza in piezas)
            {
                pieza.GetComponent<Renderer>().material.color = colores[colorNum];
            }
            StartCoroutine(CaerPieza());
        }
        else
        {
            Debug.Log("GameOver");
            gameOverImage.gameObject.SetActive(true);
        }
    }

    IEnumerator CaerPieza()
    {
        while (PoderIrAbajo())
        {
            encontradaLineaLlena = false;
            moviendo = true;
            foreach (var pieza in piezas)
            {
                pieza.transform.position = new Vector3(pieza.transform.position.x, pieza.transform.position.y - 1, pieza.transform.position.z);
                Debug.Log($"Posición {pieza.transform.position.x}, {pieza.transform.position.y}");
            }
            moviendo = false;
            yield return new WaitForSeconds(tiempoMovimiento);
        }

        // Llega abajo, guarda posición y spawnea nueva pieza
        if (!PoderIrAbajo())
        {
            if (numPiezaEspecial == 0)
            {
                VaciarColumna((int)piezas[0].transform.position.x);
                SpawnPieza();
            }
            else
            {
                foreach (var pieza in piezas)
                {
                    if (pieza.transform.position.y < altura)
                    {
                        Debug.Log($"Posición X: {pieza.transform.position.x}, Posición Y: {pieza.transform.position.y}");
                        posiciones[(int)pieza.transform.position.x, (int)pieza.transform.position.y] = true;
                        GameObject nuevaPosicion = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        nuevaPosicion.transform.position = pieza.transform.position;
                        nuevaPosicion.transform.GetComponent<Renderer>().material.color = colores[colorNum];
                        todasLasPiezas.Add(new Pieza(nuevaPosicion, (int)pieza.transform.position.x, (int)pieza.transform.position.y));
                        Debug.LogWarning($"Posición {pieza.transform.position.x}, {pieza.transform.position.y}");
                    }
                }
                int lineaLlena = 0;
                for (int i = altura - 1; i >= 0; i--)
                {
                    if (LineaLlena(i))
                    {
                        encontradaLineaLlena = true;
                        lineaLlena = i;
                        cantidadLineasLlenas++;
                    }
                }
                if (encontradaLineaLlena)
                {
                    for (int i = 0; i < cantidadLineasLlenas; i++)
                    {
                        AjustarPosiciones(lineaLlena);
                    }
                    AudioSource.PlayClipAtPoint(full, Camera.main.transform.position);
                }
                cantidadLineasLlenas = 0;
                encontradaLineaLlena = false;
                if (!posiciones[columnas / 2, altura - 1] || posiciones[columnas / 2, altura - 2])
                {
                    SpawnPieza();
                }
            }
        }
    }

    bool PoderIrAbajo()
    {
        foreach (var pieza in piezas)
        {
            int x = (int)pieza.transform.position.x;
            int y = (int)pieza.transform.position.y - 1;

            if (y < 0 || posiciones[x, y])
            {
                return false;
            }
        }
        return true;
    }

    bool PoderIrDerecha()
    {
        bool ret = false;
        foreach(var pieza in piezas)
        {
            if (pieza.transform.position.x == columnas - 1)
            {
                ret = false;
                break;
            }
            else if (pieza.transform.position.x < columnas && !posiciones[(int)pieza.transform.position.x + 1, (int)pieza.transform.position.y])
            {
                ret = true;
            }
            else
            {
                ret = false;
                break;
            } 
        }
        return ret;
    }

    bool PoderIrIzquierda()
    {
        bool ret = false;
        foreach (var pieza in piezas)
        {
            if (pieza.transform.position.x > 0 && !posiciones[(int)pieza.transform.position.x - 1, (int)pieza.transform.position.y]) ret = true;
            else
            {
                ret = false;
                break;
            }
        }
        return ret;
    }

    bool PoderIrAbajoTecla(GameObject piezaAbajo)
    {
        if (piezaAbajo.transform.position.y > 0 && !posiciones[(int)piezaAbajo.transform.position.x, (int)piezaAbajo.transform.position.y - 1]) return true;
        else return false;
    }

    // Piezas
    void SpawnS()
    {
        if (!posiciones[columnas / 2 - 1, altura - 1] && !posiciones[columnas / 2, altura - 1] && !posiciones[columnas / 2, altura - 2] && !posiciones[columnas / 2 + 1, altura - 2])
        {
            if (profundidad)
            {
                piezas[0].transform.position = new Vector3(columnas / 2 - 1, altura - 1, (float)random.NextDouble());
                piezas[1].transform.position = new Vector3(columnas / 2, altura - 1, (float)random.NextDouble());
                piezas[2].transform.position = new Vector3(columnas / 2, altura - 2, (float)random.NextDouble());
                piezas[3].transform.position = new Vector3(columnas / 2 + 1, altura - 2, (float)random.NextDouble());
            }
            else
            {
                piezas[0].transform.position = new Vector3(columnas / 2 - 1, altura - 1, 0);
                piezas[1].transform.position = new Vector3(columnas / 2, altura - 1, 0);
                piezas[2].transform.position = new Vector3(columnas / 2, altura - 2, 0);
                piezas[3].transform.position = new Vector3(columnas / 2 + 1, altura - 2, 0);
            }
        }
        else gameOver = true;
    }

    void SpawnSNext()
    {
        siguientesPiezas[0].transform.position = new Vector3(columnas + 3, altura / 2 + 1, 0);
        siguientesPiezas[1].transform.position = new Vector3(columnas + 4, altura / 2 + 1, 0);
        siguientesPiezas[2].transform.position = new Vector3(columnas + 4, altura / 2, 0);
        siguientesPiezas[3].transform.position = new Vector3(columnas + 5, altura / 2, 0);
    }

    void SpawnI()
    {
        if (!posiciones[columnas / 2 - 2, altura - 1] && !posiciones[columnas / 2 - 1, altura - 1] && !posiciones[columnas / 2, altura - 1] && !posiciones[columnas / 2 + 1, altura - 1])
        {
            if (profundidad)
            {
                piezas[0].transform.position = new Vector3(columnas / 2 - 2, altura - 1, (float)random.NextDouble());
                piezas[1].transform.position = new Vector3(columnas / 2 - 1, altura - 1, (float)random.NextDouble());
                piezas[2].transform.position = new Vector3(columnas / 2, altura - 1, (float)random.NextDouble());
                piezas[3].transform.position = new Vector3(columnas / 2 + 1, altura - 1, (float)random.NextDouble());
            }
            else
            {
                piezas[0].transform.position = new Vector3(columnas / 2 - 2, altura - 1, 0);
                piezas[1].transform.position = new Vector3(columnas / 2 - 1, altura - 1, 0);
                piezas[2].transform.position = new Vector3(columnas / 2, altura - 1, 0);
                piezas[3].transform.position = new Vector3(columnas / 2 + 1, altura - 1, 0);
            }
        }
        else gameOver = true;
    }

    void SpawnINext()
    {
        siguientesPiezas[0].transform.position = new Vector3(columnas + 2, altura / 2 + 1, 0);
        siguientesPiezas[1].transform.position = new Vector3(columnas + 3, altura / 2 + 1, 0);
        siguientesPiezas[2].transform.position = new Vector3(columnas + 4, altura / 2 + 1, 0);
        siguientesPiezas[3].transform.position = new Vector3(columnas + 5, altura / 2 + 1, 0);
    }

    void SpawnL()
    {
        if (!posiciones[columnas / 2, altura - 1] && !posiciones[columnas / 2, altura - 2] && !posiciones[columnas / 2, altura - 3] && !posiciones[columnas / 2 + 1, altura - 3])
        {
            if (profundidad)
            {
                piezas[0].transform.position = new Vector3(columnas / 2, altura - 1, (float)random.NextDouble());
                piezas[1].transform.position = new Vector3(columnas / 2, altura - 2, (float)random.NextDouble());
                piezas[2].transform.position = new Vector3(columnas / 2, altura - 3, (float)random.NextDouble());
                piezas[3].transform.position = new Vector3(columnas / 2 + 1, altura - 3, (float)random.NextDouble());
            }
            else
            {
                piezas[0].transform.position = new Vector3(columnas / 2, altura - 1, 0);
                piezas[1].transform.position = new Vector3(columnas / 2, altura - 2, 0);
                piezas[2].transform.position = new Vector3(columnas / 2, altura - 3, 0);
                piezas[3].transform.position = new Vector3(columnas / 2 + 1, altura - 3, 0);
            }
        }
        else gameOver = true;
    }

    void SpawnLNext()
    {
        siguientesPiezas[0].transform.position = new Vector3(columnas + 4, altura / 2 + 1, 0);
        siguientesPiezas[1].transform.position = new Vector3(columnas + 4, altura / 2, 0);
        siguientesPiezas[2].transform.position = new Vector3(columnas + 4, altura / 2 - 1, 0);
        siguientesPiezas[3].transform.position = new Vector3(columnas + 5, altura / 2 - 1, 0);
    }

    void SpawnT()
    {
        if (!posiciones[columnas / 2 - 1, altura - 1] && !posiciones[columnas / 2, altura - 1] && !posiciones[columnas / 2 + 1, altura - 1] && !posiciones[columnas / 2, altura - 2])
        {
            if (profundidad)
            {
                piezas[0].transform.position = new Vector3(columnas / 2 - 1, altura - 1, (float)random.NextDouble());
                piezas[1].transform.position = new Vector3(columnas / 2, altura - 1, (float)random.NextDouble());
                piezas[2].transform.position = new Vector3(columnas / 2 + 1, altura - 1, (float)random.NextDouble());
                piezas[3].transform.position = new Vector3(columnas / 2, altura - 2, (float)random.NextDouble());
            }
            else
            {
                piezas[0].transform.position = new Vector3(columnas / 2 - 1, altura - 1, 0);
                piezas[1].transform.position = new Vector3(columnas / 2, altura - 1, 0);
                piezas[2].transform.position = new Vector3(columnas / 2 + 1, altura - 1, 0);
                piezas[3].transform.position = new Vector3(columnas / 2, altura - 2, 0);
            }
        }
        else gameOver = true;
    }

    void SpawnTNext()
    {
        siguientesPiezas[0].transform.position = new Vector3(columnas + 3, altura / 2 + 1, 0);
        siguientesPiezas[1].transform.position = new Vector3(columnas + 4, altura / 2 + 1, 0);
        siguientesPiezas[2].transform.position = new Vector3(columnas + 5, altura / 2 + 1, 0);
        siguientesPiezas[3].transform.position = new Vector3(columnas + 4, altura / 2 + 2, 0);
    }

    void SpawnO()
    {
        if (!posiciones[columnas / 2 - 1, altura - 1] && !posiciones[columnas / 2, altura - 1] && !posiciones[columnas / 2 - 1, altura - 2] && !posiciones[columnas / 2, altura - 2])
        {
            if (profundidad)
            {
                piezas[0].transform.position = new Vector3(columnas / 2 - 1, altura - 1, (float)random.NextDouble());
                piezas[1].transform.position = new Vector3(columnas / 2, altura - 1, (float)random.NextDouble());
                piezas[2].transform.position = new Vector3(columnas / 2 - 1, altura - 2, (float)random.NextDouble());
                piezas[3].transform.position = new Vector3(columnas / 2, altura - 2, (float)random.NextDouble());
            }
            else
            {
                piezas[0].transform.position = new Vector3(columnas / 2 - 1, altura - 1, 0);
                piezas[1].transform.position = new Vector3(columnas / 2, altura - 1, 0);
                piezas[2].transform.position = new Vector3(columnas / 2 - 1, altura - 2, 0);
                piezas[3].transform.position = new Vector3(columnas / 2, altura - 2, 0);
            }
        }
        else gameOver = true;
    }

    void SpawnONext()
    {
        siguientesPiezas[0].transform.position = new Vector3(columnas + 3, altura / 2 + 1, 0);
        siguientesPiezas[1].transform.position = new Vector3(columnas + 4, altura / 2 + 1, 0);
        siguientesPiezas[2].transform.position = new Vector3(columnas + 3, altura / 2, 0);
        siguientesPiezas[3].transform.position = new Vector3(columnas + 4, altura / 2, 0);
    }

    void SpawnLReversed()
    {
        if (!posiciones[columnas / 2, altura - 1] && !posiciones[columnas / 2, altura - 2] && !posiciones[columnas / 2, altura - 3] && !posiciones[columnas / 2 - 1, altura - 3])
        {
            if (profundidad)
            {
                piezas[0].transform.position = new Vector3(columnas / 2, altura - 1, (float)random.NextDouble());
                piezas[1].transform.position = new Vector3(columnas / 2, altura - 2, (float)random.NextDouble());
                piezas[2].transform.position = new Vector3(columnas / 2, altura - 3, (float)random.NextDouble());
                piezas[3].transform.position = new Vector3(columnas / 2 - 1, altura - 3, (float)random.NextDouble());
            }
            else
            {
                piezas[0].transform.position = new Vector3(columnas / 2, altura - 1, 0);
                piezas[1].transform.position = new Vector3(columnas / 2, altura - 2, 0);
                piezas[2].transform.position = new Vector3(columnas / 2, altura - 3, 0);
                piezas[3].transform.position = new Vector3(columnas / 2 - 1, altura - 3, 0);
            }
        }
        else gameOver = true;
    }

    void SpawnLReversedNext()
    {
        siguientesPiezas[0].transform.position = new Vector3(columnas + 4, altura / 2 + 1, 0);
        siguientesPiezas[1].transform.position = new Vector3(columnas + 4, altura / 2, 0);
        siguientesPiezas[2].transform.position = new Vector3(columnas + 4, altura / 2 - 1, 0);
        siguientesPiezas[3].transform.position = new Vector3(columnas + 3, altura / 2 - 1, 0);
    }

    void SpawnSReversed()
    {
        if (!posiciones[columnas / 2 + 1, altura - 1] && !posiciones[columnas / 2, altura - 1] && !posiciones[columnas / 2, altura - 2] && !posiciones[columnas / 2 - 1, altura - 2])
        {
            if (profundidad)
            {
                piezas[0].transform.position = new Vector3(columnas / 2 + 1, altura - 1, (float)random.NextDouble());
                piezas[1].transform.position = new Vector3(columnas / 2, altura - 1, (float)random.NextDouble());
                piezas[2].transform.position = new Vector3(columnas / 2, altura - 2, (float)random.NextDouble());
                piezas[3].transform.position = new Vector3(columnas / 2 - 1, altura - 2, (float)random.NextDouble());
            }
            else
            {
                piezas[0].transform.position = new Vector3(columnas / 2 + 1, altura - 1, 0);
                piezas[1].transform.position = new Vector3(columnas / 2, altura - 1, 0);
                piezas[2].transform.position = new Vector3(columnas / 2, altura - 2, 0);
                piezas[3].transform.position = new Vector3(columnas / 2 - 1, altura - 2, 0);
            }
        }
        else gameOver = true;
    }

    void SpawnSReversedNext()
    {
        siguientesPiezas[0].transform.position = new Vector3(columnas + 5, altura / 2 + 1, 0);
        siguientesPiezas[1].transform.position = new Vector3(columnas + 4, altura / 2 + 1, 0);
        siguientesPiezas[2].transform.position = new Vector3(columnas + 4, altura / 2, 0);
        siguientesPiezas[3].transform.position = new Vector3(columnas + 3, altura / 2, 0);
    }

    void SpawnSpecial()
    {
        if (!posiciones[columnas / 2, altura - 1])
        {
            foreach (var pieza in piezas)
            {
                pieza.transform.position = new Vector3(columnas / 2, altura - 1, 0);
            }
        }
    }

    void SpawnSpecialNext()
    {
        foreach (var pieza in siguientesPiezas)
        {
            pieza.transform.position = new Vector3(columnas + 4, altura / 2 + 2, 0);
        }
    }

    Vector3[] RotarPieza()
    {
        // Obtener el centro de rotación
        Vector3 pivot = piezas[1].transform.position;
        Vector3[] posicionesAlRotar = new Vector3[4];
        // Rotar cada cubo alrededor del pivote
        for (int i = 0; i < piezas.Length; i++)
        {
            Vector3 relativePos = piezas[i].transform.position - pivot;
            posicionesAlRotar[i] = new Vector3(-relativePos.y + pivot.x, relativePos.x + pivot.y, piezas[i].transform.position.z);
        }
        return posicionesAlRotar;
    }

    bool PoderRotarPieza(Vector3[] posicionesAlRotar)
    {
        bool resultado = true;
        for (int i = 0; i < posicionesAlRotar.Length; i++)
        {
            if (posicionesAlRotar[i].x < 0 || posicionesAlRotar[i].y < 0 || posicionesAlRotar[i].x >= columnas || posicionesAlRotar[i].y >= altura || posiciones[(int)posicionesAlRotar[i].x, (int)posicionesAlRotar[i].y])
            {
                resultado = false;
                break;
            }
        }
        return resultado;
    }

    // Comprobar si una línea está completa
    bool LineaLlena(int numeroDeLinea)
    {
        Debug.Log("Numero de línea: " + numeroDeLinea);
        if (numeroDeLinea >= 0 && numeroDeLinea < altura)
        {
            if (Enumerable.Range(0, posiciones.GetLength(0)).All(j => posiciones[j, numeroDeLinea]))
            {
                Debug.Log($"Línea {numeroDeLinea} llena");
                foreach (var pieza in todasLasPiezas)
                {
                    if (pieza.fila >= numeroDeLinea)
                    {
                        pieza.BajarUnaFila(numeroDeLinea);
                    }
                }
                puntuacionPartida += Mathf.RoundToInt(100f - tiempoMovimiento * 100);
                puntuacionActualizada?.Invoke(puntuacionPartida);
                tiempoMovimiento -= 0.0125f;
                return true;
            }
            return false;
        }
        else
        {
            return false;
        }
    }

    void AjustarPosiciones(int lineaLlena)
    {
        for (int x = 0; x < columnas; x++)
        {
            for (int y = lineaLlena + 1; y < altura; y++)
            {
                posiciones[x, y - 1] = posiciones[x, y];
                if (y == altura - 1)
                {
                    posiciones[x, y] = false;
                }
            }
        }
    }

    void VaciarColumna(int columna)
    {
        foreach (var pieza in todasLasPiezas)
        {
            if (pieza.columna == columna)
            {
                Debug.Log($"Eliminando pieza en columna {columna}");
                pieza.EliminarPiezaColumna();
            }
        }
        for (int y = 0; y < altura; y++)
        {
            posiciones[columna, y] = false;
        }
    }

    void MostrarProximasPiezas()
    {
        switch (proximaPieza)
        {
            case 0:
                SpawnSNext();
                break;
            case 1:
                SpawnINext();
                break;
            case 2:
                SpawnLNext();
                break;
            case 3:
                SpawnTNext();
                break;
            case 4:
                SpawnONext();
                break;
            case 5:
                SpawnLReversedNext();
                break;
            case 6:
                SpawnSReversedNext();
                break;
            case 7:
                SpawnSpecialNext();
                break;
        }

        foreach (var pieza in siguientesPiezas)
        {
            pieza.GetComponent<Renderer>().material.color = colores[proximoColor];
        }
    }

    public void Derecha()
    {
        pulsadoDerecha = true;
    }

    public void Izquierda()
    {
        pulsadoIzquierda = true;
    }

    public void Abajo()
    {
        pulsadoAbajo = true;
    }

    public void Rotar()
    {
        pulsadoRotar = true;
    }

    public void BajarTodo()
    {
        pulsandoBajarTodo = true;
    }

}
