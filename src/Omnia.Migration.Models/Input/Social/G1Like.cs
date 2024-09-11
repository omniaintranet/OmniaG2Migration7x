using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.Input.Social
{
    public class G1Like
    {
        public string WebUrl { get; set; }
        public int PageId { get; set; }
        public string CommentId { get; set; }
        public string TopicId { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ModifiedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public string ModifiedBy { get; set; }
    }
}
