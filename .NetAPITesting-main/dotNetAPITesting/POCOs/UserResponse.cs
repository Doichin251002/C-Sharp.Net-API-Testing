namespace dotNetAPITesting.POCOs;

public class UserResponse
{
    public UserResponse(int id, string name, string email, string gender, string status)
    {
        this.Id = id;
        this.name = name;
        this.gender = gender;
        this.email = email;
        this.status = status;
    }

    public int Id { get; set; }
    public string name { get; set; }
    public string email { get; set; }
    public string gender { get; set; }
    public string status { get; set; }
}