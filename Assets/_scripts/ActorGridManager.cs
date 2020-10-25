using UnityEngine;


public class ActorGridManager : MonoBehaviour
{
    public static string CanvasName = "BackgroundPanel";
    public static int columnCount;
    public static string HexagonTag = "Hexagon";
    public RectTransform canvasTransform;
    public GameObject hexagonPrefab;
    private static int rowCount;
    private static float shelfHeight = 160f;
    private float spriteHexagonSide = 48f;
    
    public static float CalculateA(float side)
    {
        return 2f * CalculateR(side); // short diameter
    }

    public static float CalculateB(float side)
    {
        return (CalculateH(side) * 2f) + side; // long diameter
    }

    public static float CalculateH(float side)
    {
        return Mathf.Sin(Mathf.Deg2Rad * 30f) * side;
    }

    public static float CalculateR(float side)
    {
        return Mathf.Cos(Mathf.Deg2Rad * 30f) * side;
    }
    
    public static void DeleteGrid()
    {
        GameObject[] hexagons = GameObject.FindGameObjectsWithTag(HexagonTag);
        foreach (GameObject h in hexagons)
        {
            DestroyImmediate(h);
        }
    }

    public static void EnterEditMode()
    {
        State.gameActivity = GameActivity.edit;
        GameObject[] hexagons = GameObject.FindGameObjectsWithTag(HexagonTag);
        foreach (GameObject h in hexagons)
        {
            ActorHexagon ah = h.GetComponentInChildren<ActorHexagon>();
            if (ah != null)
            {
                ColorID currentGoalColor = ah.goalColor;
                ah.SetGoalColor(ColorID.white);
                ah.SetColor(0, currentGoalColor);
            }
        }
    }

    public static void EnterTestMode()
    {
        State.gameActivity = GameActivity.test;

        GameObject[] hexagons = GameObject.FindGameObjectsWithTag(HexagonTag);
        foreach (GameObject h in hexagons)
        {
            ActorHexagon ah = h.GetComponentInChildren<ActorHexagon>();
            if (ah != null)
            {
                ColorID currentColor = ah.hexagonColor;
                ah.SetColor(0, ColorID.white);
                if(ah.goalColor == ColorID.white)
                    ah.SetGoalColor(currentColor);
            }
        }
    }

    

    public static ActorHexagon GetHexagonActor(int ID)
    {
        GameObject hexagon = GameObject.Find(HexagonTag + ID.ToString());
        if (hexagon != null)
        {
            return hexagon.GetComponentInChildren<ActorHexagon>() as ActorHexagon;
        }
        else
        {
            return null;
        }
    }

    public static GameObject GetRandomHexagon()
    {
        GameObject[] hexagons = GameObject.FindGameObjectsWithTag("Hexagon");
        return hexagons[UnityEngine.Random.Range(0, hexagons.Length)];
    }
    
    public static void ResetGrid()
    {
        Debug.Log("Resetting grid...");
        GameObject[] hexagons = GameObject.FindGameObjectsWithTag(HexagonTag);
        foreach (GameObject h in hexagons)
        {
            ActorHexagon ah = h.GetComponentInChildren<ActorHexagon>();
            if (ah != null)
            {
                ah.SetColor(0, ColorID.white);
                ah.SetGoalColor(ColorID.white);
                ah.SetHoverColor(ColorID.white);
            }
        }
    }

   

    public static void SetHexagonColor(int hexagonID, ColorID colorID)
    {
        ActorHexagon target = GetHexagonActor(hexagonID);
        if (target != null)
        {
            target.SetColor(0, colorID);
        }
    }

    public static void SetHexagonGoalColor(int hexagonID, ColorID colorID)
    {
        ActorHexagon target = GetHexagonActor(hexagonID);
        if (target != null)
        {
            target.SetGoalColor(colorID);
        }
    }

    public static void SetHexagonHoverColor(int hexagonID, ColorID colorID)
    {
        ActorHexagon target = GetHexagonActor(hexagonID);
        if (target != null)
        {
            target.SetHoverColor(colorID);
        }
    }

    public void BuildGrid(float requestedColumns, float requestedRows)
    {
        if (canvasTransform == null)
            canvasTransform = GameObject.FindGameObjectWithTag(CanvasName).GetComponentInChildren<RectTransform>();

        DeleteGrid();

        float wideColumnWidth = CalculateB(spriteHexagonSide);
        float narrowColumnWidth = spriteHexagonSide;
        float wideColumnCount = Mathf.Floor(requestedColumns * 0.5f) + 1f;
        float narrowColumnCount = Mathf.Floor(requestedColumns * 0.5f);
        float gridWidth = requestedColumns > 0 ? ((wideColumnWidth * wideColumnCount) + (narrowColumnWidth * narrowColumnCount)) : (State.referenceScreenSize.x - CalculateB(spriteHexagonSide));
        float gridHeight = requestedRows > 0 ? requestedRows * CalculateA(spriteHexagonSide) : (State.referenceScreenSize.y - CalculateA(spriteHexagonSide) - shelfHeight);
        float gridPaddingWidth = (State.referenceScreenSize.x - gridWidth) * 0.5f;
        
        float gridPaddingHeight = 0; // shifting to bottom of the screen
        float columnW = spriteHexagonSide * 1.5f;
        columnCount = Mathf.FloorToInt(requestedColumns > 0 ? requestedColumns : gridWidth / columnW);
        float rowH = CalculateA(spriteHexagonSide);
        rowCount = Mathf.FloorToInt(requestedRows > 0 ? requestedRows : gridHeight / rowH);
        float rowShift = CalculateR(spriteHexagonSide);
        float columnOffset = CalculateH(spriteHexagonSide) + spriteHexagonSide * 0.5f;
        float rowOffset = CalculateR(spriteHexagonSide);

        Debug.Log("Requested Grid Size: " + requestedColumns + "," + requestedRows);
        Debug.Log("Reference Screen Size: " + State.referenceScreenSize);
        Debug.Log("Screen Size: " + Screen.width + "," + Screen.height);
        Debug.Log("Grid Size: " + gridWidth + "," + gridHeight);
        Debug.Log("Grid Padding: " + gridPaddingWidth + "," + gridPaddingHeight);
        Debug.Log("Hexagon Col/Row Size: " + columnW + "," + rowH);
        Debug.Log("Grid Col/Row Count: " + columnCount + "," + rowCount);

        // build hexagons
        int hexagonID = 0;
        for (int row = 0; row < rowCount; row++)
        {
            for (int column = 0; column < columnCount; column++)
            {
                GameObject hexagon = GameObject.Instantiate(hexagonPrefab);
                hexagon.name = HexagonTag + hexagonID;
                hexagon.GetComponentInChildren<ActorHexagon>().hexagonID = hexagonID;
                RectTransform hexagonTransform = hexagon.GetComponentInChildren<RectTransform>();
                hexagonTransform.SetParent(canvasTransform, true);
                hexagonTransform.anchoredPosition = new Vector2(gridPaddingWidth + columnOffset + (columnW * column), gridPaddingHeight + (rowOffset + (column % 2 == 0 ? rowH * row : rowH * row + rowShift)));

                // we have to UNDO the scaling that was done by the CanvasScaler
                hexagonTransform.localScale = Vector3.one;

                hexagonID++;
            }
        }

        // find neighbors AFTER all builds are complete
        hexagonID = 0;
        int offset = 0;
        for (int row = 0; row < rowCount; row++)
        {
            for (int column = 0; column < columnCount; column++)
            {
                offset = (column % 2 == 0 ? 0 : columnCount);
                FindNeighbors(GameObject.Find(HexagonTag + hexagonID).GetComponent<ActorHexagon>() as ActorHexagon, hexagonID, offset);
                hexagonID++;
            }
        }

        // grid is ready
        Debug.Log("Grid is ready.");
    }
    private static void FindNeighbors(ActorHexagon hexagon, int ID, int offset)
    {
        int neighbor = 0;
        // north
        neighbor = ID + columnCount;
        if (neighbor < (columnCount * rowCount))
            hexagon.AddPetal(0, GameObject.Find(HexagonTag + neighbor.ToString()).GetComponent<ActorHexagon>() as ActorHexagon);

        // south
        neighbor = ID - columnCount;
        if (neighbor >= 0)
            hexagon.AddPetal(3, GameObject.Find(HexagonTag + neighbor.ToString()).GetComponent<ActorHexagon>() as ActorHexagon);

        // northwest
        neighbor = ID - 1;
        if ((neighbor >= 0) && (neighbor % columnCount != (columnCount - 1)) && ((neighbor + offset) < rowCount * columnCount))
            hexagon.AddPetal(5, GameObject.Find(HexagonTag + (neighbor + offset).ToString()).GetComponent<ActorHexagon>() as ActorHexagon);

        // northeast
        neighbor = ID + 1;
        if ((neighbor < columnCount * rowCount) && (neighbor % columnCount != 0) && ((neighbor + offset) < rowCount * columnCount))
            hexagon.AddPetal(1, GameObject.Find(HexagonTag + (neighbor + offset).ToString()).GetComponent<ActorHexagon>() as ActorHexagon);

        // southwest
        neighbor = ID - (columnCount - offset) - 1;
        if ((neighbor >= 0) && (neighbor % columnCount != (columnCount - 1)))
            hexagon.AddPetal(4, GameObject.Find(HexagonTag + neighbor.ToString()).GetComponent<ActorHexagon>() as ActorHexagon);

        // southeast
        neighbor = ID - (columnCount - offset) + 1;
        if ((neighbor > 0) && (neighbor % columnCount != 0))
            hexagon.AddPetal(2, GameObject.Find(HexagonTag + neighbor.ToString()).GetComponent<ActorHexagon>() as ActorHexagon);
    }
}


