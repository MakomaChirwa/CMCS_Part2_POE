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
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0, 1000)]
        public decimal HoursWorked { get; set; }


        [Required]
        [Range(0, 10000)]
        public decimal HourlyRate { get; set; }
        public decimal CalculateTotal()
        {
            return HoursWorked * HourlyRate;
        }

        [NotMapped]
        public decimal Amount => HoursWorked * HourlyRate;
        public double TotalPayment { get; set; }
        public string Notes { get; set; }

        public string UploadedFilePath { get; set; }

        public ClaimStatus Status { get; set; } = ClaimStatus.Pending;

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ProcessedAt { get; set; }

        public string ProcessedBy { get; set; }

        public bool IsPaid { get; set; } = false;
        public string InvoiceNumber { get; set; }

    }
}
