using System.Collections.Generic;
using UnityEngine;

public class AnimacaoVisualRodasCarro : MonoBehaviour
{
    [Header("Rodas")]
    [SerializeField] private bool autoEncontrarRodas = true;
    [SerializeField] private Transform[] rodas;

    [Header("Ajuste Visual")]
    [SerializeField] private float raioRoda = 0.33f;
    [SerializeField] private float multiplicadorRotacao = 1f;
    [SerializeField] private bool inverterRotacao = false;

    private Rigidbody rb;
    private Vector3 ultimaPosicao;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        ultimaPosicao = transform.position;
    }

    private void Start()
    {
        if (autoEncontrarRodas && (rodas == null || rodas.Length == 0))
            AutoDetectarRodas();
    }

    private void LateUpdate()
    {
        if (rodas == null || rodas.Length == 0)
        {
            ultimaPosicao = transform.position;
            return;
        }

        float dt = Mathf.Max(0.0001f, Time.deltaTime);
        Vector3 velocidadeMundo = ObterVelocidade(dt);
        float velocidadeFrente = Vector3.Dot(velocidadeMundo, transform.forward);

        float circunferencia = Mathf.Max(0.001f, 2f * Mathf.PI * Mathf.Max(0.05f, raioRoda));
        float angulo = (velocidadeFrente * dt / circunferencia) * 360f * multiplicadorRotacao;
        if (inverterRotacao) angulo *= -1f;

        for (int i = 0; i < rodas.Length; i++)
        {
            if (rodas[i] == null) continue;
            rodas[i].Rotate(Vector3.right, angulo, Space.Self);
        }

        ultimaPosicao = transform.position;
    }

    public void AutoDetectarRodas()
    {
        Transform[] todos = GetComponentsInChildren<Transform>(true);
        List<Transform> encontradas = new List<Transform>(8);

        for (int i = 0; i < todos.Length; i++)
        {
            Transform t = todos[i];
            if (t == null || t == transform) continue;

            string nome = t.name.ToLowerInvariant();
            if (!EhNomeDeRoda(nome)) continue;
            encontradas.Add(t);
        }

        rodas = encontradas.ToArray();
    }

    private Vector3 ObterVelocidade(float dt)
    {
        if (rb != null && !rb.isKinematic)
            return rb.linearVelocity;

        Vector3 deslocamento = transform.position - ultimaPosicao;
        return deslocamento / dt;
    }

    private static bool EhNomeDeRoda(string nome)
    {
        if (string.IsNullOrEmpty(nome)) return false;
        if (nome.Contains("wheel")) return true;
        if (nome.Contains("roda")) return true;
        if (nome.Contains("pneu")) return true;
        return false;
    }
}
