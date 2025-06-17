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

   void Update () 
	{
		// Rotate the game object that this script is attached to by 15 in the X axis,
		// 30 in the Y axis and 45 in the Z axis, multiplied by deltaTime in order to make it per second
		// rather than per frame.
		transform.Rotate (new Vector3 (15, 30, 45) * Time.deltaTime);
	}

    
}
