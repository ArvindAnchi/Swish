using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Swish.Areas.Identity.Data;
using Swish.Models;

namespace Swish.Data
{
    public class SwishDBContext : IdentityDbContext<SwishUser>
    {

        public SwishDBContext(DbContextOptions<SwishDBContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
            // ...
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<PostImages>()
                .HasOne(e => e.postModel)
                .WithMany(e => e.postImages)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<IsFriendViewModel>().HasNoKey();
        }
        public DbSet<FriendsModel> Friends { get; set; }
        public DbSet<PostModel> UserPost { get; set; }
        public DbSet<ChatModel> ChatModel { get; set; }
        public DbSet<CommentModel> CommentsModels { get; set; }
        public DbSet<LikedComments> LikedComments { get; set; }
        public DbSet<LikedPosts> LikedPosts { get; set; }
        public DbSet<PostImages> PostImages { get; set; }
        public DbSet<IsFriendViewModel> IsFriendViewModel { get; set; }
        public DbSet<BlockedModel> BlockedModel { get; set; }

    }
}
