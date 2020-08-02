using Microsoft.EntityFrameworkCore.Migrations;

namespace Swish.Migrations
{
    public partial class StoredProcs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			var CheckIfFriend = @"CREATE PROCEDURE [dbo].[CheckIfFriend] @MyUserName VARCHAR(256), @ChkUserName VARCHAR(256)
			AS
			BEGIN
				SET NOCOUNT ON;
				SELECT * FROM (
				SELECT * 
				FROM AspNetUsers 
				WHERE UserName IN ( 
					( SELECT User2 FROM Friends WHERE User1 = @MyUserName) 
					UNION 
					( SELECT User1 FROM Friends WHERE User2 = @MyUserName) 
				)) AS ASD
				WHERE UserName = @ChkUserName
			END
			GO";
			migrationBuilder.Sql(CheckIfFriend);

			var GetFriends = @"CREATE PROCEDURE [dbo].[GetFriends] @MyUserName VARCHAR(256)
			AS
			BEGIN
				SET NOCOUNT ON;
				SELECT * 
				FROM AspNetUsers 
				WHERE UserName IN ( 
					( SELECT User2 FROM Friends WHERE (User1 = @MyUserName AND Confirmed = 1)) 
					UNION 
					( SELECT User1 FROM Friends WHERE (User2 = @MyUserName AND Confirmed = 1)) 
				)
			END
			GO";
			migrationBuilder.Sql(GetFriends);

			var GetNotifications = @"CREATE PROCEDURE [dbo].[GetNotifications]
				@UserName VARCHAR(256)
			AS
			BEGIN
				SET NOCOUNT ON;

				SELECT * 
				FROM AspNetUsers 
				WHERE UserName IN ((SELECT User1 FROM Friends WHERE (User2 = @UserName AND Confirmed = 0)))
			END
			GO";
			migrationBuilder.Sql(GetNotifications);

			var GetUserPosts = @"CREATE PROCEDURE [dbo].[GetUserPosts] @UserName VARCHAR(256)
			AS
			BEGIN

				SET NOCOUNT ON;

				SELECT *
				FROM AspNetUsers, UserPost
				WHERE AspNetUsers.Id = UserPost.UserID AND AspNetUsers.UserName = @UserName
			END
			GO";
			migrationBuilder.Sql(GetUserPosts);

			var LikeComment = @"CREATE PROCEDURE [dbo].[LikeComment]
				@UName VARCHAR(256),
				@CommentId INT
			AS
			BEGIN
				SET NOCOUNT ON;
				IF (NOT EXISTS (SELECT * FROM [LikedComments] WHERE CommentId = @CommentId AND UName = @UName))
				BEGIN
					UPDATE CommentsModels SET cLikes = cLikes + 1 WHERE CommentID = @CommentId
					INSERT INTO [LikedComments] (CommentId , UName) VALUES (@CommentId, @UName)
				END
				ELSE
				BEGIN
					UPDATE CommentsModels SET cLikes = cLikes - 1 WHERE CommentID = @CommentId
					DELETE FROM [LikedComments] WHERE CommentId = @CommentId AND UName = @UName
				END
				SELECT * FROM CommentsModels WHERE CommentID = @CommentId
			END
			GO";
			migrationBuilder.Sql(LikeComment);

			var LikePost = @"CREATE PROCEDURE [dbo].[LikePost]
				@UName VARCHAR(256),
				@PostID INT
			AS
			BEGIN
				SET NOCOUNT ON;
				IF (NOT EXISTS (SELECT * FROM [LikedPosts] WHERE PostId = @PostID AND UName = @UName))
				BEGIN
					UPDATE [UserPost] SET PLikes = PLikes + 1 WHERE PostID = @PostID
					INSERT INTO LikedPosts (PostId , UName) VALUES (@PostID, @UName)
				END
				ELSE
				BEGIN
					UPDATE [UserPost] SET PLikes = PLikes - 1 WHERE PostID = @PostID
					DELETE FROM LikedPosts WHERE PostId = @PostID AND UName = @UName
				END
				SELECT TOP 1 * FROM UserPost WHERE PostId = @PostID
			END
			GO";
			migrationBuilder.Sql(LikePost);

			var RemoveFriend = @"CREATE PROCEDURE [dbo].[RemoveFriend] @MyUserName varchar(255), @UserName varchar(255)
			AS
			BEGIN
				SET NOCOUNT ON;
				delete
				FROM Friends 
				WHERE (FriendKey like '%'+@MyUserName+'%') AND (FriendKey like '%'+@UserName+'%')
			END
			GO";
			migrationBuilder.Sql(RemoveFriend);
		}

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
