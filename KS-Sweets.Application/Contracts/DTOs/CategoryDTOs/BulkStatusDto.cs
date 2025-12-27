namespace KS_Sweets.Application.Contracts.DTOs.CategoryDTOs
{
    public class BulkStatusDto
    {
        public int[] Ids { get; set; } = [];
        public bool IsActive { get; set; }
    }
}
