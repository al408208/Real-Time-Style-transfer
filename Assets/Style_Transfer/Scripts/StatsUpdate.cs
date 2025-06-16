using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StatsUpdate : MonoBehaviour
{
    // Start is called before the first frame update
    public TextMeshProUGUI Hunit;
    public TextMeshProUGUI Wunit;
    public TMP_InputField inputField; // Asigna desde el Inspector
    private Vector2 lastResolution;
    public int menuTargetHeight = 540;
    public StyleTransferDobleEstilo styleTransfer;
    public Toggle blendingToggle; // Asigna en el Inspector
    public Toggle edgesToggle; // Asigna en el Inspector
    public Slider blendSlider;

    public Text consoleText; // Asigna el Text dentro de "Content"
    public ScrollRect scrollRect; // Asigna el ScrollRect de "Console View"
    public int maxLines = 50;

    void Start()
    {
        UpdateResolutionLabel();
        lastResolution = new Vector2(Screen.width, Screen.height);

        blendingToggle.onValueChanged.AddListener(OnSettingsChanged);
        edgesToggle.onValueChanged.AddListener(OnSettingsChanged);

        blendSlider.onValueChanged.AddListener(OnBlendValueChanged);
        blendSlider.value = styleTransfer.blendFactor;
    }

    // Update is called once per frame
    void Update()
    {
        if (Screen.width != lastResolution.x || Screen.height != lastResolution.y)
        {
            UpdateResolutionLabel();
            lastResolution = new Vector2(Screen.width, Screen.height);
        }
    }

    void UpdateResolutionLabel()
    {
        Hunit.text = Screen.height.ToString();
        Wunit.text = Screen.width.ToString();
    }

    public void UpdateClicked()
    {
        if (int.TryParse(inputField.text, out int menuTargetHeight))
        {
            if (styleTransfer.targetHeight != menuTargetHeight)
            {
                if (menuTargetHeight > 540)
                {
                    styleTransfer.SetTargetHeight(540);
                }
                else if (menuTargetHeight < 0)
                {
                    styleTransfer.SetTargetHeight(0);
                }
                else
                {
                    styleTransfer.SetTargetHeight(menuTargetHeight);
                }
            }
        }
        else
        {
            Debug.LogWarning("El texto introducido no es un número válido.");
        }
    }

    void OnSettingsChanged(bool _)//sé que hay un parámetro, pero no lo necesito
    {
        bool enableBlending = blendingToggle.isOn;
        bool showEdges = edgesToggle.isOn;

        styleTransfer.SetBlended(enableBlending, showEdges);
    }

    void OnBlendValueChanged(float value)
    {
        styleTransfer.blendFactor = value;
    }



    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        consoleText.text += "\n" + logString;

        // Limitar número de líneas
        string[] lines = consoleText.text.Split('\n');
        if (lines.Length > maxLines)//Se borran lineas antiguas
        {
            consoleText.text = string.Join("\n", lines, lines.Length - maxLines, maxLines);
        }

        // Desplazar automáticamente hacia abajo
        Canvas.ForceUpdateCanvases(); // fuerza actualización del layout
        scrollRect.verticalNormalizedPosition = 1f; // 0 = abajo
        
    }
}
