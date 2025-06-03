using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Puerta : MonoBehaviour
{

    public GameObject objetActivable1;
    public GameObject objetActivable2;
    private int totaldnas;
    public GameObject player;
    public TextMeshProUGUI message;
   [SerializeField] private DnaController dnaController;  
    // Start is called before the first frame update
    void Start()
    {
        dnaController=FindObjectOfType<DnaController>();
    }

    // Update is called once per frame
    void Update()
    {
        totaldnas=dnaController.getDNAs();

        if(totaldnas==3){
            objetActivable2.SetActive(false);
            objetActivable1.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player && totaldnas < 3)
        {
            SetInfoText("RECOGE LOS 3 DNAS");

            Invoke("ClearInfoText", 3f); // Llama al método "ClearInfoText" después de 2 segundos
        }
    }

    public void SetInfoText(string newInfo)
    {        
        message.text = newInfo.ToString();
    }

    public void ClearInfoText()
    {
        message.text = ""; // Borra el texto del mensaje
    }
}
