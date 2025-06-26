using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alcanzado : MonoBehaviour
{
    public GameObject cha1;
    public GameObject cha2;
    public GameObject cha3;
    bool alcanzado = false;
    // Update is called once per frame
    void Update()
    {
        if (gameObject.layer == 7)
        {
            alcanzado = true;
            cha1.SetActive(true);
            cha2.SetActive(true);
            cha3.SetActive(true);
            Destroy(gameObject);
        }
    }
    public bool GetAlcanzado()
    {
        return alcanzado;
    }
}
