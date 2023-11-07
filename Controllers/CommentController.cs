using BlogAPI.Context;
using BlogAPI.DTOs;
using BlogAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogAPI.Controllers
{
    [ApiController]
    [Route("api/comment")]
    public class CommentController : ControllerBase
    {
        private readonly BlogContext _context;

        public CommentController(BlogContext context)
        {
            _context = context;
        }

        /// <summary> Create a new comment of post </summary>
        /// <response code="201"> Return the comment created </response>
        [Authorize]
        [HttpPost("{postId}")]
        [ProducesResponseType(201)]
        public ActionResult<Response<Comment>> CreateComment(long postId, CommentDto commentDto)
        {
            try
            {
                var post = _context.Posts.Where(p => p.Id == postId && p.IsPublic).FirstOrDefault();
                if (post == null)
                {
                    return NotFound(new Response(message: "Post not found.", success: false));
                }

                var userId = Convert.ToInt64(User.Identity?.Name);
                Comment comment = new()
                {
                    CommentText = commentDto.CommentText,
                    UserAuthId = userId,
                    PostId = postId
                };

                _context.Comments.Add(comment);
                _context.SaveChanges();

                return new Response<Comment>(data: comment);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response(
                        message: "An internal error occurred while creating comment.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
        }

        /// <summary> Get user comments </summary>
        /// <response code="200"> Return a comments page </response>
        [Authorize]
        [HttpGet("user-comments")]
        public ActionResult<PageResponse<Comment>> UserComments([FromQuery] Pagination pagination)
        {
            try
            {
                var userId = Convert.ToInt64(User.Identity?.Name);
                Pagination validPagination = new(pagination.Page, pagination.Size);

                var comments = _context.Comments
                    .Where(c => c.UserAuthId == userId && c.Post.IsPublic)
                    .Skip((validPagination.Page - 1) * pagination.Size)
                    .Take(validPagination.Size)
                    .ToList();

                var totalRecords = _context.Comments.Where(c => c.UserAuthId == userId && c.Post.IsPublic).Count();

                PageResponse<List<Comment>> pageResponse = new(
                    data: comments,
                    page: validPagination.Page,
                    size: validPagination.Size,
                    totalRecords: totalRecords
                );

                return Ok(pageResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response(
                        message: "An internal error occurred when accessing comments.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
        }

        /// <summary> Get post comments </summary>
        /// <response code="200"> Return a comments page </response>
        [HttpGet("post-comments/{postId}")]
        public ActionResult<PageResponse<Comment>> PostComments([FromQuery] Pagination pagination, long postId)
        {
            try
            {
                var userId = Convert.ToInt64(User.Identity?.Name);
                var post = _context.Posts.Find(postId);
                if (post == null || (!post.IsPublic && post.UserAuthId != userId))
                {
                    return NotFound(new Response(message: "Post not found.", success: false));
                }

                Pagination validPagination = new(pagination.Page, pagination.Size);

                var comments = _context.Comments
                    .Where(c => c.PostId == postId)
                    .Include(c => c.User)
                    .Skip((validPagination.Page - 1) * pagination.Size)
                    .Take(validPagination.Size)
                    .ToList();

                var totalRecords = _context.Comments.Where(c => c.PostId == postId).Count();

                PageResponse<List<Comment>> pageResponse = new(
                    data: comments,
                    page: validPagination.Page,
                    size: validPagination.Size,
                    totalRecords: totalRecords
                );

                return Ok(pageResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response(
                        message: "An internal error occurred when accessing comments.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
        }

        /// <summary> Update a comment </summary>
        /// <response code="200"> Return the comment updated </response>
        [Authorize]
        [HttpPut("{commentId}")]
        public ActionResult<Response<Comment>> UpdateComment(long commentId, CommentDto commentDto)
        {
            try
            {
                var userId = Convert.ToInt64(User.Identity?.Name);
                var comment = _context.Comments.Where(c => c.Id == commentId && c.UserAuthId == userId).FirstOrDefault();
                if (comment == null)
                {
                    return NotFound(new Response(message: "Comment not found.", success: false));
                }

                comment.CommentText = commentDto.CommentText;
                comment.IsUpdated = true;

                _context.Comments.Update(comment);
                _context.SaveChanges();

                return new Response<Comment>(data: comment);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response(
                        message: "An internal error occurred while updating the comment.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
        }

        /// <summary> Delete a comment </summary>
        /// <response code="204"> No content is returned </response>
        [Authorize]
        [HttpDelete("{commentId}")]
        [ProducesResponseType(204)]
        public IActionResult DeleteComment(long commentId)
        {
            try
            {
                var userId = Convert.ToInt64(User.Identity?.Name);
                var comment = _context.Comments.Where(c => c.Id == commentId && c.UserAuthId == userId).FirstOrDefault();
                if (comment == null)
                {
                    return NotFound(new Response(message: "Comment not found.", success: false));
                }

                _context.Comments.Remove(comment);
                _context.SaveChanges();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response(
                        message: "An internal error occurred while deleting the comment.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
        }
    }
}