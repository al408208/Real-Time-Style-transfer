using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;
using UnityEngine.SceneManagement;

public class StyleTransferDobleEstilo : MonoBehaviour
{
    [Tooltip("Performs the preprocessing and postprocessing steps")]
    public ComputeShader styleTransferShader;

    [Tooltip("The height of the image being fed to the model")]
    public int targetHeight = 540;

    [Tooltip("List of model asset files")]
    public NNModel[] modelAssets;

    [Tooltip("The backend used when performing inference")]
    public WorkerFactory.Type workerType = WorkerFactory.Type.Auto;

    private IWorker[] engines;
    private IWorker currentEngine  = null; // null o -1 = Sin modelo, 0+ = Modelos cargados

    [SerializeField]
    private MenuManager menuManager; // Referencia al script MenuManager

    private bool imgclick=false;

    void Start()
    {
        // Inicializar los workers para todos los modelos
        engines = new IWorker[modelAssets.Length];
        for (int i = 0; i < modelAssets.Length; i++)
        {
            Model runtimeModel = ModelLoader.Load(modelAssets[i]);
            engines[i] = WorkerFactory.CreateWorker(workerType, runtimeModel);
        }
    }

    private void OnDisable()
    {
       // Liberar todos los motores al desactivar el script
        if (engines != null)
        {
            foreach (var engine in engines)
            {
                if (engine != null) engine.Dispose();
            }
        }
    }

    void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Return) || imgclick)&&  menuManager.getMenuActive())
        {
            if (!menuManager.getPuedeMoverse()) return; // Bloquea el input mientras la animación está activa

            int selectedStyle = menuManager.getStyle(); // Obtener el estilo seleccionado desde MenuManager

            if (selectedStyle >= 0 && selectedStyle < engines.Length)
            {
                currentEngine = engines[selectedStyle]; // Activar el modelo seleccionado
            }
            else
            {
                currentEngine = null; // Si es -1 o fuera de rango, desactivar estilizado
            }
            imgclick=false;
            menuManager.closeMenu();
        }

            //currentModelIndex = (currentModelIndex + 1) % (modelAssets.Length + 1); // Ciclo entre 0 (sin modelo) y los modelos cargados


    }

    private void StylizeImage(RenderTexture src)
    {
        if (currentEngine  == null) return; // No aplicar si no hay modelo activo

        //IWorker currentEngine = engines[currentModelIndex - 1]; // Ajustar índice ya que 0 es "sin modelo"

        RenderTexture rTex;
        if (src.height > targetHeight && targetHeight >= 8)
        {
            float scale = src.height / targetHeight;
            int targetWidth = (int)(src.width / scale);
            targetHeight -= (targetHeight % 8);
            targetWidth -= (targetWidth % 8);
            rTex = RenderTexture.GetTemporary(targetWidth, targetHeight, 24, src.format);
        }
        else
        {
            rTex = RenderTexture.GetTemporary(src.width, src.height, 24, src.format);
        }

        Graphics.Blit(src, rTex);
        ProcessImage(rTex, "ProcessInput");

        Tensor input = new Tensor(rTex, channels: 3);
        currentEngine.Execute(input);
        Tensor prediction = currentEngine.PeekOutput();
        input.Dispose();

        RenderTexture.active = null;
        prediction.ToRenderTexture(rTex);
        prediction.Dispose();

        ProcessImage(rTex, "ProcessOutput");
        Graphics.Blit(rTex, src);
        RenderTexture.ReleaseTemporary(rTex);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (currentEngine != null)
        {
            StylizeImage(src);
        }

        Graphics.Blit(src, dest);
    }

    private void ProcessImage(RenderTexture image, string functionName)
    {
        int numthreads = 8;
        int kernelHandle = styleTransferShader.FindKernel(functionName);
        RenderTexture result = RenderTexture.GetTemporary(image.width, image.height, 24, RenderTextureFormat.ARGBHalf);
        result.enableRandomWrite = true;
        result.Create();

        styleTransferShader.SetTexture(kernelHandle, "Result", result);
        styleTransferShader.SetTexture(kernelHandle, "InputImage", image);
        styleTransferShader.Dispatch(kernelHandle, result.width / numthreads, result.height / numthreads, 1);

        Graphics.Blit(result, image);
        RenderTexture.ReleaseTemporary(result);
    }

    public void OnImageClicked()
    {
        imgclick=true;
    }
}