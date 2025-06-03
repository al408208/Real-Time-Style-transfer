using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinSpawn : MonoBehaviour
{
    public GameObject player;
    [SerializeField] private InicioSpwan inicioSpwan;  

    void Start(){
        {
            inicioSpwan=FindObjectOfType<InicioSpwan>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            inicioSpwan.initSpwan=false;
            Destroy(gameObject);
        }
    }
}
