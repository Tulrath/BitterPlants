using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HelpPanel : MonoBehaviour {

    RectTransform help;
    private ScrollRect scrollViewRect;

    void Start()
    {
        help = GameObject.FindGameObjectWithTag("HelpPanel").GetComponentInChildren<RectTransform>();
        scrollViewRect = GameObject.FindGameObjectWithTag("HelpPanel").GetComponentInChildren<ScrollRect>() as ScrollRect;
    }

    public void ClickGotIt()
    {
        help.localScale = Vector3.zero;
        GameObject.FindGameObjectWithTag("AudioManager").GetComponentInChildren<ActorAudioManager>().playButtonClick();
    }

    public void ScrollToTop()
    {
        scrollViewRect.verticalNormalizedPosition = 1f;
    }
}
