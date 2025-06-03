using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class transciones : MonoBehaviour
{

    [SerializeField] string scene;
    public GameObject player;
    // Start is called before the first frame update
    void OnTriggerEnter (Collider other)
    {
        if (other.gameObject == player)
        {
            SceneManager.LoadScene(scene);
        }
    }

}
