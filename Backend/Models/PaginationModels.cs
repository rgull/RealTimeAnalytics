using System.ComponentModel.DataAnnotations;

namespace RealTimeSensorTrack.Models
{
    public class PaginationRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
        public int Page { get; set; } = 1;

        [Range(1, 1000, ErrorMessage = "Page size must be between 1 and 1000")]
        public int PageSize { get; set; } = 50;

        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
    }

    public class PaginatedResponse<T>
    {
        public IEnumerable<T> Data { get; set; } = new List<T>();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }

        public PaginatedResponse(IEnumerable<T> data, int page, int pageSize, int totalCount)
        {
            Data = data;
            Page = page;
            PageSize = pageSize;
            TotalCount = totalCount;
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            HasNextPage = page < TotalPages;
            HasPreviousPage = page > 1;
        }
    }

    public class SensorReadingsFilter : PaginationRequest
    {
        public int? SensorId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? SensorType { get; set; }
    }

    public class AlertsFilter : PaginationRequest
    {
        public int? SensorId { get; set; }
        public string? Severity { get; set; }
        public bool? IsResolved { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
