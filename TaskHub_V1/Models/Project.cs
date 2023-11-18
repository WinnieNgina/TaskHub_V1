

namespace TaskHub_V1.Models
{
    public class Project
    {
        public string Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string ProjectName { get; set; }
        public string ProjectVersion { get; set; }
        public string ProjectTitle { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; }
        public string ProjectManagerId { get; set; }
        public User ProjectManager { get; set; }
        public string FilePath { get; set; }
        public ICollection<UserProject> Team { get; set; }
        public ICollection<ProjectTasks> ProjectTasks { get; set; }
        public ProjectStatus Status { get; set; }
        public ICollection<Comment> Comments { get; set; }
    }
    public enum ProjectStatus
    {
        NotStarted,
        InProgress,
        Completed
    }

}
