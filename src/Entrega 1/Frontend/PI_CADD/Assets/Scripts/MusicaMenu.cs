using UnityEngine;

public class MenuMusic : MonoBehaviour
{
    public AudioClip musicaMenu;

    IEnumerator Start()
    {
        // 🔥 espera 1 frame para garantir que o AudioManager aplicou o volume
        yield return null;

        if (AudiosScript.instancia != null)
        {
            AudiosScript.instancia.TocarMusica(musicaMenu);
        }
    }
}