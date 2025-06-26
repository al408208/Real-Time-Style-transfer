using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerMechanics : MonoBehaviour
{
    public ShooterC shooterC;
    private bool statsactivate = true; 
    public GameObject Estadisticas1;
    public GameObject Estadisticas2;
    public TargetScript Enemy1;
    public TargetScript Enemy2;
    public Alcanzado alc;
    public TargetScript Enemy3;
    public GameObject ob1,ob2,ob3,ob4;
    private bool activo = false;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            statsactivate = !statsactivate;
            if (statsactivate)
            {
                Cursor.lockState = CursorLockMode.None;
            }else{
                Cursor.lockState = CursorLockMode.Locked;
            }
            Estadisticas1.SetActive(statsactivate);
            Estadisticas2.SetActive(statsactivate);
        }
        if (alc.GetAlcanzado())
        {
            if (Enemy1.muerte == true && Enemy2.muerte == true && Enemy3.muerte == true && !activo)
            {
                ob1.SetActive(true);
                ob2.SetActive(true);
                ob3.SetActive(true);
                ob4.SetActive(true);
                shooterC.addBalas(-10);
                activo = true;
            }
        }
        
    }
}
