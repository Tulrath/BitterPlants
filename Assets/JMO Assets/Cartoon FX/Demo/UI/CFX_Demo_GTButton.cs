using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Cartoon FX - (c) 2015 - Jean Moreno
//
// Script for the Demo scene

public class CFX_Demo_GTButton : MonoBehaviour
{
	public Color NormalColor = new Color32(128,128,128,128), HoverColor = new Color32(128,128,128,128);
	
	public string Callback;
	public GameObject Receiver;
	
	private Rect CollisionRect;
	private bool Over;
	
	//-------------------------------------------------------------
	
	void Awake()
	{
		//CollisionRect = GetComponent<Image>().GetScreenRect(Camera.main);
		CollisionRect = new Rect(0, 0, Screen.width, Screen.height);
	}
	
	void Update ()
	{
		if(CollisionRect.Contains(Input.mousePosition))
		{
			GetComponent<Image>().color = HoverColor;
			
			if(Input.GetMouseButtonDown(0))
			{
				OnClick();
			}
		}
		else
		{
			GetComponent<Image>().color = NormalColor;
		}
	}
	
	//-------------------------------------------------------------
	
	private void OnClick()
	{
		Receiver.SendMessage(Callback);
	}
}
