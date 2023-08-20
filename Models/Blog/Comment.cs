using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogAPI.Models
{
    public class Comment
    {
        public long Id { get; set; }
        public string CommentText { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public bool IsUpdated { get; set; } = false;
        public long PostId { get; set; }
        public Post Post { get; set; }
        public long UserAuthId { get; set; }
        public UserAuth User { get; set; }
    }
}