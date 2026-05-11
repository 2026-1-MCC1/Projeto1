using System.Collections;
using UnityEngine;

public class MenuMusic : MonoBehaviour
{
    public AudioClip musicaMenu;

    private IEnumerator Start()
    {
        // Espera 1 frame para garantir que o AudioManager já inicializou.
        yield return null;

        if (AudiosScript.instancia == null)
        {
            Debug.LogWarning("MenuMusic: AudiosScript.instancia não encontrada na cena.");
            yield break;
        }

        // Pede para o gerenciador global tocar a música do menu.
        AudiosScript.instancia.TocarMusica(musicaMenu);
    }
}
