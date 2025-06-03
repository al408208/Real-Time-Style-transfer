using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MensajeInicio : MonoBehaviour
{
    
    public TextMeshProUGUI message;
    public GameObject player;

    // Start is called before the first frame update
    
    void OnTriggerEnter (Collider other)
    {
        if (other.gameObject == player)
        {
            SetInfoText("Busca la munición para eliminar al zombie");
            Invoke("ClearInfoText", 2f);
        }
    }
    public void SetInfoText(string newInfo)
    {        
        message.text = newInfo.ToString();
    }

    public void ClearInfoText()
    {
        message.text = ""; // Borra el texto del mensaje
        Destroy(gameObject);
        
    }
}
