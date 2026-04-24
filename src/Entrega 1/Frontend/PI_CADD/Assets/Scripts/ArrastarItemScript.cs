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

    [Header("Quantidade")]
    public int quantidadeMaxima = 3;             // Quantidade máxima disponível deste item
    private int quantidadeAtual;                 // Quantidade restante em tempo real

    private GameObject objetoArrastando;         // Referência ao objeto sendo arrastado
    private bool arrastando = false;             // Controla se está arrastando no momento

    void Start()
    {
        quantidadeAtual = quantidadeMaxima;      // Inicializa com a quantidade máxima
    }

    // Chamado UMA VEZ quando o dedo/mouse começa a arrastar
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (quantidadeAtual <= 0) return;        // Bloqueia se não tiver mais itens disponíveis

        arrastando = true;                       // Marca que está arrastando

        objetoArrastando = Instantiate(prefabDoItem); // Cria uma cópia do prefab na cena
        objetoArrastando.transform.localScale = escalaPadrao;                   // Aplica a escala
        objetoArrastando.transform.rotation = Quaternion.Euler(rotacaoInicial); // Aplica a rotação

        // Desativa física e collider enquanto arrasta para não interferir nos carros
        Rigidbody rb = objetoArrastando.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;               // Desativa a física enquanto arrasta
            rb.useGravity = false;               // Desativa gravidade enquanto arrasta
        }

        // Desativa o collider enquanto arrasta para não buguar com os carros
        Collider col = objetoArrastando.GetComponent<Collider>();
        if (col != null)
            col.enabled = false;                 // Collider desligado — carros passam por cima sem bug
    }

    // Chamado A CADA FRAME enquanto o dedo/mouse estiver arrastando
    public void OnDrag(PointerEventData eventData)
    {
        if (!arrastando || objetoArrastando == null) return; // Segurança

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // Raio da câmera ao mouse
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))       // Se o raio acertou algum collider na cena
        {
            Vector3 pos = hit.point + offsetPosicao; // Posição do hit + offset

            Collider col = objetoArrastando.GetComponent<Collider>();
            pos.y = (col != null ? col.bounds.extents.y : 0.5f) + offsetPosicao.y; // Altura correta

            objetoArrastando.transform.position = pos; // Move o objeto
        }
    }

    // Chamado UMA VEZ quando o dedo/mouse solta
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!arrastando || objetoArrastando == null) return; // Segurança

        arrastando = false;                      // Marca que parou de arrastar

        Rigidbody rb = objetoArrastando.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;              // Reativa a física ao soltar
            rb.useGravity = true;                // Reativa gravidade ao soltar
        }

        // Reativa o collider DEPOIS de um pequeno delay para não buguar com carros em movimento
        Collider col = objetoArrastando.GetComponent<Collider>();
        if (col != null)
            StartCoroutine(ReativarCollider(col)); // Reativa com delay via Coroutine

        quantidadeAtual--;                       // Desconta uma unidade do estoque
        AtualizarUI();                           // Atualiza o visual da quantidade

        objetoArrastando = null;                 // Limpa a referência
    }

    // Reativa o collider após um pequeno delay para evitar bug com carros
    System.Collections.IEnumerator ReativarCollider(Collider col)
    {
        yield return new WaitForSeconds(0.5f);   // Espera 0.5s antes de reativar
        if (col != null)
            col.enabled = true;                  // Reativa o collider
    }

    void AtualizarUI()
    {
        // Desativa o botão visualmente se acabou o estoque
        UnityEngine.UI.Image imagem = GetComponent<UnityEngine.UI.Image>();
        if (imagem != null)
            imagem.color = quantidadeAtual <= 0
                ? new Color(1, 1, 1, 0.3f)      // Transparente quando sem estoque
                : new Color(1, 1, 1, 1f);        // Normal quando tem estoque

        // Log para debug — remove depois
        Debug.Log($"{prefabDoItem.name}: {quantidadeAtual}/{quantidadeMaxima} restantes");
    }
}