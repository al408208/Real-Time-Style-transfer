using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WaypointPatrol : MonoBehaviour
{
    public NavMeshAgent navMeshAgent;
    public Transform[] waypoints;
    private Animator animator;

    int m_CurrentWaypointIndex;
    public TargetScript targetScript; 

    void Start ()
    {
        animator = GetComponent<Animator>();

        navMeshAgent.SetDestination (waypoints[0].position);
    }

    void Update ()
    {
        if(targetScript.seguirPersonaje){
            navMeshAgent.isStopped = true;
            if(animator.GetBool("IsDead")==true){
                targetScript.muerte=true;
            }
        }else{
            if(animator.GetBool("IsDead")==false){
                navMeshAgent.isStopped = false;
            }
        }

        if(animator.GetBool("IsDead")==false){
            if(navMeshAgent.remainingDistance < navMeshAgent.stoppingDistance)
            {
                m_CurrentWaypointIndex = (m_CurrentWaypointIndex + 1) % waypoints.Length;
                navMeshAgent.SetDestination (waypoints[m_CurrentWaypointIndex].position);
            }
        }else{
            navMeshAgent.isStopped = true;
            targetScript.muerte=true;
        }
        
    }
}