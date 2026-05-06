using UnityEngine;
using UnityEngine.EventSystems;

public class ArrastarItemScript : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject prefabDoItem;  // Prefab que será instanciado ao arrastar (atribuído no Inspector)

    [Header("Escala")]
    public Vector3 escalaPadrao = new Vector3(0.5f, 0.5f, 0.5f); // Tamanho inicial do objeto instanciado

    [Header("Rotação Inicial")]
    public Vector3 rotacaoInicial = Vector3.zero; // Rotação em Euler (graus) ao instanciar — (0,0,0) = sem rotação

    [Header("Rotação Durante Arraste")]
    public bool permitirRotacaoDuranteArraste = true; // Permite rotacionar o item antes de soltar
    public float velocidadeRotacaoArraste = 140f;     // Graus por segundo ao segurar as teclas
    public KeyCode teclaRotacionarEsquerda = KeyCode.Q;
    public KeyCode teclaRotacionarDireita = KeyCode.E;

    [Header("Offset de Posição")]
    public Vector3 offsetPosicao = Vector3.zero; // Deslocamento extra aplicado sobre o ponto de hit

    [Header("Quantidade")]
    public int quantidadeMaxima = 3;             // Quantidade máxima disponível deste item
    private int quantidadeAtual;                 // Quantidade restante em tempo real

    [Header("Planejamento")]
    public bool registrarNoPlanejamento = false; // Liga para salvar os itens colocados antes da perseguição
    public bool manterObjetoEstaticoAoSoltar = false; // Se true, não ativa física no objeto ao soltar

    private GameObject objetoArrastando;         // Referência ao objeto sendo arrastado
    private bool arrastando = false;             // Controla se está arrastando no momento
    private bool tevePosicionamentoValido = false;
    private bool posicionamentoAtualValido = false;
    private Camera cameraPrincipal;

    void Start()
    {
        quantidadeAtual = quantidadeMaxima;      // Inicializa com a quantidade máxima
        cameraPrincipal = Camera.main;
    }

    void Update()
    {
        if (!arrastando || objetoArrastando == null) return;
        AtualizarRotacaoDuranteArraste();
    }

    // Chamado UMA VEZ quando o dedo/mouse começa a arrastar
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (quantidadeAtual <= 0 || prefabDoItem == null) return; // Bloqueia sem item ou sem prefab

        if (cameraPrincipal == null)
            cameraPrincipal = Camera.main;

        if (cameraPrincipal == null)
        {
            Debug.LogError("ArrastarItemScript: nenhuma câmera principal encontrada para raycast.");
            return;
        }

        arrastando = true;                       // Marca que está arrastando
        tevePosicionamentoValido = false;
        posicionamentoAtualValido = false;

        objetoArrastando = Instantiate(prefabDoItem); // Cria uma cópia do prefab na cena
        objetoArrastando.transform.localScale = escalaPadrao;                   // Aplica a escala
        objetoArrastando.transform.rotation = Quaternion.Euler(rotacaoInicial); // Aplica a rotação

        // Desativa física e collider enquanto arrasta para não interferir nos carros
        Rigidbody rb = objetoArrastando.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;               // Desativa a física enquanto arrasta
            rb.useGravity = false;               // Desativa gravidade enquanto arrasta
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Desativa o collider enquanto arrasta para não bugar com os carros
        Collider col = objetoArrastando.GetComponent<Collider>();
        if (col != null)
            col.enabled = false;                 // Collider desligado — carros passam por cima sem bug
    }

    // Chamado A CADA FRAME enquanto o dedo/mouse estiver arrastando
    public void OnDrag(PointerEventData eventData)
    {
        if (!arrastando || objetoArrastando == null || cameraPrincipal == null) return;

        Ray ray = cameraPrincipal.ScreenPointToRay(eventData.position); // Funciona para mouse e touch
        RaycastHit hit;
        posicionamentoAtualValido = false;

        if (Physics.Raycast(ray, out hit))       // Se o raio acertou algum collider na cena
        {
            if (ColliderEstaBloqueadoParaPosicionamento(hit.collider))
                return;

            Vector3 pos = hit.point + offsetPosicao; // Posição do hit + offset

            Collider col = objetoArrastando.GetComponent<Collider>();
            float alturaCollider = col != null ? col.bounds.extents.y : 0.5f;
            pos.y = hit.point.y + alturaCollider + offsetPosicao.y; // Mantém item alinhado ao chão

            objetoArrastando.transform.position = pos; // Move o objeto
            tevePosicionamentoValido = true;
            posicionamentoAtualValido = true;
        }

    }

    // Chamado UMA VEZ quando o dedo/mouse solta
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!arrastando || objetoArrastando == null) return; // Segurança

        arrastando = false;                      // Marca que parou de arrastar

        if (!tevePosicionamentoValido || !posicionamentoAtualValido)
        {
            Destroy(objetoArrastando);           // Não consome item se não foi possível posicionar
            objetoArrastando = null;
            return;
        }

        bool manterEstatico = manterObjetoEstaticoAoSoltar || registrarNoPlanejamento;

        Rigidbody rb = objetoArrastando.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = manterEstatico ? true : false;
            rb.useGravity = manterEstatico ? false : true;
        }

        // Reativa o collider DEPOIS de um pequeno delay para não bugar com carros em movimento
        Collider col = objetoArrastando.GetComponent<Collider>();
        if (col != null)
        {
            if (manterEstatico)
                col.enabled = true;
            else
                StartCoroutine(ReativarCollider(col)); // Reativa com delay via Coroutine
        }

        quantidadeAtual = Mathf.Max(0, quantidadeAtual - 1);
        AtualizarUI();                           // Atualiza o visual da quantidade

        if (registrarNoPlanejamento)
            PlanejamentoRuntimeData.RegistrarItem(prefabDoItem, objetoArrastando.transform);

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
        if (prefabDoItem != null)
            Debug.Log($"{prefabDoItem.name}: {quantidadeAtual}/{quantidadeMaxima} restantes");
    }

    private void AtualizarRotacaoDuranteArraste()
    {
        if (!permitirRotacaoDuranteArraste || objetoArrastando == null) return;

        float direcaoRotacao = 0f;

        if (Input.GetKey(teclaRotacionarEsquerda)) direcaoRotacao -= 1f;
        if (Input.GetKey(teclaRotacionarDireita)) direcaoRotacao += 1f;

        if (Mathf.Abs(direcaoRotacao) < 0.001f) return;

        float delta = direcaoRotacao * velocidadeRotacaoArraste * Time.deltaTime;
        objetoArrastando.transform.Rotate(0f, delta, 0f, Space.World);
    }

    private bool ColliderEstaBloqueadoParaPosicionamento(Collider colliderAlvo)
    {
        if (colliderAlvo == null) return false;
        return colliderAlvo.GetComponentInParent<BloqueioPosicionamentoArea>() != null;
    }
}
