﻿namespace TaskHub_V1.Models
{
    public class ProjectTasks
    {
        public string Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public TaskStatus Status { get; set; }
        public PriorityLevel Priority { get; set; }
        public string FilePath { get; set; }


        // object that represents the user who created the task
        public string UserId { get; set; } // Foreign key
        public User User { get; set; }

        public Project Project { get; set; }
        public string ProjectId { get; set; }
        public ICollection<Comment> Comments { get; set; }

    }
    public enum TaskStatus
    {
        ToDo,
        Open,
        InProgress,
        Completed
    }
    public enum PriorityLevel
    {
        Low,
        Medium,
        High,
        Critical
    }
}
