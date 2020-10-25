using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fullfillment : MonoBehaviour
{

    private ScreenStore screen;
    private const string StoreResultPanelName = "storeResultPanel";
    private Text StoreResultText;
    private const string StoreResultTextTag = "StoreResultText";
    
    public void Start()
    {
        screen = GameObject.Find("ScreenStore").GetComponentInChildren<ScreenStore>();
        StoreResultText = GameObject.FindGameObjectWithTag(StoreResultTextTag).GetComponentInChildren<Text>();    
    }

    public void GrantCredits(int credits)
    {
        Debug.Log(string.Format("ProcessPurchase: PASS. Gems: '{0}'", credits));
        
        int currentGems = Identity.GetGems();
        screen.SetGemCountDisplay(currentGems + credits);
        Identity.AddGems(credits);

        StoreResultText.text = "You successfully purchased " + credits.ToString() + " Gems!";
        ActorGameManager.ShowGroup(StoreResultPanelName, true);
    }

    public void PurchaseFailed()
    {
        StoreResultText.text = "Your purchase was not successful.";
        ActorGameManager.ShowGroup(StoreResultPanelName, true);
    }
}
