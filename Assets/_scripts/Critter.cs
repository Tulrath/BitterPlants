using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum MoveType
{
    stationary,
    sway,
    random,
    chase,
    bounce
}

public class Critter : MonoBehaviour {

    public string parentTag = "BackgroundPanel";
    public Vector2 moveSpeedPerSecond;
    public Vector2 swayDistance;
    public Vector2 swayMax;
    public Vector2 swayMin;
    public float spinDegreesPerSecond;
    public float flipDegreesPerSecond;
    public float fadeSeconds;
    public Sprite[] variations;
    public bool randomStartScale;
    public bool randomStartRotation;
    public bool randomMoveSpeed;
    public MoveType moveType;
    public float moveSeconds = 1f;
    public float pauseSeconds = 1f;
    public float lifeSeconds = 5f;

    public Vector2 desiredMove;
    private bool moving;
    private float nextStartMoveTime;
    private float nextStopMoveTime;
    private Vector2 swayVelocity;
    private float smoothTime = 3F;
    private float currentOpacity = 1f;
    private RectTransform targetTransform;
    public RectTransform parentTransform;
    public RectTransform myTransform;
    private Vector2 startPosition;
    private bool swayingRight;
    private float swayInterval = 2.9f;
    private float nextSwayChange;
    private Image myImage;
    private float fadeOffset;
    private ActorHexagon currentHexagon;
    private float deathTime = 0;
    
	
	void Start ()
    {
        parentTransform = GameObject.FindGameObjectWithTag(parentTag).GetComponentInChildren<RectTransform>();
        myTransform = gameObject.GetComponentInChildren<RectTransform>();
        myImage = gameObject.GetComponentInChildren<Image>();
        myTransform.SetParent(parentTransform, false);
        startPosition = myTransform.anchoredPosition;
        swayMax = startPosition + swayDistance;
        swayMin = startPosition - swayDistance;

        // randomize the initial scale
        if(randomStartScale)
        {
            float size = Random.Range(48f, 96f);
            myTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
            myTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
        }

        // randomize the initial rotation
        if (randomStartRotation)
        {
            float spinRotation = Random.Range(0, 360f);
            myTransform.rotation = Quaternion.Euler(0, 0, spinRotation);
        }

        // randomize the sprite
        if(variations.Length > 1)
            myImage.sprite = variations[Random.Range(0, variations.Length)];

        // randomize where we are in our sway
        swayingRight = Random.Range(0, 2) == 0 ;
        nextSwayChange = Time.time + Random.Range(0, swayInterval);

        // randomize our speed
        if(randomMoveSpeed)
            moveSpeedPerSecond += new Vector2(Random.Range(-moveSpeedPerSecond.x * 0.25f, moveSpeedPerSecond.x * 0.25f), Random.Range(-moveSpeedPerSecond.y * 0.25f, moveSpeedPerSecond.y * 0.25f));
        
        // randomize our fade
        fadeOffset = Random.Range(0, fadeSeconds);

        // set the time of death
        if (lifeSeconds != 0)
            deathTime = Time.realtimeSinceStartup + lifeSeconds;

    }
	
	void Update () {


        if (deathTime != 0 && Time.realtimeSinceStartup > deathTime)
            Destroy(gameObject);

        if(!moving && Time.realtimeSinceStartup > nextStartMoveTime)
        {
            moving = true;
            nextStopMoveTime = Time.realtimeSinceStartup + moveSeconds;
            GameObject go = null;
            switch(moveType)
            {
                case MoveType.random:
                    go = ActorGridManager.GetRandomHexagon();
                    if(go)
                        targetTransform = go.GetComponent<RectTransform>();
                    break;
                case MoveType.stationary:
                    targetTransform = null;
                    moveSpeedPerSecond = Vector2.zero;
                    break;   
            }

        }

        if(moving && Time.realtimeSinceStartup > nextStopMoveTime)
        {
            moving = false;
            nextStartMoveTime = Time.realtimeSinceStartup + pauseSeconds;
        }
        
        if(moving)
        {
            if(targetTransform != null)
            {
                Vector2 tgt = targetTransform.anchoredPosition;
                Vector2 pos = myTransform.anchoredPosition;
                desiredMove = new Vector2(tgt.x < pos.x ? -moveSpeedPerSecond.x : moveSpeedPerSecond.x, tgt.y < pos.y ? -moveSpeedPerSecond.y : moveSpeedPerSecond.y) * Time.deltaTime;
            }
            else
            {
                desiredMove = moveSpeedPerSecond * Time.deltaTime;
            }
        }
    
        if(desiredMove != Vector2.zero)
        {
            Vector2 newPosition = myTransform.anchoredPosition + desiredMove;
            newPosition.x = newPosition.x < -32f ? (State.referenceScreenSize.x + 32f) : newPosition.x;
            newPosition.x = newPosition.x > (State.referenceScreenSize.x + 64f) ? -32f : newPosition.x;
            newPosition.y = newPosition.y < 32f ? (State.referenceScreenSize.y + 32f) : newPosition.y;
            newPosition.y = newPosition.y > (State.referenceScreenSize.y + 64f) ? 32f : newPosition.y;

            myTransform.anchoredPosition = newPosition;
            
        }

        if(moveType == MoveType.sway && swayDistance.x !=0)
        {
            swayMax = new Vector2(swayMax.x, myTransform.anchoredPosition.y);
            swayMin = new Vector2(swayMin.x, myTransform.anchoredPosition.y);
            //myTransform.anchoredPosition = Vector2.SmoothDamp(myTransform.anchoredPosition, swayingRight? swayMax :swayMin, ref swayVelocity, smoothTime);   
            myTransform.anchoredPosition = Vector2.SmoothDamp(myTransform.anchoredPosition, (swayingRight ? swayMax : swayMin), ref swayVelocity, smoothTime, 10f, Time.deltaTime);

            if (Time.time > nextSwayChange)
            {
                nextSwayChange = Time.time + swayInterval;
                swayingRight = !swayingRight;
            }
        }

        if(spinDegreesPerSecond != 0)
        {
            myTransform.Rotate(Vector3.forward, Time.deltaTime * spinDegreesPerSecond, Space.World);
        }

        if (flipDegreesPerSecond != 0)
        {
            myTransform.Rotate(Vector3.right, Time.deltaTime * flipDegreesPerSecond, Space.World);
        }

        if(fadeSeconds != 0)
        {
            currentOpacity = Mathf.PingPong(((Time.time + fadeOffset) / fadeSeconds), 1f);
            myImage.color = new Color(1f, 1f, 1f, currentOpacity);
        }
    }
}
