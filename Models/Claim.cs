using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMCS.Models
{
    public enum ClaimStatus { Pending, PreApproved, Approved, Rejected, Settled }

    public class Claim
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string LecturerId { get; set; }

        [Required]
        [Range(0, 1000)]
        public decimal HoursWorked { get; set; }

        [Required]
        [Range(0, 10000)]
        public decimal HourlyRate { get; set; }

        [NotMapped]
        public decimal Amount => HoursWorked * HourlyRate;

        public string Notes { get; set; }

        public string UploadedFilePath { get; set; }

        public ClaimStatus Status { get; set; } = ClaimStatus.Pending;

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ProcessedAt { get; set; }

        public string ProcessedBy { get; set; }
    }
}
