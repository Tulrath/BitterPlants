using System;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using FullSerializer;

[DynamoDBTable("BitterPlants_UserBoards")]
public class BoardDDB
{
    [DynamoDBHashKey]
    public string IdentityID { get; set; }
    [DynamoDBRangeKey]
    public string BoardID { get; set; }
    [DynamoDBProperty]
    public int GameModeID { get; set; }
    [DynamoDBProperty]
    public string BoardName { get; set; }
    [DynamoDBProperty]
    public int Votes_A { get; set; }
    [DynamoDBProperty]
    public int Votes_D { get; set; }
    [DynamoDBProperty]
    public int Plays_A { get; set; }
    [DynamoDBProperty]
    public int Plays_D { get; set; }
    [DynamoDBProperty]
    public int Wins_A { get; set; }
    [DynamoDBProperty]
    public int Wins_D { get; set; }
    [DynamoDBProperty]
    public float WinsPlaysRatio { get; set; }
    [DynamoDBProperty]
    public float VotesPlaysRatio { get; set; }

    [DynamoDBProperty(typeof(BoardConverter))]
    public Board UserBoard { get; set; }

}

public class BoardConverter : IPropertyConverter
{
    public DynamoDBEntry ToEntry(object value)
    {
        Board userBoard = value as Board;
        fsData serializedData;
        new fsSerializer().TrySerialize(typeof(Board), userBoard, out serializedData).AssertSuccessWithoutWarnings();
        DynamoDBEntry entry = new Primitive { Value = (fsJsonPrinter.CompressedJson(serializedData)) };
        return entry;
    }
    public object FromEntry(DynamoDBEntry entry)
    {
        Primitive primitive = entry as Primitive;
        if (primitive == null || !(primitive.Value is String) || string.IsNullOrEmpty((string)primitive.Value))
            throw new ArgumentOutOfRangeException();
        fsData deserializedData = fsJsonParser.Parse((primitive.Value as string));
        object value = null;
        new fsSerializer().TryDeserialize(deserializedData, typeof(Board), ref value).AssertSuccessWithoutWarnings();
        return (Board)value;
    }
}