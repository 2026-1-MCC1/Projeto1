using UnityEngine;
using UnityEngine.EventSystems;

public class ArrastarItemScript : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject prefabDoItem;  // Prefab que será instanciado ao arrastar (atribuído no Inspector)

    [Header("Escala")]
    public Vector3 escalaPadrao = new Vector3(0.5f, 0.5f, 0.5f); // Tamanho inicial do objeto instanciado

    [Header("Rotação Inicial")]
    public Vector3 rotacaoInicial = Vector3.zero; // Rotação em Euler (graus) ao instanciar — (0,0,0) = sem rotação

    [Header("Offset de Posição")]
    public Vector3 offsetPosicao = Vector3.zero; // Deslocamento extra aplicado sobre o ponto de hit

    private GameObject objetoArrastando; // Referência ao objeto sendo arrastado (invisível no Inspector)

    // Chamado UMA VEZ quando o dedo/mouse começa a arrastar
    public void OnBeginDrag(PointerEventData eventData)
    {
        objetoArrastando = Instantiate(prefabDoItem); // Cria uma cópia do prefab na cena

        objetoArrastando.transform.localScale = escalaPadrao;                   // Aplica a escala definida no Inspector
        objetoArrastando.transform.rotation = Quaternion.Euler(rotacaoInicial); // Converte Euler → Quaternion e aplica a rotação

        Rigidbody rb = objetoArrastando.GetComponent<Rigidbody>(); // Tenta pegar o Rigidbody do objeto criado
        if (rb != null)        // Só executa se o prefab tiver Rigidbody
        {
            rb.isKinematic = true;  // Desativa a física — objeto não responde a forças, só ao código
            rb.useGravity = false;  // Desativa gravidade para não cair enquanto arrasta
        }
    }

    // Chamado A CADA FRAME enquanto o dedo/mouse estiver arrastando
    public void OnDrag(PointerEventData eventData)
    {
        // Cria um raio saindo da câmera em direção ao ponto do mouse/toque na tela
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit; // Struct que vai guardar as informações do que o raio acertou

        if (Physics.Raycast(ray, out hit)) // Dispara o raio — entra no if se acertou algum Collider na cena
        {
            Vector3 pos = hit.point + offsetPosicao; // Pega o ponto de colisão e soma o offset configurado no Inspector

            Collider col = objetoArrastando.GetComponent<Collider>(); // Pega o Collider do objeto arrastado
            pos.y = (col != null ? col.bounds.extents.y : 0.5f) + offsetPosicao.y; // Altura base do collider + offset Y

            objetoArrastando.transform.position = pos; // Move o objeto para a posição calculada
        }
    }

    // Chamado UMA VEZ quando o dedo/mouse solta
    public void OnEndDrag(PointerEventData eventData)
    {
        if (objetoArrastando != null) // Segurança: garante que ainda existe um objeto sendo arrastado
        {
            Rigidbody rb = objetoArrastando.GetComponent<Rigidbody>(); // Recupera o Rigidbody
            if (rb != null)       // Só executa se tiver Rigidbody
            {
                rb.isKinematic = false; // Reativa a física — objeto volta a responder a forças
                rb.useGravity = true;   // Reativa gravidade — objeto cai normalmente ao ser solto
            }
        }

        objetoArrastando = null; // Limpa a referência (boa prática para evitar memory leak)
    }
}