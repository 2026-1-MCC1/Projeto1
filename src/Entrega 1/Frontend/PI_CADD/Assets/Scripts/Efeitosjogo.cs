using UnityEngine;

public class EfeitosJogo : MonoBehaviour
{
    public AudioClip sirene;
    public AudioClip batida;

    private void Start()
    {
        if (sirene != null && AudiosScript.instancia != null && AudiosScript.instancia.efeitosSource != null)
        {
            AudiosScript.instancia.efeitosSource.clip = sirene;
            AudiosScript.instancia.efeitosSource.loop = true;
            AudiosScript.instancia.efeitosSource.Play();
        }
    }

    public void TocarBatida()
    {
        if (AudiosScript.instancia == null) return;
        AudiosScript.instancia.TocarEfeito(batida);
    }
}
