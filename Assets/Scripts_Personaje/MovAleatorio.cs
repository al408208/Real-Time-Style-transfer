using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovAleatorio : MonoBehaviour
{
    public float velocidad; // Velocidad de movimiento del objeto
    public float limiteX = 15f; // Punto límite en el eje X

    private int direccion = 1; // Dirección inicial del movimiento
    private int randomValue;

    void Start(){
        velocidad = Random.Range(2, 5); // Genera un número aleatorio entre 4 (inclusive) y 6 (exclusivo)

        randomValue = Random.Range(0, 2); // Genera un número aleatorio entre 0 y 1

        direccion = (randomValue == 0) ? -1 : 1; // Asigna -1 si el valor aleatorio es 0, o 1 si es 1
    }

    void FixedUpdate()
    {
        // Mueve el objeto en la dirección actual
        transform.Translate(new Vector3 (0, 0, 2) * velocidad * direccion * Time.deltaTime);

         if (transform.position.x <= -limiteX){
            direccion = 1;
        }
        if (transform.position.x >= limiteX){
            direccion = -1;
        }

        
    }

    void OnTriggerEnter (Collider other)
    {
        other.transform.SetParent(transform);             
    }

     void OnTriggerExit (Collider other)
    {
        other.transform.SetParent(null);
        
    }
    
}
