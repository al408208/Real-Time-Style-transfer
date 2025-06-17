using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class PlayerInteraction : MonoBehaviour
{

    public Camera mainCam;
    public float interactionDistance = 2f;

    public GameObject interactionUI;
    public GameObject aim;

    private void Update()
    {
        InteractionRay();
    }

    void InteractionRay()
    {
        Ray ray = mainCam.ViewportPointToRay(Vector3.one / 2f);
        RaycastHit hit;
  Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.red);

        bool hitSomething = false;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {


            if (hit.collider.CompareTag("Interactuable"))
            {
                hitSomething = true;
            }
        }

        interactionUI.SetActive(hitSomething);
        aim.SetActive(!hitSomething);
    }
    
}