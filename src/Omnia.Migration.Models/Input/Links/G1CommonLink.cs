using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.Input.Links
{
    public enum G1IconType
    {
        Font,
        Custom
    }
    public class G1IconItem
    {
        public G1IconType IconType { get; set; }
        public string FontValue { get; set; }
        public string CustomValue { get; set; }
        public string BackgroundColor { get; set; }
    }
    public class G1CommonLink
    {
        public Guid LinkId { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string Category { get; set; }
        public string Information { get; set; }
        public G1IconItem Icon { get; set; }
        public bool IsOpenNewWindow { get; set; }
        public string LimitAccess { get; set; }
        public bool Mandatory { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ModifiedAt { get; set; }
    }
}
