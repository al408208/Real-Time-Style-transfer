using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;
using UnityEngine.SceneManagement;

public class StyleTransfer : MonoBehaviour
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

    [Tooltip("The backend used when performing inference")]
    public WorkerFactory.Type workerType = WorkerFactory.Type.Auto;

    [Tooltip("Captures the depth data for the target GameObjects")]
    public Camera styleDepth;

    [Tooltip("Captures the depth data for the entire scene")]
    public Camera sourceDepth;

    private Model m_RuntimeModel;
    private IWorker engine;

    [Header("Temporal Blending")]
    bool showEdges = false;
    private RenderTexture previousStylizedFrame;
    [Range(0f, 1f)] public float blendFactor = 0.2f; // α
    bool enableTemporalBlending = false;    // <— controla el blending
    public int stylizeEveryNFrames = 2;
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

        // Force the SourceDepth Camera to render to a Depth texture
        sourceDepth.targetTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Depth);
        sourceDepth.forceIntoRenderTexture = true;

        cachedStylizedFrame = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBHalf);
        cachedStylizedFrame.Create();
        m_RuntimeModel = ModelLoader.Load(modelAsset);
        engine = WorkerFactory.CreateWorker(workerType, m_RuntimeModel);
    }

    private void OnDisable()
    {
        engine.Dispose();
        // Release the Depth texture for the StyleDepth camera
        RenderTexture.ReleaseTemporary(styleDepth.targetTexture);
        // Release the Depth texture for the SourceDepth camera
        RenderTexture.ReleaseTemporary(sourceDepth.targetTexture);
        // Libera el buffer del frame previo
        if (previousStylizedFrame != null)
        {
            previousStylizedFrame.Release();
            previousStylizedFrame = null;
        }
    }

    void Update()
    {

        if (styleDepth.targetTexture.width != Screen.width || styleDepth.targetTexture.height != Screen.height)
        {
            // Get the screen dimensions
            int width = Screen.width;
            int height = Screen.height;
            // Assign depth textures with the new dimensions
            styleDepth.targetTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Depth);
            sourceDepth.targetTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Depth);
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


    private void Merge(RenderTexture styleImage, RenderTexture sourceImage)
    {
        // Specify the number of threads on the GPU
        int numthreads = 8;
        // Get the index for the specified function in the ComputeShader
        int kernelHandle = styleTransferShader.FindKernel("Merge");
        // Define a temporary HDR RenderTexture
        int width = styleImage.width;
        int height = styleImage.height;
        RenderTexture result = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGBHalf);
        // Enable random write access
        result.enableRandomWrite = true;
        // Create the HDR RenderTexture
        result.Create();

        // Set the value for the Result variable in the ComputeShader
        styleTransferShader.SetTexture(kernelHandle, "Result", result);
        // Set the value for the InputImage variable in the ComputeShader
        styleTransferShader.SetTexture(kernelHandle, "InputImage", styleImage);
        // Set the value for the StyleDepth variable in the ComputeShader
        styleTransferShader.SetTexture(kernelHandle, "StyleDepth", styleDepth.activeTexture);
        // Set the value for the SrcDepth variable in the ComputeShader
        styleTransferShader.SetTexture(kernelHandle, "SrcDepth", sourceDepth.activeTexture);
        // Set the value for the SrcImage variable in the ComputeShader
        styleTransferShader.SetTexture(kernelHandle, "SrcImage", sourceImage);

        // Execute the ComputeShader
        styleTransferShader.Dispatch(kernelHandle, result.width / numthreads, result.height / numthreads, 1);

        // Copy the result into the source RenderTexture
        Graphics.Blit(result, styleImage);

        // Release the temporary RenderTexture
        RenderTexture.ReleaseTemporary(result);
    }

    private void StylizeImage(RenderTexture src)
    {
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

        RenderTexture originalNoStyle = RenderTexture.GetTemporary(rTex.width, rTex.height, 0, src.format);
        Graphics.Blit(src, originalNoStyle);

        Graphics.Blit(src, rTex);
        ProcessImage(rTex, "ProcessInput");
        Tensor input = new Tensor(rTex, channels: 3);
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

        // Blend suavizado
        // Graphics.Blit(Texture2D.blackTexture, previousStylizedFrame); // reinicia blending al negro

        if (previousStylizedFrame == null || previousStylizedFrame.width != rTex.width || previousStylizedFrame.height != rTex.height)
        {
            if (previousStylizedFrame != null) previousStylizedFrame.Release();

            previousStylizedFrame = new RenderTexture(rTex.width, rTex.height, 0, RenderTextureFormat.ARGBHalf);
            previousStylizedFrame.enableRandomWrite = true;
            previousStylizedFrame.Create();
            ClearPreviousStylizedFrame();
        }

        if (enableTemporalBlending)
        {
            BlendWithPrevious(rTex, originalNoStyle);
        }

        Graphics.Blit(rTex, src);
        RenderTexture.ReleaseTemporary(rTex);
        RenderTexture.ReleaseTemporary(originalNoStyle);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        // Create a temporary RenderTexture to store copy of the current frame
        RenderTexture sourceImage = RenderTexture.GetTemporary(src.width, src.height, 24, src.format);
        // Copy the current frame
        Graphics.Blit(src, sourceImage);

        if (stylizeImage)
        {
            if (currentFrame % stylizeEveryNFrames == 0)
            {
                Graphics.Blit(src, cachedStylizedFrame);  // copiar el frame original
                StylizeImage(cachedStylizedFrame);
                if (targetedStylization)
                {
                    // Merge the stylized frame and original frame
                    Merge(cachedStylizedFrame, sourceImage);
                }
            }
            Graphics.Blit(cachedStylizedFrame, dest); // usar el resultado cacheado
            currentFrame++;

        }
        else
        {

            Graphics.Blit(src, dest);
        }

        // Release the temporary RenderTexture
        RenderTexture.ReleaseTemporary(sourceImage);
    }

    private void BlendWithPrevious(RenderTexture current, RenderTexture originalNoStyle)
    {
        int kernel = styleTransferShader.FindKernel("TemporalBlendWithSobel");
        styleTransferShader.SetBool("ShowEdges", showEdges); // showEdges es tu variable pública

        RenderTexture blended = RenderTexture.GetTemporary(current.width, current.height, 0, RenderTextureFormat.ARGBHalf);
        blended.enableRandomWrite = true;
        blended.Create();

        // Setear texturas
        styleTransferShader.SetTexture(kernel, "Result", blended);
        styleTransferShader.SetTexture(kernel, "InputImage", current);               // imagen ya estilizada
        styleTransferShader.SetTexture(kernel, "PreviousImage", previousStylizedFrame); // último frame con estilo
        styleTransferShader.SetTexture(kernel, "EdgeSourceImage", originalNoStyle);  // imagen sin estilo
        styleTransferShader.SetFloat("BlendFactor", blendFactor);

        styleTransferShader.Dispatch(kernel, current.width / 8, current.height / 8, 1);

        Graphics.Blit(blended, current);
        Graphics.Blit(current, previousStylizedFrame); //current o blended??

        RenderTexture.ReleaseTemporary(blended);
    }
    private void ClearPreviousStylizedFrame()
    {
        var activeRT = RenderTexture.active;
        RenderTexture.active = previousStylizedFrame;
        GL.Clear(true, true, Color.black); // o usa Color.clear si prefieres transparente
        RenderTexture.active = activeRT;
    }
    public void SetTargetHeight(int newHeight)
    {
        targetHeight = newHeight;
        ClearPreviousStylizedFrame(); // Esto fuerza el recálculo en el próximo frame
    }
    public int GetTargetHeight()
    {
        return targetHeight;
    }
    public void SetBlended(bool blend, bool edges)
    {
        enableTemporalBlending = blend;
        showEdges = edges;
        ClearPreviousStylizedFrame(); // Esto fuerza el recálculo en el próximo frame
    }
    public void OnImageClicked()
    {
        stylizeImage = !stylizeImage;
    }

}
