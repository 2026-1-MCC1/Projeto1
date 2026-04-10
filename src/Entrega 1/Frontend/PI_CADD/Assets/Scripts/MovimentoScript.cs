using UnityEngine;

public class MovimentoScript : MonoBehaviour
{
    // SerializeField permite alterar os valores de moveSpeed diretamente na Unity.
    [SerializeField] float moveSpeed = 10f;
    void Start()
    {

    }
    void Update()
    {
        Movimentacao();
    }
    // Função para movimentar o Fugitivo/Policial usando as teclas WASD ou as setas do teclado.
    void Movimentacao()
    {
        float xValue = Input.GetAxis("Horizontal") * Time.deltaTime * moveSpeed;
        float yValue = 0f;
        float zValue = Input.GetAxis("Vertical") * Time.deltaTime * moveSpeed;
        transform.Translate(xValue, yValue, zValue);
    }
}