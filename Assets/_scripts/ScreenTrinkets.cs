using UnityEngine;
using System.Collections;

public class ScreenTrinkets : MonoBehaviour
{

    private ActorAudioManager audioManager;

    void Start()
    {
        audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponentInChildren<ActorAudioManager>();
        audioManager.playScreenLoaded();
    }

   

}
