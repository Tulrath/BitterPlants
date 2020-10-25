using Amazon.DynamoDBv2.DataModel;

[DynamoDBTable("BitterPlants_Users")]
public class UserDDB
{
   
    [DynamoDBHashKey]
    public string IdentityID { get; set; }

    
}


