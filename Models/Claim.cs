using System;
using System.Collections.Generic; 
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMCS.Models
{
    public enum ClaimStatus { Pending, PreApproved, Approved, Rejected, Settled }

    public class Claim
    {
        [Key]
        public int Id { get; set; }

        // Who submitted the claim
        [Required]
        public string LecturerId { get; set; }

        // Description or module name
        [Required]
        public string Description { get; set; } = string.Empty;

        // Hours worked
        [Required]
        [Range(0, 1000)]
        public decimal HoursWorked { get; set; }

        // Rate per hour
        [Required]
        [Range(0, 10000)]
        public decimal HourlyRate { get; set; }

        // Final calculated amount stored in DB
        public decimal TotalAmount { get; set; }

        // Optional notes
        public string Notes { get; set; }

        // Tracks all automated system actions for auditing
        [NotMapped]
        public List<string> AutomationMessages { get; set; } = new List<string>();


        // File uploaded by lecturer (optional)
        public string UploadedFilePath { get; set; }

        // Workflow status
        public ClaimStatus Status { get; set; } = ClaimStatus.Pending;

        // Timestamps
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }

        // Approver
        public string ProcessedBy { get; set; }

        // HR Settlement
        public bool IsPaid { get; set; } = false;
        public string InvoiceNumber { get; set; }

        // Calculated property shown in views (not stored)
        [NotMapped]
        public decimal CalculatedAmount => HoursWorked * HourlyRate;
    }
}

