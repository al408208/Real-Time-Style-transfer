using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using StarterAssets;

public class CamaraPortal : MonoBehaviour
{

    public Camera otherCam;  // La cámara del personaje
    public Transform portalSalida;  // Portal en la otra habitación
    public GameObject player;
    private bool collidersDisabled = false; // Para saber si las colisiones están desactivadas
    List<Collider> portalColliders = new List<Collider>();
    List<Collider> paredColliders = new List<Collider>();
    public CinemachineVirtualCamera playerFollowCamera;//damping..

    void Start()
    {
        foreach (Collider col in FindObjectsOfType<Collider>())
        {
            if (col.CompareTag("portal"))
            {
                portalColliders.Add(col);
            }
            if (col.CompareTag("pared"))
            {
                paredColliders.Add(col);
            }
        }
    }
    void Update()
    {

        if (collidersDisabled && Vector3.Distance(player.transform.position, portalSalida.position) > 1.6f)
        {

            EnableCollisionsPortal();
        }
        if (collidersDisabled && Vector3.Distance(player.transform.position, portalSalida.position) > 4.8f)
        {

            EnableCollisionsPared();
            collidersDisabled = false; // Resetear el estado
        }

        if (!collidersDisabled && Vector3.Distance(player.transform.position, portalSalida.position) < 1.6f)
        {
            DisableCollisions("pared");
        }


    }
    void LateUpdate()
    {
        Vector3 relativePos = transform.InverseTransformPoint(Camera.main.transform.position);

        // REFLEJA Y APLICA EL OFFSET EN EL OTRO PORTAL
        Vector3 mirroredPos = new Vector3(relativePos.x, relativePos.y, relativePos.z);
        otherCam.transform.position = portalSalida.TransformPoint(mirroredPos);


        Quaternion relativeRot = Quaternion.Inverse(transform.rotation) * Camera.main.transform.rotation;
        otherCam.transform.rotation = portalSalida.rotation * relativeRot;
    }

    private void OnTriggerStay(Collider other)
    {

        Vector3 PlayerFromPortal = transform.InverseTransformPoint(player.transform.position);
        if (other.tag == "Player")
        {

            if (PlayerFromPortal.y <= 0.2)
            {
                //Desacutvar damping
                Vector3 originalDamping = playerFollowCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>().Damping;
                playerFollowCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>().Damping = Vector3.zero;

                // DESACTIVAR CharacterController si existe para transportar a mi personaje
                CharacterController cc = player.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;

                //POSICION
                Vector3 newPosition = portalSalida.TransformPoint(PlayerFromPortal);
                newPosition.x = portalSalida.position.x;
                newPosition.z = portalSalida.position.z;
                player.transform.position = newPosition;

                //ROTACION
                Quaternion relativeRotation = Quaternion.Inverse(transform.rotation) * player.transform.rotation;
                player.transform.rotation = portalSalida.rotation * relativeRotation;

                //ROTACION de la camara
                Quaternion camRelativeRot = Quaternion.Inverse(transform.rotation) * Camera.main.transform.rotation;
                Quaternion newCamRot = portalSalida.rotation * camRelativeRot;

                var controller = player.GetComponent<ThirdPersonController>();//para girar mi root que controla a donde mira la camara

                if (controller != null)
                {
                    controller.SetCameraRotation(newCamRot);
                }

                // REACTIVAR CharacterController
                if (cc != null) cc.enabled = true;

                StartCoroutine(RestoreDampingNextFrame(playerFollowCamera, originalDamping));
                Debug.Log("Teletransportado a " + other.transform.position);
                DisableCollisions("pared");
                DisableCollisions("portal");
            }
        }
    }


    void DisableCollisions(string tagExcepcion)
    {
        if (tagExcepcion == "pared")
        {
            foreach (Collider col in paredColliders)
                col.enabled = false;

        }
        else
        {
            foreach (Collider col in portalColliders)
                col.enabled = false;

            collidersDisabled = true;// Marcar que las colisiones fueron desactivadas

        }
    }


    void EnableCollisionsPared()
    {
        foreach (Collider col in paredColliders)
            col.enabled = true;
    }
    void EnableCollisionsPortal()
    {
        foreach (Collider col in portalColliders)
            col.enabled = true;
    }
    
    IEnumerator RestoreDampingNextFrame(CinemachineVirtualCamera vcam, Vector3 originalDamping)
    {
        yield return null; // Espera un frame
        vcam.GetCinemachineComponent<Cinemachine3rdPersonFollow>().Damping = originalDamping;
    }
}






