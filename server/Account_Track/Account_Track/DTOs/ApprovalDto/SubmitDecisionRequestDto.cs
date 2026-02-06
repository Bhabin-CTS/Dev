using System.ComponentModel.DataAnnotations;

namespace Account_Track.DTOs.ApprovalDto
{
    public class SubmitDecisionRequestDto
    {
        //public int ReviewerId { get; set; }

        [Required]
        public int Decision { get; set; }      // e.g., 1=Approve, 2=Reject, etc.

        public string? Comments { get; set; }  // optional

    }
}
