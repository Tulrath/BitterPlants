using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class ActorTrinketManager : MonoBehaviour {

    public Trinket[] trinkets;
    public GameObject trinketButtonPrefab;
    public RectTransform viewportContent;

    public Trinket unknownTrinket;

    public ScrollRect scrollViewRect;
    public const string trinketListScrollViewTag = "CollectionScrollView";
    public const string trinketButtonTag = "TrinketButton";
    private const float trinketListTopPadding = 128f;
    private const float trinketListLeftPadding = 64f;
    private float trinketButtonSpaceVertical = 180f;
    private float trinketButtonSpaceHorizontal = 128f;

    public GameObject[] rewardExplosions;

    private void BuildTrinketButton(Trinket trinket, int stars, int buttonID)
    {
        Debug.LogFormat("Builidng trinket button {0} with {1} stars using {2} buttonID", trinket.sprite.name, stars, buttonID);

        // set GameObject properties
        GameObject newButton = Instantiate(trinketButtonPrefab);
        newButton.name = buttonID.ToString() + Database.delimiter + trinket.sprite.name + Database.delimiter + stars.ToString();
        TrinketButton button = newButton.GetComponentInChildren<TrinketButton>();
        RectTransform trans = newButton.GetComponentInChildren<RectTransform>();
        
        // set position, scale
        trans.SetParent(viewportContent, false);
        trans.anchoredPosition = new Vector2(trinketListLeftPadding + ((buttonID%4) * trinketButtonSpaceHorizontal), -trinketListTopPadding + ((buttonID / 4) * -trinketButtonSpaceVertical));
        trans.localScale = Vector3.one;

        // set the appearance and name
        button.SetTrinket(trinket);
        
    }

    private void BuildTrinketButtons()
    {
        ClearTrinketButtons();
        
        // build a button for each trinket
        int i = 0;
        if (trinkets.Length > 0)
        {
            foreach (Trinket t in trinkets)
            {
                if(Identity.CollectLocked(t.sprite.name))
                    BuildTrinketButton(unknownTrinket, 0, i);
                else
                    BuildTrinketButton(t, Identity.CollectStars(t.sprite.name), i); 

                i++;
            }
        }

        ScrollToTop();

    }

    private void ClearTrinketButtons()
    {
        GameObject[] buttons = GameObject.FindGameObjectsWithTag(trinketButtonTag);
        foreach (GameObject b in buttons)
        {
            Destroy(b);
        }
    }

    private void FindScrollViewRect()
    {
        if (scrollViewRect == null)
        {
            GameObject go;
            go = GameObject.FindGameObjectWithTag(trinketListScrollViewTag);
            if (go == null)
                go = GameObject.Find(trinketListScrollViewTag);
            if (go)
                scrollViewRect = go.GetComponentInChildren<ScrollRect>() as ScrollRect;
            else
                Debug.LogError("CollectionScrollView could not be found");

            if (scrollViewRect == null)
                Debug.LogError("ScrollRect for CollectionScrollView not found");
        }
    }

    private void ScrollToTop()
    {
        FindScrollViewRect();
        scrollViewRect.verticalNormalizedPosition = 1f;
    }

    void Start()
    {
        if(SceneManager.GetActiveScene().name == "trinkets")
        {
            FindScrollViewRect();
            BuildTrinketButtons();
        }
       
    }
    
}
