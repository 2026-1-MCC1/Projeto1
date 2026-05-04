using UnityEngine;

public class MusicaMenu : MonoBehaviour
{
 
    public AudioClip musicaMenu;

    private void Start()
    {
        AudiosScript.instancia.TocarMusica(musicaMenu);
    }
}