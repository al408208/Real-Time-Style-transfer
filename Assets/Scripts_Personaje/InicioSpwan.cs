using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InicioSpwan : MonoBehaviour
{
    public GameObject player;
    public bool initSpwan=false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
     void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            initSpwan=true;
            Destroy(gameObject);
        }
    }
}
