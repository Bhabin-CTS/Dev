namespace Account_Track.Utils.Enum
{
    public enum Status 
    { 
        Unread=1, 
        Read=2 
    }
    
    public enum NotificationType 
    {
        ApprovalReminder = 1, 
        SuspiciousActivity = 2, 
        SystemMessage = 3, 
        TransactionUpdate = 4, 
        SecurityAlert = 5
    }
}
