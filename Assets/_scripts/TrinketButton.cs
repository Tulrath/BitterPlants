using UnityEngine;
using UnityEngine.UI;


public class TrinketButton : MonoBehaviour
{

    public Image spriteImage;
    public Text nameText;

    public void SetTrinket(Trinket t)
    {
        spriteImage.sprite = t.sprite;
        nameText.text = t.displayName;
    }

    public void OnClick()
    {

    }
	
}
