using Amazon.DynamoDBv2.DataModel;

[DynamoDBTable("BitterPlants_Nicknames")]
public class NicknameDDB
{
    [DynamoDBHashKey]
    public string Nickname { get; set; }
    [DynamoDBProperty]
    public string IdentityID { get; set; }
}