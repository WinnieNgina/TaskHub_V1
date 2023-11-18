namespace TaskHub_V1.Models
{
    public class UserProject
    {
        public string UserId { get; set; }
        public User User { get; set; }
        public string ProjectId { get; set; }
        public Project Project { get; set; }
    }
}
