using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Transform posicion; 
    public GameObject zombie;
    private float timer = 0.0f; 
    [SerializeField] private InicioSpwan inicioSpwan;  

    void Start(){
        {
            inicioSpwan=FindObjectOfType<InicioSpwan>();
        }
    }

    void Update()
    {             
        if(inicioSpwan.initSpwan) {
            timer += Time.deltaTime;
            if (timer >= 10){ //if para que cada segundo cree una copia yluego reinicie el contador
                Instantiate(zombie, posicion.transform.position, Quaternion.identity); 
                timer = 0; 
            }
        }                                                                 
        
    }

}
