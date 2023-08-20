using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BlogAPI.Models
{
    public class Post
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string? CoverFileName { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public bool IsUpdated { get; set; } = false;
        public bool IsPublic { get; set; } = true;
        public long UserAuthId { get; set; }
        public UserAuth UserAuth { get; set; }
        public long BlogId { get; set; }
        public Blog Blog { get; set; }
        [JsonIgnore]
        public List<Comment> Comments { get; set; } = new List<Comment>();
    }
}