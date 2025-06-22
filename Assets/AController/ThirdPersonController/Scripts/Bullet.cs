using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private Transform vfxHitgreen;
    [SerializeField] private Transform vfxHitred;
    public int damage=20;    
    //public AudioSource sound;

    

    private Rigidbody bulletRigid;

    private void Awake(){
        bulletRigid=GetComponent<Rigidbody>();
    }

    private void Start(){            
        //sound.Play();
        float speed= 40f;
        bulletRigid.velocity=transform.forward*speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<TargetScript>() != null)
        {//hit

            Instantiate(vfxHitgreen, transform.position, Quaternion.identity);
            other.GetComponent<TargetScript>().slowed();
            other.GetComponent<TargetScript>().dañado(damage);
        }
        else
        {
            Instantiate(vfxHitred, transform.position, Quaternion.identity);

        }
        if (other.gameObject.layer == 0 || other.gameObject.layer == 7 || other.gameObject.layer == 8) 
        {
            other.gameObject.layer = 7;
            // Get a list of the child objects for the selected GameObject
            Transform[] allChildren = other.transform.gameObject.GetComponentsInChildren<Transform>();

            for (int i = 0; i < allChildren.Length; i++)
            {
                // MeshRenderer meshRenderer = allChildren[i].GetComponent<MeshRenderer>();
                allChildren[i].gameObject.layer = 7;
                // if (meshRenderer != null && meshRenderer.enabled)
                //  {
                //      allChildren[i].gameObject.layer = 6;
                // }
            }
        }
        Destroy(gameObject);
        
    }
}
