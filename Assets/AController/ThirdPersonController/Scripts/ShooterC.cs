using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using StarterAssets;
using UnityEngine.InputSystem;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ShooterC : MonoBehaviour
{
   
    [SerializeField] private Rig aimrig;
    [SerializeField] private CinemachineVirtualCamera aimVirtualCamera;
    [SerializeField] private float normalSensitivity;
    [SerializeField] private float aimSensitivity;   
    [SerializeField] private LayerMask aimColliderLayerMask=new LayerMask();  
    [SerializeField] private Transform debugTransform;
    [SerializeField] private Transform pfBullet;
    [SerializeField] private Transform pfBullet2;
    [SerializeField] private Transform spawnbulletp;
    public GameObject objetActivable1;
    public AudioSource sound;
    public AudioSource sound2;
    public AudioSource noAmmo;

    public TextMeshProUGUI info;
    private int municion=0;
    [SerializeField] private float tiempoMaximo; 
    [SerializeField] private Slider slider; 
    public float tiempoActual; 
    
    private Animator animator;
    private float aimWeight;

    private StarterAssetsInputs starterAssetsInputs;
    private ThirdPersonController thirdPersonController;

    private void Awake(){
        starterAssetsInputs=GetComponent<StarterAssetsInputs>();
        thirdPersonController=GetComponent<ThirdPersonController>();
        animator=GetComponent<Animator>();
        objetActivable1.SetActive(false);
        ActivarTemporizador();
    }
    private void Update(){
        

        if (Input.GetKeyDown(KeyCode.K))
        {
            ScreenCapture.CaptureScreenshot("012.png");
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            SceneManager.LoadScene("Realistic Demo");
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            SceneManager.LoadScene("Example_01");
        }
        //tiempoActual-=Time.deltaTime;
        //if(tiempoActual>=0){
        slider.value=tiempoActual;
        //}

        Vector3 mousePosition=Vector3.zero;
        Vector2 screenPoint=new Vector2(Screen.width/2f,Screen.height/2f);
        Ray ray=Camera.main.ScreenPointToRay(screenPoint);
        if(Physics.Raycast(ray,out RaycastHit raycastHit,999f,aimColliderLayerMask)){
           debugTransform.position=raycastHit.point;
            mousePosition=raycastHit.point;
        }

        if(starterAssetsInputs.aim){
            aimVirtualCamera.gameObject.SetActive(true);
            thirdPersonController.SetSensitivity(aimSensitivity);
            aimWeight=1f;
            thirdPersonController.SetRotateOnMove(false);
            animator.SetLayerWeight(1,Mathf.Lerp(animator.GetLayerWeight(1), 1f, Time.deltaTime*10f));

            Vector3 worldAimTarget=mousePosition;
            worldAimTarget.y=transform.position.y;
            Vector3 aimDirection=(worldAimTarget-transform.position).normalized;

            transform.forward=Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime*20f);
           
            if(starterAssetsInputs.shoot){
                
                if(municion>0){
                    tiempoActual+=2.5f; //cuanto sumo para el super disparo
                    municion-=1;
                    SetInfoText(municion+"/10");
                    
                    Vector3 aimDir=(mousePosition-spawnbulletp.position).normalized;
                    if(tiempoActual==20f){//siguiente es super
                        objetActivable1.SetActive(true);

                    }
                    if(tiempoActual==22.5f){//lo actual mas uno mas lo que sumo es que ese disparo es super 
                        sound2.Play();
                        objetActivable1.SetActive(false);
                        Instantiate(pfBullet, spawnbulletp.position,Quaternion.LookRotation(aimDir,Vector3.up));
                        tiempoActual=0f;
                    }else{
                        sound.Play();
                        Instantiate(pfBullet2, spawnbulletp.position,Quaternion.LookRotation(aimDir,Vector3.up));
                    }                
                
                }else{
                    noAmmo.Play();
                }     
                starterAssetsInputs.shoot=false;
            }
            
        }else{
            aimVirtualCamera.gameObject.SetActive(false);
            thirdPersonController.SetSensitivity(normalSensitivity);
            aimWeight=0f;
            
            thirdPersonController.SetRotateOnMove(true);
            
            animator.SetLayerWeight(1,Mathf.Lerp(animator.GetLayerWeight(1), 0f, Time.deltaTime*10f));
        }

        
        aimrig.weight=Mathf.Lerp(aimrig.weight,aimWeight,Time.deltaTime*20f);
        
    }

    public void SetInfoText(string newInfo)
	{        
		info.text =newInfo.ToString();
        
	}

    public void ActivarTemporizador(){
        tiempoActual=tiempoMaximo;
        slider.maxValue=20f;
    }
    public void addBalas(){
        municion+=5;
        if(municion>10){
            municion=10;
        }
        SetInfoText(municion+"/10");
    }
   
}
