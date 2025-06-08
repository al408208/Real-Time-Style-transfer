using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GraphicRaycaster raycaster;   // Asigna el GraphicRaycaster del Canvas
    public EventSystem eventSystem;      // Asigna el EventSystem de la escena
    public Image image1, image2, image3;       // Asigna aquí las 3 imágenes a controlar
    public RectTransform selector;
    
    private AsyncOperation preloadOperation;
    private bool sceneReady = false;

    void Start()
    {
        preloadOperation = SceneManager.LoadSceneAsync("SandBox");
        preloadOperation.allowSceneActivation = false;
        StartCoroutine(EsperarCarga());
    }
    void Update()
    {
        PointerEventData pointerData = new PointerEventData(eventSystem)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerData, results);

        bool hovered = false;

        foreach (RaycastResult result in results)
        {
            if (result.gameObject == image1.gameObject)
            {
                selector.anchoredPosition = new Vector2(-650, selector.anchoredPosition.y);
                hovered = true;
                break;
            }
            else if (result.gameObject == image2.gameObject)
            {
                selector.anchoredPosition = new Vector2(0, selector.anchoredPosition.y);
                hovered = true;
                break;
            }
            else if (result.gameObject == image3.gameObject)
            {
                selector.anchoredPosition = new Vector2(650, selector.anchoredPosition.y);
                hovered = true;
                break;
            }
        }

        if (!hovered)
        {
            selector.anchoredPosition = new Vector2(-1300, selector.anchoredPosition.y);
            // selector.gameObject.SetActive(false);
        }
    }


    public void TheGallery()
    {
        SeleccionarModo(1);
    }
    public void FragmentedRealities()
    {
        SeleccionarModo(2);
    }
    public void StyleMechanics()
    {
        SeleccionarModo(3);
    }

    IEnumerator EsperarCarga()
    {
        while (preloadOperation.progress < 0.9f)
            yield return null;

        sceneReady = true;
        Debug.Log("Escena precargada y lista para activar.");
    }
    public void SeleccionarModo(int modo)
    {
        if (modo == 1)
        {
            GameManager.Instance.modalidadSeleccionada = GameManager.ModoJuego.A;
        }
        else if (modo == 2)
        {
            GameManager.Instance.modalidadSeleccionada = GameManager.ModoJuego.B;
        }
        else
        {
            GameManager.Instance.modalidadSeleccionada = GameManager.ModoJuego.C;
        }
        if (sceneReady)
        {
            preloadOperation.allowSceneActivation = true;
        }
        else
        {
            Debug.LogWarning("La escena aún no está lista. Esperá un momento.");
        }
    }
}
