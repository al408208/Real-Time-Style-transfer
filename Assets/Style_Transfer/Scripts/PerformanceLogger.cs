using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using UnityEngine.Profiling;
using Tayx.Graphy.Fps;
using Tayx.Graphy.Ram;
public class PerformanceLogger : MonoBehaviour
{

    private List<string> logLines = new List<string>();
    G_FpsText graphyfps;
    G_RamText graphyMemory;

    void Start()
    {
    //logLines.Add("Frame;Time(s);FPS;FrameTimeMs");
    logLines.Add("Frame;Time(s);FPS;FrameTimeMs;MonoRamMB;AllocatedRamMB;ReservedRamMB");

        graphyfps = FindObjectOfType<G_FpsText>();
        graphyMemory = FindObjectOfType<G_RamText>();
        
    }

    void Update()
    {

        float ms = graphyfps.getMS();

        // FPS y ms
        float fps = graphyfps.getfps();
        float reserved = graphyMemory.getReserved();
        float allocated = graphyMemory.getAllocated();
        float mono = graphyMemory.getMono();

        // Guardar datos
        string line = string.Format("{0};{1:0.00};{2:0.00};{3:0.00};{4:0.00};{5:0.00};{6:0.00}",
                    Time.frameCount, Time.time,fps, ms, mono, allocated, reserved);
        //string line = string.Format("{0};{1:0.00};{2:0.00};{3:0.00};{4:0.00};{5:0.00};{6:0.00}",Time.frameCount,Time.time, fps, ms, mono, allocated, reserved);
        logLines.Add(line);

        // Salida con Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SaveToCSV();
        }
    }

    void OnApplicationQuit()
    {
        SaveToCSV();
    }

    void SaveToCSV()
    {
        string folderPath = Application.dataPath + "/Logs";
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

        string filePath = folderPath + "/performance_log_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
        File.WriteAllLines(filePath, logLines.ToArray());
        Debug.Log("Datos de rendimiento guardados en: " + filePath);
    }
}
