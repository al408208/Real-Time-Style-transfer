﻿using System.Collections;
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
    private IWorker currentEngine = null; // null o -1 = Sin modelo, 0+ = Modelos cargados

    [SerializeField]
    private MenuManager menuManager; // Referencia al script MenuManager

    private bool imgclick = false;

    [Header("Temporal Blending")]
    public bool showEdges = false;
    private RenderTexture previousStylizedFrame;
    public bool enableTemporalBlending = true;  
    [Range(0f, 1f)] public float blendFactor = 0.2f; // α

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
        
        if (previousStylizedFrame != null)
        {
            previousStylizedFrame.Release();
            previousStylizedFrame = null;
        }
    }

    void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Return) && menuManager.getMenuActive()) || imgclick)
        {
            if (!menuManager.getPuedeMoverse()) return; // Bloquea el input mientras la animación está activa

            int selectedStyle = menuManager.getStyle(); // Obtener el estilo seleccionado desde MenuManager

            if (selectedStyle >= 0 && selectedStyle < engines.Length)
            {
                currentEngine = engines[selectedStyle]; // Activar el modelo seleccionado
                ClearPreviousStylizedFrame();
            }
            else
            {
                currentEngine = null; // Si es -1 o fuera de rango, desactivar estilizado
            }
            imgclick = false;
            menuManager.closeMenu();
        }

        //currentModelIndex = (currentModelIndex + 1) % (modelAssets.Length + 1); // Ciclo entre 0 (sin modelo) y los modelos cargados


    }

    private void StylizeImage(RenderTexture src)
    {
        if (currentEngine == null) return; // No aplicar si no hay modelo activo

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

        RenderTexture originalNoStyle = RenderTexture.GetTemporary(rTex.width, rTex.height, 0, src.format);
        Graphics.Blit(src, originalNoStyle);

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
        // Copy rTex into src
        Graphics.Blit(rTex, src);

        // Release the temporary RenderTexture
        RenderTexture.ReleaseTemporary(rTex);
        RenderTexture.ReleaseTemporary(originalNoStyle);
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
        imgclick = true;
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
}