using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotation : MonoBehaviour
{
    public GameObject player;
    private ShooterC shooterC; 
    public AudioSource sound;
    void Start()
    {
        shooterC=FindObjectOfType<ShooterC>();

    }
    // Start is called before the first frame update
    void OnTriggerEnter (Collider other)
    {
        if (other.gameObject == player)
        {
            shooterC.addBalas(10);
            sound.Play();
            Destroy(gameObject);
        }
    }


    
}
