namespace TaskHub_V1.Models
{
    public class Comment
    {
        public string Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Content { get; set; }
        public string ContentTitle { get; set; }

        // Replace UserId with string to match the IdentityUser Id type
        public string UserId { get; set; }

        // Navigation property for the User
        public User User { get; set; }

        public string ProjectTasksId { get; set; }
        public ProjectTasks ProjectTasks { get; set; }

        public string ProjectId { get; set; }

        // Navigation property for the Project
        public Project Project { get; set; }
    }
}
