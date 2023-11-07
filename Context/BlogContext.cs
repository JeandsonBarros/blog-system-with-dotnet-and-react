using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BlogAPI.Models;

namespace BlogAPI.Context
{
    public class BlogContext: DbContext
    {
        public BlogContext(DbContextOptions<BlogContext> options): base(options){
        }

        public DbSet<UserAuth> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<AuthorizationCode> AuthorizationCodes { get; set; }
    }
}