using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;
using UnityEngine.SceneManagement;

public class LayerStyleTransfer : MonoBehaviour
{
    [Tooltip("Performs the preprocessing and postprocessing steps")]
    public ComputeShader styleTransferShader;

    [Tooltip("Stylize the camera feed")]
    public bool stylizeImage = true;

    [Tooltip("Stylize only specified GameObjects")]
    public bool targetedStylization = true;

    [Tooltip("The height of the image being fed to the model")]
    public int targetHeight = 540;

    [Tooltip("The model asset file that will be used when performing inference")]
    public NNModel modelAsset;
    public NNModel modelAsset2;
    public NNModel modelAsset3;

    [Tooltip("The backend used when performing inference")]
    public WorkerFactory.Type workerType = WorkerFactory.Type.Auto;

    [Tooltip("Captures the depth data for the target GameObjects")]
    public Camera styleDepth;
    public Camera styleDepth2;
    public Camera styleDepth3;

    private IWorker engine;
    private IWorker engine2;
    private IWorker engine3;


    [Tooltip("Captures the depth data for the entire scene")]
    public Camera sourceDepth;

    private Model m_RuntimeModel;
    public int stylizeEveryNFrames = 3;
    private int currentFrame = 0;
    private RenderTexture cachedStylizedFrame;
    void Start()
    {

        // Get the screen dimensions
        int width = Screen.width;
        int height = Screen.height;

        // Force the StyleDepth Camera to render to a Depth texture
        styleDepth.targetTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Depth);
        styleDepth.forceIntoRenderTexture = true;

        styleDepth2.targetTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Depth);
        styleDepth2.forceIntoRenderTexture = true;

        styleDepth3.targetTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Depth);
        styleDepth3.forceIntoRenderTexture = true;

        // Force the SourceDepth Camera to render to a Depth texture
        sourceDepth.targetTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Depth);
        sourceDepth.forceIntoRenderTexture = true;

        cachedStylizedFrame = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBHalf);
        cachedStylizedFrame.Create();


        m_RuntimeModel = ModelLoader.Load(modelAsset);
        engine = WorkerFactory.CreateWorker(workerType, m_RuntimeModel);
        engine2 = WorkerFactory.CreateWorker(workerType, ModelLoader.Load(modelAsset2));
        engine3 = WorkerFactory.CreateWorker(workerType, ModelLoader.Load(modelAsset3));

    }

    private void OnDisable()
    {
        engine.Dispose();
        engine2.Dispose();
        engine3.Dispose();

        // Release the Depth texture for the StyleDepth camera
        RenderTexture.ReleaseTemporary(styleDepth.targetTexture);
        RenderTexture.ReleaseTemporary(styleDepth2.targetTexture);
        RenderTexture.ReleaseTemporary(styleDepth3.targetTexture);
        // Release the Depth texture for the SourceDepth camera
        RenderTexture.ReleaseTemporary(sourceDepth.targetTexture);

    }

    void Update()
    {

        if (styleDepth.targetTexture.width != Screen.width || styleDepth.targetTexture.height != Screen.height)
        {
            int width = Screen.width;
            int height = Screen.height;

            RenderTexture.ReleaseTemporary(styleDepth.targetTexture);
            RenderTexture.ReleaseTemporary(styleDepth2.targetTexture);
            RenderTexture.ReleaseTemporary(styleDepth3.targetTexture);
            RenderTexture.ReleaseTemporary(sourceDepth.targetTexture);

            styleDepth.targetTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Depth);
            styleDepth2.targetTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Depth);
            styleDepth3.targetTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Depth);
            styleDepth3.forceIntoRenderTexture = true;
            sourceDepth.targetTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Depth);
        }


        if (Input.GetKeyDown(KeyCode.Q)){
            stylizeImage = !stylizeImage;

        }
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

    private void StylizeImageCustom(RenderTexture src, IWorker engine)
    {
        RenderTexture rTex;
        int finalTargetHeight = targetHeight;
        int finalTargetWidth = src.width;

        if (src.height > targetHeight && targetHeight >= 8)
        {
            float scale = (float)src.height / (float)targetHeight;
            finalTargetWidth  = Mathf.FloorToInt(src.width  / scale);
            finalTargetHeight = Mathf.FloorToInt(src.height / scale);

            // Alineamos a múltiplos de 8 sin modificar el campo original
            finalTargetHeight = finalTargetHeight - (finalTargetHeight % 8);
            finalTargetWidth  = finalTargetWidth  - (finalTargetWidth  % 8);
        }

        rTex = RenderTexture.GetTemporary(finalTargetWidth, finalTargetHeight, 24, src.format);
        Graphics.Blit(src, rTex);
        ProcessImage(rTex, "ProcessInput");

        Tensor input = new Tensor(rTex, channels: 3);
        // 4.1. Pedimos el IEnumerator que recorrerá capa a capa
        IEnumerator schedule = engine.StartManualSchedule(input);

        // 4.2. Iteramos TODO el grafo para que Barracuda prepare cada capa
        //      (sin esta iteración, nunca se encola nada internamente).
        while (schedule.MoveNext())
        {
            // Aquí 'MoveNext' encola capa tras capa; no hay que hacer nada dentro.
        }

        // 4.3. Una vez terminado el recorrido, ya podemos liberar el Tensor de entrada
        input.Dispose();

        // 4.4. Forzamos la ejecución de lo que haya encolado el schedule
        engine.FlushSchedule();
        Tensor prediction = engine.PeekOutput();

        RenderTexture.active = null;
        prediction.ToRenderTexture(rTex);
        prediction.Dispose();
        
        ProcessImage(rTex, "ProcessOutput");
        Graphics.Blit(rTex, src);
        RenderTexture.ReleaseTemporary(rTex);
    }

   private void Merge(RenderTexture styleImage1, RenderTexture styleImage2, RenderTexture styleImage3, RenderTexture sourceImage)
    {
        int numthreads = 8;
        int kernelHandle = styleTransferShader.FindKernel("Merge");
        int width = styleImage1.width;
        int height = styleImage1.height;
        RenderTexture result = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGBHalf);
        result.enableRandomWrite = true;
        result.Create();

        styleTransferShader.SetTexture(kernelHandle, "Result", result);
        styleTransferShader.SetTexture(kernelHandle, "InputImage", styleImage1); // estilo 1
        styleTransferShader.SetTexture(kernelHandle, "StyleImage2", styleImage2); // estilo 2
        styleTransferShader.SetTexture(kernelHandle, "StyleImage3", styleImage3);  // estilo 3
        styleTransferShader.SetTexture(kernelHandle, "StyleDepth", styleDepth.activeTexture);
        styleTransferShader.SetTexture(kernelHandle, "StyleDepth2", styleDepth2.activeTexture);
        styleTransferShader.SetTexture(kernelHandle, "StyleDepth3", styleDepth3.activeTexture);
        styleTransferShader.SetTexture(kernelHandle, "SrcDepth", sourceDepth.activeTexture);
        styleTransferShader.SetTexture(kernelHandle, "SrcImage", sourceImage);

        styleTransferShader.Dispatch(kernelHandle, result.width / numthreads, result.height / numthreads, 1);
        Graphics.Blit(result, styleImage1);
        RenderTexture.ReleaseTemporary(result);
    }



    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        RenderTexture originalFrame = RenderTexture.GetTemporary(src.width, src.height, 24, src.format);
        RenderTexture styleFrame2 = RenderTexture.GetTemporary(src.width, src.height, 24, src.format);
        RenderTexture styleFrame3 = RenderTexture.GetTemporary(src.width, src.height, 24, src.format);
        Graphics.Blit(src, originalFrame);
        Graphics.Blit(src, styleFrame2);
        Graphics.Blit(src, styleFrame3);

        if (stylizeImage)
        {
            if (currentFrame % stylizeEveryNFrames == 0)
            {
                // Solo procesamos cada N frames
                Graphics.Blit(src, cachedStylizedFrame);  // copiar el frame original
                StylizeImageCustom(cachedStylizedFrame, engine);
                StylizeImageCustom(styleFrame2, engine2);
                StylizeImageCustom(styleFrame3, engine3);

                if (targetedStylization)
                    Merge(cachedStylizedFrame, styleFrame2, styleFrame3, originalFrame);
            }

            Graphics.Blit(cachedStylizedFrame, dest); // usar el resultado cacheado
            currentFrame++;
        }
        else
        {
            Graphics.Blit(src, dest);
        }

        RenderTexture.ReleaseTemporary(originalFrame);
        RenderTexture.ReleaseTemporary(styleFrame2);
        RenderTexture.ReleaseTemporary(styleFrame3);
    }


}
