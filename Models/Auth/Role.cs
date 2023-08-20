using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BlogAPI.Models
{
    public class Role
    {
        public long Id { get; set; }
        public string RoleName {get; set; }
        [JsonIgnore]
        public List<UserAuth> Users { get; set; }
    }
}