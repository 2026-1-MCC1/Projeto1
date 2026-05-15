using UnityEngine;

public class CameraEditorLivre : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidade = 35f;
    public float multiplicadorSprint = 2f;
    public float multiplicadorLento = 0.4f;

    [Header("Rotacao")]
    public bool olharComBotaoDireito = true;
    public float sensibilidadeMouse = 2f;
    public bool inverterY = false;
    public float limitePitch = 80f;

    [Header("Zoom")]
    public float velocidadeZoom = 80f;
    public float alturaMinima = 3f;
    public float alturaMaxima = 200f;

    private float yaw;
    private float pitch;

    private void Start()
    {
        // Converte rotação inicial da câmera para yaw/pitch separados.
        Vector3 angulos = transform.eulerAngles;
        yaw = angulos.y;
        pitch = angulos.x > 180f ? angulos.x - 360f : angulos.x;
    }

    private void Update()
    {
        AtualizarRotacao();
        AtualizarMovimento();
        AtualizarZoom();
    }

    private void AtualizarMovimento()
    {
        float x = 0f;
        float z = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x += 1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) z += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) z -= 1f;

        Vector3 direcaoPlano = (transform.right * x) + (transform.forward * z);
        direcaoPlano.y = 0f;

        if (direcaoPlano.sqrMagnitude > 1f)
            direcaoPlano.Normalize();

        float multiplicador = 1f;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            multiplicador = multiplicadorSprint;
        else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            multiplicador = multiplicadorLento;

        Vector3 movimento = direcaoPlano * velocidade * multiplicador * Time.deltaTime;

        transform.position += movimento;

        // Segurança: mantém a câmera entre altura mínima e máxima.
        Vector3 pos = transform.position;
        pos.y = Mathf.Clamp(pos.y, alturaMinima, alturaMaxima);
        transform.position = pos;
    }

    private void AtualizarRotacao()
    {
        // Se estiver ligado, só gira câmera com botão direito pressionado.
        bool podeOlhar = !olharComBotaoDireito || Input.GetMouseButton(1);

        if (olharComBotaoDireito)
        {
            Cursor.lockState = podeOlhar ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !podeOlhar;
        }

        if (!podeOlhar)
            return;

        float mouseX = Input.GetAxis("Mouse X") * sensibilidadeMouse;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidadeMouse;

        yaw += mouseX;
        pitch += inverterY ? mouseY : -mouseY;
        pitch = Mathf.Clamp(pitch, -limitePitch, limitePitch);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private void AtualizarZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Approximately(scroll, 0f)) return;

        // Aproxima/afasta na direção que a câmera está olhando.
        transform.position += transform.forward * (scroll * velocidadeZoom * Time.deltaTime * 60f);

        Vector3 pos = transform.position;
        pos.y = Mathf.Clamp(pos.y, alturaMinima, alturaMaxima);
        transform.position = pos;
    }

    private void OnDisable()
    {
        if (!olharComBotaoDireito) return;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
