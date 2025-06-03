using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DnaController : MonoBehaviour
{
    public GameObject objetActivable2;
    public GameObject objetActivable3;
    public GameObject objetActivable4;
    private int dnas=0;
    // Start is called before the first frame update
    void Start()
    {
        objetActivable2.SetActive(false);
        objetActivable3.SetActive(false);
        objetActivable4.SetActive(false);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void addDNAs(){
        dnas+=1;
        if(dnas==1){
            objetActivable2.SetActive(true);
        }else if(dnas==2){
            objetActivable3.SetActive(true);
        }else if(dnas==3){
            objetActivable4.SetActive(true);
        }
    }

    public int getDNAs(){
       return dnas;
    }
}
