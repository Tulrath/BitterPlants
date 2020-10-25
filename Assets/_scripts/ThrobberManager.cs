using UnityEngine;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class ThrobberManager : MonoBehaviour {

    public RectTransform throbber;
    private RectTransform rect;
    private float spinDegreesPerSecond = 360f;

    void Start()
    {
        if (!throbber)
            this.enabled = false;
        rect = GetComponent<RectTransform>();
    }
    
	void Update ()
    {
        rect.localScale = State.TasksWaiting() ? Vector3.one : Vector3.zero;
        throbber.Rotate(Vector3.forward, Time.deltaTime * spinDegreesPerSecond, Space.World);
    }
}
