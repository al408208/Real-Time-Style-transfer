using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EscenarioController : MonoBehaviour
{
    public StyleTransferDobleEstilo scriptModoA;
    public LayerStyleTransfer scriptModoB;
    public StyleTransfer scriptModoC;

    public GameObject camExtra1;
    public GameObject camExtra2;
    public GameObject sourceCam;

    void Start()
    {
        switch (GameManager.Instance.modalidadSeleccionada)
        {
            case GameManager.ModoJuego.A:
                scriptModoA.enabled = true;
                break;
            case GameManager.ModoJuego.B:
                camExtra1.SetActive(true);
                camExtra2.SetActive(true);
                sourceCam.SetActive(true);
                scriptModoB.enabled = true;
                break;
            case GameManager.ModoJuego.C:
                camExtra1.SetActive(true);
                sourceCam.SetActive(true);
                scriptModoC.enabled = true;
                break;
        }
        
    }
}
