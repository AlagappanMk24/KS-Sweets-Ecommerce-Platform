using System.Data.Common;

namespace KS_Sweets.Domain.Models.Request
{
    public class DataTablesRequest
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public Search Search { get; set; } = new();
        public List<Order> Order { get; set; } = new();
        public List<Column> Columns { get; set; } = new();
    }
    public class Search
    {
        public string Value { get; set; } = string.Empty;
        public bool Regex { get; set; }
    }

    public class Order
    {
        public int Column { get; set; }
        public string Dir { get; set; } = string.Empty;
    }

    public class Column
    {
        public string Data { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool Orderable { get; set; }
    }
}