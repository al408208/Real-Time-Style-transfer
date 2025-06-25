using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleRotation : MonoBehaviour
{
    private ParticleSystem particles;

    void Start()
    {
        // Obtiene el componente ParticleSystem
        particles = GetComponent<ParticleSystem>();
        particles.Play();
    }
}
