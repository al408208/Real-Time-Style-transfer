using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetScript : MonoBehaviour
{

    public GameObject player;
    private int vida=100;
    private Animator animator;
    public GameEnding gameEnding;
    public bool seguirPersonaje=false;
    public bool muerte=false;
    public int velocidadp;


    // Start is called before the first frame update
    void Start()
    {
        animator=GetComponent<Animator>();
    }

           

    // Update is called once per frame
    void Update()
    {
        if(this.gameObject.name!="zombi_cueva"){
            if(Vector3.Distance(transform.position,player.transform.position)<7 && muerte==false){
                seguirPersonaje=true;
                var lookpos=player.transform.position-transform.position;//rotar en y hacia el personaje
                lookpos.y=0;
                var rotation=Quaternion.LookRotation(lookpos);
                transform.rotation=Quaternion.RotateTowards(transform.rotation,rotation,2);
                
                transform.Translate(Vector3.forward*velocidadp*Time.deltaTime);//movimiento
            }else{
                seguirPersonaje=false;
            }
        }
        
        
    }

    void OnTriggerEnter (Collider other)
    {
        if (other.gameObject == player)
        {
           gameEnding.CaughtPlayer ();
        }
    }

    public void dañado(int damage){
        vida-=damage;
        if(vida<=0){
            animator.SetBool("IsDead",true);
            Invoke("destroyer", 4f);
            
        }

    }

    private void destroyer(){
        Destroy(gameObject);
    }
}
