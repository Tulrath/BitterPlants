using UnityEngine;
using System.Collections.Generic;

public class ActorHardwareManager : MonoBehaviour
{
   
    private void Start()
    {
       

        DontDestroyOnLoad(gameObject as Object);
    }

    private void Update()
    {
        //if running on Android, check for Exit and Exit
        if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
                return;
            }
        }
        
    }
}