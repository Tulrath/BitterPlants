using UnityEngine;

public enum PlayerType
{
    bee,
    grasshopper
}



public class Player
{
    private Vector2 _moveDirection;
    private float changeDirectionTime;
    private float plantSeedTime;

    public ColorID color;
    public ActorHexagon hexagon;
    public float nextMoveDelay = 0.25f;
    public bool isBot;
    public PlayerType playerType = PlayerType.bee;
    public float nextMoveTime;
    public bool ignored;
    public Vector2 moveDirection
    {
        get
        {

            if (isBot && Time.realtimeSinceStartup > changeDirectionTime)
            {
                changeDirectionTime = Time.realtimeSinceStartup + Random.Range(0.25f, 1.75f);
                _moveDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            }
            
            return _moveDirection;
        }
        set { _moveDirection = value; }
    }

    public bool isPlantingSeed
    {
        get
        {
            if(isBot && Time.realtimeSinceStartup > plantSeedTime && hexagon != null && (playerType == PlayerType.bee ? hexagon.hexagonColor == ColorID.white : hexagon.hexagonColor != ColorID.white))
            {
                plantSeedTime = Time.realtimeSinceStartup + Random.Range(2f, 4f);
                return true;
            }
            return false;
        }
        set
        {
            plantSeedTime = value == true ? 0 : Time.realtimeSinceStartup + Random.Range(2f, 4f);

        }
    }
    

    public Player(ActorHexagon h, ColorID c)
    {
        hexagon = h;
        color = c;
        nextMoveTime = Time.realtimeSinceStartup;
        isBot = false;
        moveDirection = new Vector2(1f, -1f);
        changeDirectionTime = Time.realtimeSinceStartup + Random.Range(0.5f, 2f);
    }

    
}
