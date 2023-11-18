using TaskHub_V1.Models;
using Microsoft.AspNetCore.Identity;

public class User : IdentityUser
{
    // Additional properties
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string SecretKey { get; set; } = Guid.NewGuid().ToString();
    public string ProfilePicturePath { get; set; }

    // Navigation properties
    public ICollection<ProjectTasks> AssignedTasks { get; set; }
    public ICollection<Project> ManagedProjects { get; set; }
    public ICollection<Comment> Comments { get; set; }
    public ICollection<UserProject> UserProjects { get; set; }
}

