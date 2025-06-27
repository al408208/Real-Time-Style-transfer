using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using StarterAssets;


public class MenuManager : MonoBehaviour
{
    public RectTransform aux;
    public GameObject menu;
    public GameObject hud;
    public GameObject carrousel;
    public GameObject fondo;
    private bool puedeMoverse = true; // Variable para controlar el input
    private bool menuactive=false; 

    [SerializeField]
    private int max=5;
    private int styleIn=0; //overline es simplemente hasta donde puede ir el cuadro, de 0 a 2 y el estilo es en el que estamos
    private int styleSelected=0;

    public RawImage rawImage1;
    public RawImage rawImage2;
    public RawImage rawImage3;

    public RawImage AnimrawImage0;
    public RawImage AnimrawImage1;
    public RawImage AnimrawImage2;
    public RawImage AnimrawImage3;
    public RawImage AnimrawImage4;

    public Texture2D[] imagenes; 

    private Animator animator;
    private Animator animatorBordes;
    private Animator animatorBordes2;

    private Animator animatorLado1;
    private Animator animatorLado2;

    private PlayerInput playerInput;

    void Start()//OPCION ARCAICA
    {
        //OPCION ARCAICA
        //rawImage1.texture = CargarImagen("3");  // Busca "1.png" o "1.jpg" en Resources
        //rawImage2.texture = CargarImagen("2");
        //rawImage3.texture = CargarImagen("1");

        animator = AnimrawImage2.GetComponent<Animator>();
        animatorLado1 = AnimrawImage1.GetComponent<Animator>();
        animatorLado2 = AnimrawImage3.GetComponent<Animator>();
        animatorBordes = AnimrawImage0.GetComponent<Animator>();
        animatorBordes2 = AnimrawImage4.GetComponent<Animator>();
        
       playerInput = FindObjectOfType<ThirdPersonController>().GetComponent<PlayerInput>();
    }

    Texture2D CargarImagen(string nombreArchivo)
    {
        return Resources.Load<Texture2D>(nombreArchivo);
    }

    // Update is called once per frame
    void Update()
    { 
        
        //if (Input.GetKeyDown(KeyCode.A)){
            //ScreenCapture.CaptureScreenshot("SomeLevel1.png");
        //}

        if (!puedeMoverse) return; // Bloquea el input mientras la animación está activa

        
        if (Input.GetKeyDown(KeyCode.M))
        {
            menuactive = !menuactive;

            if (menuactive)
            {
                aux.anchoredPosition = new Vector2(aux.anchoredPosition.x, 0);
                menu.SetActive(true);
                carrousel.SetActive(true);
                fondo.SetActive(true);
                hud.SetActive(false);
                playerInput.enabled = false; // Desactiva input del jugador

            }
            else
            {
                hud.SetActive(true);
                menu.SetActive(false);
                carrousel.SetActive(false);
                fondo.SetActive(false);
                playerInput.enabled = true; // Desactiva input del jugador
            }
        }

        if (Input.GetKeyDown(KeyCode.Return) && menuactive)
        {
            styleSelected = styleIn;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) && menuactive)
        {
            puedeMoverse = false; // Bloquea las teclas
            menu.SetActive(false);
            //imgDesactivada.SetActive(true);
            

            animator.SetInteger("Direccion", -1);  
            animatorLado1.SetInteger("Direccion", -1);
            animatorLado2.SetInteger("Direccion", -1);  
            animatorBordes2.SetInteger("Direccion", -1);   

            if(styleIn==max){//si parto del maximo
                styleIn=0;
                rawImage1.texture = imagenes[max]; 
                rawImage2.texture = imagenes[styleIn];
                rawImage3.texture = imagenes[styleIn+1];
            }        
            else{
                styleIn++;
                if(styleIn==max){//si hemos pulsado y nos colocamos en el maximo
                    rawImage1.texture = imagenes[styleIn-1]; 
                    rawImage2.texture = imagenes[styleIn];
                    rawImage3.texture = imagenes[0];
                }else{
                    rawImage1.texture = imagenes[styleIn-1]; 
                    rawImage2.texture = imagenes[styleIn];
                    rawImage3.texture = imagenes[styleIn+1];
                }
            }
            
            Invoke("Reset", 0.8f);
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) && menuactive)
        {
            puedeMoverse = false; // Bloquea las teclas
            menu.SetActive(false);
            //imgDesactivada.SetActive(true);

            animator.SetInteger("Direccion", 1); 
            animatorLado1.SetInteger("Direccion", 1);  
            animatorLado2.SetInteger("Direccion", 1);   
            animatorBordes.SetInteger("Direccion", 1);

            if(styleIn==0){
                styleIn=max;
                rawImage1.texture = imagenes[styleIn-1]; 
                rawImage2.texture = imagenes[styleIn];
                rawImage3.texture = imagenes[0];
            }
            else{
                styleIn--;
                if(styleIn==0){
                    rawImage1.texture = imagenes[max]; 
                    rawImage2.texture = imagenes[styleIn];
                    rawImage3.texture = imagenes[styleIn+1];
                }else{
                    rawImage1.texture = imagenes[styleIn-1]; 
                    rawImage2.texture = imagenes[styleIn];
                    rawImage3.texture = imagenes[styleIn+1];
                }
            }
            Invoke("Reset", 0.8f);
        }
    }

    void Reset(){

        menu.SetActive(true); 
        //imgDesactivada.SetActive(false);
        animator.SetInteger("Direccion", 0);  
        animatorBordes.SetInteger("Direccion", 0);  
        animatorBordes2.SetInteger("Direccion", 0);  
        animatorLado1.SetInteger("Direccion", 0);  
        animatorLado2.SetInteger("Direccion", 0); 
        
        if(styleIn+1==max){//vamos al maximo en la siguiente
            AnimrawImage0.texture=imagenes[styleIn-2];
            AnimrawImage1.texture = imagenes[styleIn-1]; 
            AnimrawImage2.texture = imagenes[styleIn];
            AnimrawImage3.texture = imagenes[max];
            AnimrawImage4.texture= imagenes[0];
            
        }else if(styleIn-1==0){ 
            AnimrawImage0.texture=imagenes[max];
            AnimrawImage1.texture = imagenes[0]; 
            AnimrawImage2.texture = imagenes[styleIn];
            AnimrawImage3.texture = imagenes[styleIn+1];
            AnimrawImage4.texture= imagenes[styleIn+2];

        }else if(styleIn==max){//hemos llegado al maximo
            AnimrawImage0.texture=imagenes[styleIn-2];
            AnimrawImage1.texture = imagenes[styleIn-1]; 
            AnimrawImage2.texture = imagenes[styleIn];
            AnimrawImage3.texture = imagenes[0];
            AnimrawImage4.texture= imagenes[0+1];
            
        }else if(styleIn==0){ 
            AnimrawImage0.texture=imagenes[max-1];
            AnimrawImage1.texture = imagenes[max]; 
            AnimrawImage2.texture = imagenes[styleIn];
            AnimrawImage3.texture = imagenes[styleIn+1];
            AnimrawImage4.texture= imagenes[styleIn+2];
        }else{
            AnimrawImage0.texture=imagenes[styleIn-2];
            AnimrawImage1.texture = imagenes[styleIn-1]; 
            AnimrawImage2.texture = imagenes[styleIn];
            AnimrawImage3.texture = imagenes[styleIn+1];
            AnimrawImage4.texture= imagenes[styleIn+2];
        }
       
        puedeMoverse = true; // Bloquea las teclas
    }
   

    public int getStyle(){
        return styleSelected;
    }

    public bool getMenuActive(){
        return menuactive;
    }
    public bool getPuedeMoverse(){
        return puedeMoverse;
    }

    public void OnImageClicked()
    {
        styleSelected = -1;
    }

    public void closeMenu(){
        menuactive=false;
        playerInput.enabled = true;
        menu.SetActive(false);
        hud.SetActive(true);
        //si yo desactivo el carrousel no da tiempo a reiniciar animaciones y se queda con mal diseño, necesito el carrousel siempre activo
        aux.anchoredPosition = new Vector2(aux.anchoredPosition.x, 500);
        //carrousel.SetActive(false);
    }    

}
