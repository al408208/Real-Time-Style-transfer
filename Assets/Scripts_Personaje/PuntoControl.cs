using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuntoControl : MonoBehaviour
{
    public GameObject player;
    private bool kk=false;

    void FixedUpdate(){
        if(kk){
            kk=false;
            Change();
        }
    }

    // Start is called before the first frame update
    void OnTriggerEnter (Collider other)
    { 
        kk=true;
    }

    private void Change(){
        player.transform.position = new Vector3(-2.25f, 12.4f, 94.72f);
    }
    
}
