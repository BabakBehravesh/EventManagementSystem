namespace EventManagementSystem.Presentation.DTOs
{
    public record ApiPaginatedResponse<T>: ApiResponse<IEnumerable<T>>
    {
        public int PageNumber { get; init; }

        public int PageSize { get; init; }

        public int TotalCount { get; init; }

        public int TotalPages { get; init; }

        public bool HasPrevious => PageNumber > 1;

        public bool HasNext => PageNumber < TotalPages;

        public static ApiPaginatedResponse<T> SuccessResult(
            IEnumerable<T> data,
            int pageNumber,
            int pageSize, 
            int totalCount,
            string message ="Request successful!")
        {
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            return new ApiPaginatedResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                StatusCode = StatusCodes.Status200OK
            };
        }
    }
}
 