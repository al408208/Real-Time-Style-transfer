using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum ModoJuego { A, B, C }
    public ModoJuego modalidadSeleccionada;
    public GameObject graphy;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // persiste entre escenas
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegistrarGraphy()
    {
        //GameObject posibleGraphy = GameObject.FindWithTag("Graphy");
        GameObject posibleGraphy = GameObject.Find("[Graphy]");
        if (posibleGraphy != null)
        {
            graphy = posibleGraphy; // solo si lo encuentra
        }
        if (graphy != null)
        {
            graphy.SetActive(true);
        }
    }
}
