using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EscenarioController : MonoBehaviour
{
    public StyleTransferDobleEstilo scriptModoA;
    public LayerStyleTransfer scriptModoB;
    public MenuManager menuManager;

    public GameObject camExtra1;
    public GameObject camExtra2;
    public GameObject camExtra3;
    public GameObject sourceCam;
    
    private bool statsactivate=true; 
    public GameObject Estadisticas1;
    public GameObject Estadisticas2;

    void Start()
    {
        switch (GameManager.Instance.modalidadSeleccionada)
        {
            case GameManager.ModoJuego.A:
                scriptModoA.enabled = true;
                menuManager.enabled = true;
                break;
            case GameManager.ModoJuego.B:
                camExtra1.SetActive(true);
                camExtra2.SetActive(true);
                camExtra3.SetActive(true);
                sourceCam.SetActive(true);
                scriptModoB.enabled = true;
                break;
        }

    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            statsactivate = !statsactivate;
            Estadisticas1.SetActive(statsactivate);
            Estadisticas2.SetActive(statsactivate);
        }
    }
}
