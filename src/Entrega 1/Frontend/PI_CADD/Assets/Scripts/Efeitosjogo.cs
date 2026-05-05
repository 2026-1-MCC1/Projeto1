using UnityEngine;

public class EfeitosJogo : MonoBehaviour
{
    public AudioClip sirene;
    public AudioClip batida;

    private void Start()
    {
        // Toca a sirene em loop
        if (sirene != null)
        {
            AudiosScript.instancia.efeitosSource.clip = sirene;
            AudiosScript.instancia.efeitosSource.loop = true;
            AudiosScript.instancia.efeitosSource.Play();
        }
    }

    public void TocarBatida()
    {
        AudiosScript.instancia.TocarEfeito(batida);
    }
}