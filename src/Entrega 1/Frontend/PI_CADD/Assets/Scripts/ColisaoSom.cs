using UnityEngine;

public class ColisaoSom : MonoBehaviour
{
    public AudioClip somBatida;

    private void OnCollisionEnter(Collision collision)
    {
        // Só toca se colidir com objetos específicos
        if (collision.gameObject.CompareTag("ObjetoJogavel"))
        {
            Debug.Log("BATIDA VALIDA!");

            if (somBatida != null && AudiosScript.instancia != null)
            {
                AudiosScript.instancia.TocarEfeito(somBatida);
            }
        }
    }
}