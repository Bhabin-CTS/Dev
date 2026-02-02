namespace Account_Track.DTOs
{
    public class PaginationDto
    {
        public int Total { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public int Page => (Offset / Limit) + 1;
        public int Pages => (int)Math.Ceiling((double)Total / Limit);
        public bool HasNextPage => Offset + Limit < Total;
        public bool HasPreviousPage => Offset > 0;
    }
}
