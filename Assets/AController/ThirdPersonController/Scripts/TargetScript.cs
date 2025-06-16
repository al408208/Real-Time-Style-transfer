using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetScript : MonoBehaviour
{
    public Transform target; // Asigna aquí tu personaje en el inspector
    public float speed = 2f;
    private int vida=40;
    private Animator animator;
    public bool muerte=false;


    // Start is called before the first frame update
    void Start()
    {
        animator=GetComponent<Animator>();
    }

           

    // Update is called once per frame
    void Update()
    {
        if (target == null || vida<=0) return;

        // Mira hacia el jugador (ignorando la altura para evitar que mire hacia arriba o abajo)
        Vector3 direction = target.position - transform.position;
        direction.y = 0f;
        if (direction != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, Time.deltaTime * 5f);
        }

        // Avanza hacia adelante
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            destroyer();
        }
    }

    public void dañado(int damage){
        vida-=damage;
        if(vida<=0){
            if (animator != null)
            {
                animator.speed = 1f; 
            }
            animator.SetBool("IsDead",true);
            Invoke("destroyer", 4f);
        }
    }

    public void slowed()
    {
        speed /= 2f;
        if (animator != null)
        {
            animator.speed = 0.5f; 
        }
    }

    private void destroyer()
    {
        Destroy(gameObject);
    }
}
