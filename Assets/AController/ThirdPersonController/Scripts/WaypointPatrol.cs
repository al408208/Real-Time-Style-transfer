using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WaypointPatrol : MonoBehaviour
{
    public NavMeshAgent navMeshAgent;
    public Transform[] waypoints;

    public float rotationSpeed = 1f;
    public float rotationTolerance = 5f; // En grados

    private int m_CurrentWaypointIndex = 0;
    private bool rotating = true;

    void Start()
    {
        navMeshAgent.updateRotation = false;
        navMeshAgent.stoppingDistance = 0.2f;
        navMeshAgent.SetDestination(waypoints[m_CurrentWaypointIndex].position);
        rotating = true;
    }

    void Update()
    {
        Vector3 direction = (waypoints[m_CurrentWaypointIndex].position - transform.position).normalized;
        direction.y = 0;

        // FASE 1: ROTAR
        if (rotating)
        {
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    lookRotation,
                    rotationSpeed * Time.deltaTime * 100f
                );

                float angle = Quaternion.Angle(transform.rotation, lookRotation);
                if (angle < rotationTolerance)
                {
                    rotating = false;
                    navMeshAgent.isStopped = false;
                }
                else
                {
                    navMeshAgent.isStopped = true;
                    return; // Esperar a girar completamente
                }
            }
        }

        if (navMeshAgent.remainingDistance < navMeshAgent.stoppingDistance)
        {
            m_CurrentWaypointIndex = (m_CurrentWaypointIndex + 1) % waypoints.Length;
            navMeshAgent.SetDestination(waypoints[m_CurrentWaypointIndex].position);
            rotating = true;
        }
        
    }
}