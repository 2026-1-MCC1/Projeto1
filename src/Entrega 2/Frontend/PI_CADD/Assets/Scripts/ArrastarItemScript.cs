using UnityEngine;
using UnityEngine.EventSystems;

public class ArrastarItemScript : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject prefabDoItem;

    [Header("Escala")]
    public Vector3 escalaPadrao = new Vector3(0.5f, 0.5f, 0.5f);

    [Header("Rotação Inicial")]
    public Vector3 rotacaoInicial = Vector3.zero;

    [Header("Rotação Durante Arraste")]
    public bool permitirRotacaoDuranteArraste = true;
    public float velocidadeRotacaoArraste = 140f;
    public KeyCode teclaRotacionarEsquerda = KeyCode.Q;
    public KeyCode teclaRotacionarDireita = KeyCode.E;

    [Header("Offset de Posição")]
    public Vector3 offsetPosicao = Vector3.zero;

    [Header("Quantidade")]
    public int quantidadeMaxima = 3;
    private int quantidadeAtual;

    [Header("Planejamento")]
    public bool registrarNoPlanejamento = false;
    public bool manterObjetoEstaticoAoSoltar = false;

    private GameObject objetoArrastando;
    private bool arrastando = false;
    private bool tevePosicionamentoValido = false;
    private bool posicionamentoAtualValido = false;
    private Camera cameraPrincipal;

    void Start()
    {
        quantidadeAtual = quantidadeMaxima;
        cameraPrincipal = Camera.main;
    }

    void Update()
    {
        if (!arrastando || objetoArrastando == null) return;
        AtualizarRotacaoDuranteArraste();
    }

    // Instancia o item e desativa fisica/colisor enquanto esta em arraste.
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (quantidadeAtual <= 0 || prefabDoItem == null) return;

        if (cameraPrincipal == null)
            cameraPrincipal = Camera.main;

        if (cameraPrincipal == null)
        {
            Debug.LogError("ArrastarItemScript: nenhuma câmera principal encontrada para raycast.");
            return;
        }

        arrastando = true;
        tevePosicionamentoValido = false;
        posicionamentoAtualValido = false;

        objetoArrastando = Instantiate(prefabDoItem);
        objetoArrastando.transform.localScale = escalaPadrao;
        objetoArrastando.transform.rotation = Quaternion.Euler(rotacaoInicial);

        Rigidbody rb = objetoArrastando.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Collider[] cols = objetoArrastando.GetComponentsInChildren<Collider>();
        foreach (Collider c in cols)
            c.enabled = false;

        UnityEngine.AI.NavMeshObstacle[] navs = objetoArrastando.GetComponentsInChildren<UnityEngine.AI.NavMeshObstacle>();
        foreach (var nav in navs)
            nav.enabled = false;
    }

    // Move o item para o ponto valido de raycast.
    public void OnDrag(PointerEventData eventData)
    {
        if (!arrastando || objetoArrastando == null || cameraPrincipal == null) return;

        Ray ray = cameraPrincipal.ScreenPointToRay(eventData.position);
        RaycastHit hit;
        posicionamentoAtualValido = false;

        if (Physics.Raycast(ray, out hit))
        {
            if (ColliderEstaBloqueadoParaPosicionamento(hit.collider))
                return;

            Vector3 pos = hit.point + offsetPosicao;

            Collider col = objetoArrastando.GetComponentInChildren<Collider>();
            float alturaCollider = col != null ? col.bounds.extents.y : 0.5f;
            pos.y = hit.point.y + alturaCollider + offsetPosicao.y;

            objetoArrastando.transform.position = pos;
            tevePosicionamentoValido = true;
            posicionamentoAtualValido = true;
        }
    }

    // Finaliza o arraste e aplica o comportamento escolhido para o item.
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!arrastando || objetoArrastando == null) return;

        arrastando = false;

        if (!tevePosicionamentoValido || !posicionamentoAtualValido)
        {
            Destroy(objetoArrastando);
            objetoArrastando = null;
            return;
        }

        bool manterEstatico = manterObjetoEstaticoAoSoltar;

        Rigidbody rb = objetoArrastando.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = manterEstatico;
            rb.useGravity = !manterEstatico;
        }

        Collider[] cols = objetoArrastando.GetComponentsInChildren<Collider>();
        foreach (var c in cols) c.enabled = true;

        UnityEngine.AI.NavMeshObstacle[] navs = objetoArrastando.GetComponentsInChildren<UnityEngine.AI.NavMeshObstacle>();
        foreach (var nav in navs)
            nav.enabled = true;

        quantidadeAtual = Mathf.Max(0, quantidadeAtual - 1);
        AtualizarUI();

        if (registrarNoPlanejamento)
            PlanejamentoRuntimeData.RegistrarItem(prefabDoItem, objetoArrastando.transform);

        objetoArrastando = null;
    }

    private void AtualizarUI()
    {
        UnityEngine.UI.Image imagem = GetComponent<UnityEngine.UI.Image>();
        if (imagem != null)
            imagem.color = quantidadeAtual <= 0
                ? new Color(1f, 1f, 1f, 0.3f)
                : new Color(1f, 1f, 1f, 1f);
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
