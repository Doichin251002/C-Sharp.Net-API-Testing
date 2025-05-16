namespace dotNetAPITesting.POCOs;

public class UserRequest
{
    public UserRequest(string name, string gender, string email, string status)
    {
        this.name = name;
        this.gender = gender;
        this.email = email;
        this.status = status;
    }

    public string name { get; set; }
    public string gender { get; set; }
    public string email { get; set; }
    public string status { get; set; }
}