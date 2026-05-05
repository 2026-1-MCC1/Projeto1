using UnityEngine;

public class ColisaoSom : MonoBehaviour
{
    public AudioClip somBatida;

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("COLIDIU COM: " + collision.gameObject.name);

        if (somBatida != null && AudiosScript.instancia != null)
        {
            AudiosScript.instancia.TocarEfeito(somBatida);
        }
        else
        {
            Debug.LogWarning("Som de batida ou AudioManager não encontrado!");
        }
    }
}