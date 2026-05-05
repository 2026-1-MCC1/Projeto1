using UnityEngine;

public class MenuMusic : MonoBehaviour
{
    public AudioClip musicaMenu;

    void Start()
    {
        if (AudiosScript.instancia != null)
        {
            AudiosScript.instancia.TocarMusica(musicaMenu);
        }
        else
        {
            Debug.LogError("AudioManager não encontrado!");
        }
    }
}