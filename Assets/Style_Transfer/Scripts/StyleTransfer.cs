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


    //mejora que no llega a funcionar
   /*private RenderTexture lastStylizedFrame;
    private int frameCounter = 0;

    public float blendAlpha = 0.2f; // cuánto suaviza
    private Material blendMaterial;
    private RenderTexture blendTarget;*/
    


    void Start()
    {
       // blendMaterial = new Material(Shader.Find("Hidden/BlendLerp"));

        // Get the screen dimensions
        int width = Screen.width;
        int height = Screen.height;

        // Force the StyleDepth Camera to render to a Depth texture
        styleDepth.targetTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Depth);
        styleDepth.forceIntoRenderTexture = true;

        // Force the SourceDepth Camera to render to a Depth texture
        sourceDepth.targetTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Depth);
        sourceDepth.forceIntoRenderTexture = true;


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

       /* if (lastStylizedFrame != null)
        {
            lastStylizedFrame.Release();
            lastStylizedFrame = null;
        }*/
    }

    void Update()
    {

        if (styleDepth.targetTexture.width != Screen.width ||styleDepth.targetTexture.height != Screen.height)
        {
            // Get the screen dimensions
            int width = Screen.width;
            int height = Screen.height;
            // Assign depth textures with the new dimensions
            styleDepth.targetTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Depth);
            sourceDepth.targetTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Depth);
        }

        if (Input.GetKeyDown(KeyCode.Q)){
            stylizeImage = !stylizeImage;

        }
            
        if (Input.GetKeyDown(KeyCode.Z)){
            SceneManager.LoadScene("Scene");
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

        Graphics.Blit(src, rTex);
        ProcessImage(rTex, "ProcessInput");
        Tensor input = new Tensor(rTex, channels: 3);
        engine.Execute(input);
        Tensor prediction = engine.PeekOutput();
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
       // Create a temporary RenderTexture to store copy of the current frame
        RenderTexture sourceImage = RenderTexture.GetTemporary(src.width, src.height, 24, src.format);
        // Copy the current frame
        Graphics.Blit(src, sourceImage);

        if (stylizeImage){ 
            StylizeImage(src);
            if (targetedStylization){
                // Merge the stylized frame and original frame
                Merge(src, sourceImage);
            }    
        } 

        Graphics.Blit(src, dest);

        // Release the temporary RenderTexture
        RenderTexture.ReleaseTemporary(sourceImage);   
    
        /* if (stylizeImage){
            if (lastStylizedFrame == null || lastStylizedFrame.width != src.width || lastStylizedFrame.height != src.height)
            {
                if (lastStylizedFrame != null) lastStylizedFrame.Release();
                lastStylizedFrame = new RenderTexture(src.width, src.height, 24, src.format);
            }

            if (blendTarget == null || blendTarget.width != src.width || blendTarget.height != src.height)
            {
                if (blendTarget != null) blendTarget.Release();
                blendTarget = new RenderTexture(src.width, src.height, 24, src.format);
            }

            // Procesamos el frame actual
            StylizeImage(src);

            // Hacemos blending entre el anterior y el actual
            blendMaterial.SetTexture("_BlendTex", lastStylizedFrame);
            blendMaterial.SetFloat("_Alpha", blendAlpha);
            Graphics.Blit(src, blendTarget, blendMaterial);

            // Actualizamos el último frame
            Graphics.Blit(blendTarget, lastStylizedFrame);

            // Mostramos el resultado suavizado
            Graphics.Blit(lastStylizedFrame, dest);
        }else{
            Graphics.Blit(src, dest);
        }*/
    }

}
