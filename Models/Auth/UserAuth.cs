using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BlogAPI.Models
{
    public class UserAuth
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string? FileProfilePictureName { get; set; }
        [JsonIgnore]
        public string Password { get; set; }
        public bool IsConfirmedEmail { get; set; } = false;
        public IList<Role> Roles { get; set; } = new List<Role>();
        [JsonIgnore]
        public List<Blog> Blogs { get; set; } = new List<Blog>();
        [JsonIgnore]
        public List<Post> Posts { get; set; } = new List<Post>();
        [JsonIgnore]
        public List<Comment> Comments { get; set; } = new List<Comment>();
    }
}