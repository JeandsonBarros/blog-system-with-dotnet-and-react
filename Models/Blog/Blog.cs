using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BlogAPI.Models
{
    public class Blog
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Matter { get; set; }
        public string ColorPrimary { get; set; }
        public string ColorSecondary { get; set; }
        public bool IsPublic { get; set; } = true;
        public long UserAuthId { get; set; }
        public UserAuth UserAuth { get; set; }
        [JsonIgnore]
        public List<Post> Posts { get; set; } = new List<Post>();
    }
}