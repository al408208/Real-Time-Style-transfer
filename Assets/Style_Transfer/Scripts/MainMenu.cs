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
    public Slider barraCarga;  // Asigna desde el Inspector
    public GameObject barra;
    public GameObject menu;
    public GameObject controls;
    bool cActive = false;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            ScreenCapture.CaptureScreenshot("012.png");
        }
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

    public void SeleccionarModo(int modo)
    {

        if (modo == 1)
        {
            GameManager.Instance.modalidadSeleccionada = GameManager.ModoJuego.A;
            StartCoroutine(CargarEscena("SandBox"));
        }
        else if (modo == 2)
        {
            GameManager.Instance.modalidadSeleccionada = GameManager.ModoJuego.B;
            StartCoroutine(CargarEscena("SandBox"));
        }
        else
        {
            GameManager.Instance.modalidadSeleccionada = GameManager.ModoJuego.C;
            StartCoroutine(CargarEscena("TestRoom"));
        }
        menu.SetActive(false);
    }

    IEnumerator CargarEscena(string nombre)
    {
        barra.SetActive(true);
        AsyncOperation carga = SceneManager.LoadSceneAsync(nombre);
        carga.allowSceneActivation = false;

        float timer = 0f;

        while (carga.progress < 0.9f || timer < 2f)
        {
            timer += Time.deltaTime;
            float progresoReal = Mathf.Clamp01(carga.progress / 0.9f);
            barraCarga.value = Mathf.Clamp01(timer / 2f * progresoReal);  // sube en 2s como mínimo
            yield return null;
        }

        // Llenar barra al 100%
        barraCarga.value = 1f;

        // Esperar un momento o mostrar mensaje "Presiona para continuar"
        yield return new WaitForSeconds(0.1f);

        carga.allowSceneActivation = true;

    }
    public void InfoClicked()
    {
        cActive = !cActive;
        controls.SetActive(cActive);
    }
}
