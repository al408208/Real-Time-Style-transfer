using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPortal : MonoBehaviour
{
    // Variables de movimiento y cámara
    public float horizontalCamSpeed = 2f;
    public float verticalCamSpeed = 2f;
    public float velocidad = 5f;
    public float multiplicadorVelocidad = 1f;

    // Variables de salto y gravedad
    public float velocidadSalto = 5f;
    public float fuerzaGravedad = 1f;

    // Referencias a componentes
    public Camera FPSCamera;
    private Rigidbody Rbody;

    [System.Serializable]
    public struct Control
    {
        public KeyCode tecla;
        public Vector3 direccion;
    }

    public Control[] MisControles = new Control[4];

    void Start()
    {
        Rbody = GetComponent<Rigidbody>();

        // Inicializar controles WASD
        
    }

    void FixedUpdate()
    {
        // Girar la cámara para mirar:
        float h = horizontalCamSpeed * Input.GetAxis("Mouse X") * Time.fixedDeltaTime;
        float v = verticalCamSpeed * Input.GetAxis("Mouse Y") * Time.fixedDeltaTime;

        transform.Rotate(0, h, 0);
        FPSCamera.transform.Rotate(-v, 0, 0);

        // Pulsar Shift para correr:
        multiplicadorVelocidad = Input.GetKey(KeyCode.LeftShift) ? 2f : 1f;

        // Movimiento con WASD
        Vector3 movimientoTotal = transform.position;
        for (int i = 0; i < MisControles.Length; i++)
        {
            if (Input.GetKey(MisControles[i].tecla))
            {
                movimientoTotal += transform.TransformDirection(MisControles[i].direccion) * velocidad * multiplicadorVelocidad * Time.deltaTime;
            }
        }

        Rbody.MovePosition(movimientoTotal);

        // Saltar con espacio:
        if (Input.GetKeyDown(KeyCode.Space) && Mathf.Abs(Rbody.velocity.y) < 0.01f)
        {
            Rbody.AddForce(Vector3.up * velocidadSalto, ForceMode.Impulse);
        }

        // Aplicar gravedad manualmente:
        Rbody.AddForce(Vector3.down * 9.81f * fuerzaGravedad, ForceMode.Acceleration);

        // Recuperar el cursor al pulsar Esc:
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
