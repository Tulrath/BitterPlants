using System.Collections.Generic;

public class Board
{ 
    public Dictionary<string, int> gameGoals = new Dictionary<string, int>();
    public Dictionary<int, int> presetBoard = new Dictionary<int, int>();
    public GameMode gameMode = GameMode.artist;
    public Queue<GameStep> seedsPlantedSolution = new Queue<GameStep>();
}
