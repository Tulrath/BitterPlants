using UnityEngine;
using System.Collections;
using Amazon;


public class ActorIdentityManager : MonoBehaviour {

	
	void Start ()
    {
        UnityInitializer.AttachToGameObject(this.gameObject);

        // create an Identify object and Start Syncronizing Databases
        Identity.StartSync();


    }

}
