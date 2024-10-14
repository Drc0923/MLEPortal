using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_Backend.Models
{
    /// <summary>
    /// Represents an experiment request submitted by a user.
    /// </summary>
    public class ExperimentRequest
    {
        [Key]
        public string ExperimentID { get; set; } // GUID generated by the API

        [ForeignKey("UserID")]
        public string UserID { get; set; } // FK to Users

        [Required]
        public int AlgorithmID { get; set; } // FK to Algorithms

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [Required]
        public ExperimentStatus Status { get; set; }

        public string Parameters { get; set; } // JSON serialized parameters

        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? ErrorMessage { get; set; }

        // Navigation properties
        [ForeignKey("UserID")]
        public User User { get; set; }

        [ForeignKey("AlgorithmID")]
        public Algorithm Algorithm { get; set; }

        public DockerSwarmParameters DockerSwarmParameters { get; set; }
        public ICollection<ExperimentAlgorithmParameterValue> ParameterValues { get; set; }
        public ExperimentResult ExperimentResult { get; set; }
    }

    public enum ExperimentStatus
    {
        WaitingInQueue,
        BeingExecuted,
        BeingProcessed,
        Finished,
        Failed
    }
}