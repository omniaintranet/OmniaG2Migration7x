using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.Input.Social
{
    public class G1Comment
    {
        public Guid Id { get; set; }
        public string WebUrl { get; set; }
        public int PageId { get; set; }
        public string Content { get; set; }
        public Guid ParentId { get; set; }
        public bool IsDelete { get; set; }
        public string TopicId { get; set; }
        public List<G1Comment> Children { get; set; }        
        public int Level { get; set; }
        public List<G1Like> Likes { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ModifiedAt { get; set; }
    }
}
