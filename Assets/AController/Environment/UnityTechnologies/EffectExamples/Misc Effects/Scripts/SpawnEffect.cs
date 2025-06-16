using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnEffect : MonoBehaviour
{

    public float spawnEffectTime = 2;
    public float pause = 1;
    public AnimationCurve fadeIn;

    ParticleSystem ps;
    float timer = 0;

    int shaderProperty;

    public GameObject player;

    [Header("Nombre de la escena a precargar")]
    private string sceneToPreload = "RealisticDemo";
    AsyncOperation preloadOperation;
    [Header("Delay para evitar cargar durante picos de uso al principio")]
    public float delayBeforePreload = 3f;

    private bool sceneReady = false;


    void Start()
    {
        shaderProperty = Shader.PropertyToID("_cutoff");
        ps = GetComponentInChildren<ParticleSystem>();

        var main = ps.main;

        main.duration = spawnEffectTime;
        if (SceneManager.GetActiveScene().name == "SandBox")
        {
            sceneToPreload = "RealisticDemo";
        }
        else if (SceneManager.GetActiveScene().name == "RealisticDemo")
        {
            sceneToPreload = "Example_01";
        }else{
            sceneToPreload = "Sandbox";
        }
        if (GameManager.Instance.modalidadSeleccionada == GameManager.ModoJuego.C)
        {
            sceneToPreload = "Sandbox";
        }

        StartCoroutine(DelayedPreload());
    }

    void Update()
    {
        if (timer < spawnEffectTime + pause)
        {
            timer += Time.deltaTime;
        }
        else
        {
            timer = 0;
        }

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            ps.Play();

            Invoke("ActivarEscena", 1.5f);
        }
    }

    IEnumerator DelayedPreload()
    {
        yield return new WaitForSeconds(delayBeforePreload);

        preloadOperation = SceneManager.LoadSceneAsync(sceneToPreload);
        preloadOperation.allowSceneActivation = false;

        // Esperamos a que esté casi lista (Unity llega hasta 0.9f y se detiene ahí)
        while (preloadOperation.progress < 0.9f)
        {
            yield return null;
        }

        sceneReady = true;
        Debug.Log("Escena precargada y lista para activar.");
    }

    void ActivarEscena()
    {
        if (sceneReady && preloadOperation != null)
        {
            preloadOperation.allowSceneActivation = true;
        }
        else
        {
            Debug.LogWarning("La escena aún no está lista. Esperá un momento.");
        }
    }
}
