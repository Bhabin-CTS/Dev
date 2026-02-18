namespace Account_Track.DTOs.AccountDto
{
    /// <summary>
    /// Mirrors Transaction CreateTnxSpResult pattern for SP output handling.
    /// </summary>
    public class CreateAccountSpResult
    {
        public int Success { get; set; }
        public string? ErrorCode { get; set; }
        public string Message { get; set; } = string.Empty;

        public int? AccountId { get; set; }
        public int? AccountNumber { get; set; }

        // Keep enum ints to match raw SP output if needed
        public int? AccountType { get; set; }
        public int? Status { get; set; }

        public decimal? Balance { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
