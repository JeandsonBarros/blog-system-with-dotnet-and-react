using BlogAPI.Context;
using BlogAPI.DTOs;
using BlogAPI.Models;
using BlogAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BlogAPI.CustomExceptions;
using Microsoft.EntityFrameworkCore;

namespace BlogAPI.Controllers
{
    [ApiController]
    [Route("api/post")]
    public class PostController : ControllerBase
    {
        private readonly BlogContext _context;
        private readonly FilesService _filesService;

        public PostController(BlogContext context, FilesService filesService)
        {
            _filesService = filesService;
            _context = context;
        }

        /// <summary> Create a new post of blog </summary>
        /// <response code="201"> Return the post created </response>
        /// <response code="401"> If not authenticated </response>
        [Authorize]
        [HttpPost("{blogId}")]
        [ProducesResponseType(201)]
        public async Task<ActionResult<Response<Post>>> CreatePost(long blogId, [FromForm] PostDto postDto)
        {
            try
            {
                var userId = Convert.ToInt64(User.Identity?.Name);

                var blog = _context.Blogs
                    .Where(x => x.Id == blogId && x.UserAuthId == userId)
                    .FirstOrDefault();

                if (blog == null)
                {
                    return NotFound(new Response(message: "Blog not found.", success: false));
                }

                Post post = new()
                {
                    Title = postDto.Title,
                    Subtitle = postDto.Subtitle,
                    Text = postDto.Text,
                    IsPublic = postDto.IsPublic,
                    UserAuthId = userId,
                    Blog = blog
                };

                if (postDto.CoverFile != null)
                {
                    post.CoverFileName = await _filesService.SaveFile(postDto.CoverFile);
                }

                _context.Posts.Add(post);
                _context.SaveChanges();

                return CreatedAtAction(
                    nameof(GetUserPost),
                    new { postId = post.Id },
                    new Response<Post>(
                        data: post,
                        message: $"The post {post.Title} has been created.",
                        success: true
                    )
                );
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response(
                        message: "An internal error occurred while creating post.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
        }

        /// <summary> Get post from authenticated user </summary>
        /// <response code="200"> Return a post </response>
        [Authorize]
        [HttpGet("of-user/{postId}")]
        public ActionResult<Response<Post>> GetUserPost(long postId)
        {
            try
            {
                var userId = Convert.ToInt64(User.Identity?.Name);

                var post = _context.Posts
                    .Where(p => p.Id == postId && p.UserAuthId == userId)
                    .Include(p=>p.Blog)
                    .FirstOrDefault();

                if (post == null)
                {
                    return NotFound(new Response(message: "Post not found.", success: false));
                }

                return Ok(new Response<Post>(data: post));
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response(
                        message: "An internal error occurred while accessing post.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
        }

        /// <summary> Get posts from authenticated user </summary>
        /// <response code="200"> Return a page of posts </response>
        [Authorize]
        [HttpGet("of-user")]
        public ActionResult<PageResponse<Post>> GetUserPosts([FromQuery] Pagination pagination, [FromQuery] string? search)
        {
            try
            {
                Pagination validPagination = new(pagination.Page, pagination.Size);
                var userId = Convert.ToInt64(User.Identity?.Name);
                var posts = new List<Post>();
                var totalRecords = 0;

                if (String.IsNullOrEmpty(search))
                {
                    posts = _context.Posts
                        .Where(p => p.UserAuthId == userId)
                        .Include(p => p.Blog)
                        .OrderByDescending(p => p.Date)
                        .Skip((validPagination.Page - 1) * validPagination.Size)
                        .Take(validPagination.Size)
                        .ToList();

                    totalRecords = _context.Posts.Where(p => p.UserAuthId == userId).Count();
                }
                else
                {
                    posts = _context.Posts
                        .Where(p => p.UserAuthId == userId && p.Title.Contains(search))
                        .Include(p => p.Blog)
                        .OrderByDescending(p => p.Date)
                        .Skip((validPagination.Page - 1) * validPagination.Size)
                        .Take(validPagination.Size)
                        .ToList();

                    totalRecords = _context.Posts.Where(p => p.UserAuthId == userId && p.Title.Contains(search)).Count();
                }

                PageResponse<List<Post>> pagedResponse = new(
                    data: posts,
                    page: validPagination.Page,
                    size: validPagination.Size,
                    totalRecords: totalRecords
                );

                return Ok(pagedResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response(
                        message: "An internal error occurred when accessing posts.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
        }

        /// <summary> Get public and private posts from the user's blog </summary>
        /// <response code="200"> Return a page of posts </response>
        [Authorize]
        [HttpGet("of-user/blog-posts/{blogId}")]
        public ActionResult<PageResponse<Post>> GetUserBlogPosts([FromQuery] Pagination pagination, long blogId, [FromQuery] string? search)
        {
            try
            {
                Pagination validPagination = new(pagination.Page, pagination.Size);
                var userId = Convert.ToInt64(User.Identity?.Name);
                var posts = new List<Post>();
                var totalRecords = 0;
                var blog = _context.Blogs
                   .Where(b => b.Id == blogId && b.UserAuthId == userId)
                   .FirstOrDefault();

                if (blog == null)
                {
                    return NotFound(new Response(message: "Blog not found.", success: false));
                }

                if (String.IsNullOrEmpty(search))
                {
                    posts = _context.Posts
                       .Where(p => p.BlogId == blogId)
                       .Include(p=>p.Blog)
                       .OrderByDescending(p => p.Date)
                       .Skip((validPagination.Page - 1) * validPagination.Size)
                       .Take(validPagination.Size)
                       .ToList();

                    totalRecords = _context.Posts.Where(p => p.BlogId == blogId).Count();
                }
                else
                {
                    posts = _context.Posts
                       .Where(p => p.BlogId == blogId && p.Title.Contains(search))
                       .Include(p=>p.Blog)
                       .OrderByDescending(p => p.Date)
                       .Skip((validPagination.Page - 1) * validPagination.Size)
                       .Take(validPagination.Size)
                       .ToList();

                    totalRecords = _context.Posts.Where(p => p.BlogId == blogId && p.Title.Contains(search)).Count();
                }

                PageResponse<List<Post>> pagedResponse = new(
                    data: posts,
                    page: validPagination.Page,
                    size: validPagination.Size,
                    totalRecords: totalRecords
                );

                return Ok(pagedResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response(
                        message: "An internal error occurred when accessing posts.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
        }

        private async Task<Post> UpdatePost(long postId, PostDtoNoValidation postDto)
        {
            var userId = Convert.ToInt64(User.Identity?.Name);
            var post = _context.Posts
                .Where(p => p.Id == postId && p.UserAuthId == userId)
                .Include(p=>p.Blog)
                .FirstOrDefault();

            if (post == null)
            {
                throw new NotFoundException($"The logged in user does not have a post where the id is {postId}.");
            }

            if (!String.IsNullOrEmpty(postDto.Title))
            {
                post.Title = postDto.Title;
            }
            if (!String.IsNullOrEmpty(postDto.Subtitle))
            {
                post.Subtitle = postDto.Subtitle;
            }
            if (!String.IsNullOrEmpty(postDto.Text))
            {
                post.Text = postDto.Text;
            }
            if (postDto.IsPublic.HasValue)
            {
                post.IsPublic = postDto.IsPublic.Value;
            }
            if (postDto.CoverFile != null)
            {
                if (!String.IsNullOrEmpty(post.CoverFileName))
                {
                    _filesService.DeleteFile(post.CoverFileName);
                }

                post.CoverFileName = await _filesService.SaveFile(postDto.CoverFile);
            }

            post.IsUpdated = true;

            _context.Posts.Update(post);
            _context.SaveChanges();

            return post;
        }

        /// <summary> Fully update a post </summary>
        /// <response code="200"> Returns the updated post </response> 
        [Authorize]
        [HttpPut("{postId}")]
        [ProducesResponseType(201)]
        public async Task<ActionResult<Response<Post>>> PutUpdatePost(long postId, [FromForm] PostDto postDto)
        {
            try
            {
                PostDtoNoValidation postDtoNoValidation = new()
                {
                    Title = postDto.Title,
                    Subtitle = postDto.Subtitle,
                    Text = postDto.Text,
                    IsPublic = postDto.IsPublic,
                    CoverFile = postDto.CoverFile
                };

                return Ok(new Response<Post>(data: await UpdatePost(postId, postDtoNoValidation)));
            }
            catch (NotFoundException ex)
            {
                return StatusCode(
                    StatusCodes.Status404NotFound,
                    new Response(
                        message: "Post not found.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response(
                        message: "An internal error occurred while updating the post.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
        }

        /// <summary> Partially update a post </summary>
        /// <response code="200"> Returns the updated post </response>
        [Authorize]
        [HttpPatch("{postId}")]
        [ProducesResponseType(201)]
        public async Task<ActionResult<Response<Post>>> PatchUpdatePost(long postId, [FromForm] PostDtoNoValidation postDto)
        {
            try
            {
                System.Console.WriteLine(postDto.IsPublic);
                return Ok(new Response<Post>(data: await UpdatePost(postId, postDto)));
            }
            catch (NotFoundException ex)
            {
                return StatusCode(
                    StatusCodes.Status404NotFound,
                    new Response(
                        message: "Post not found.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response(
                        message: "An internal error occurred while updating the post.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
        }

        /// <summary> Delete a post </summary>
        /// <response code="204"> Not a content is returned  </response>
        [Authorize]
        [HttpDelete("{postId}")]
        [ProducesResponseType(204)]
        public IActionResult DeletePost(long postId)
        {
            try
            {
                var userId = Convert.ToInt64(User.Identity?.Name);
                var post = _context.Posts
                    .Where(p => p.Id == postId && p.UserAuthId == userId)
                    .FirstOrDefault();

                if (post == null)
                {
                    return NotFound(new Response(message: "Post not found.", success: false));
                }

                if (!String.IsNullOrEmpty(post.CoverFileName))
                {
                    _filesService.DeleteFile(post.CoverFileName);
                }

                _context.Posts.Remove(post);
                _context.SaveChanges();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response(
                        message: "An internal error occurred while deleting post.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
        }

        /// <summary> Delete post cover image </summary>
        /// <response code="204"> If post cover image deleted success </response>
        /// <response code="401"> if unauthenticated </response>
        [Authorize]
        [HttpDelete("delete-cover-post/{postId}")]
        [ProducesResponseType(204)]
        public IActionResult DeleteCoverImage(long postId)
        {
            try
            {
                var userId = Convert.ToInt64(User.Identity?.Name);
                var post = _context.Posts
                    .Where(p => p.Id == postId && p.UserAuthId == userId)
                    .FirstOrDefault();

                if (post == null)
                {
                    return NotFound(new Response(message: "Post not found.", success: false));
                }

                if (String.IsNullOrEmpty(post.CoverFileName))
                {
                    return NotFound(new Response(
                          message: "There is no cover to remove.",
                          success: false
                    ));
                }

                _filesService.DeleteFile(post.CoverFileName);

                post.CoverFileName = null;
                _context.Posts.Update(post);
                _context.SaveChanges();

                return StatusCode(StatusCodes.Status204NoContent);
            }
            catch (System.Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response(
                        message: "An internal server error occurred while deleting the cover image.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
        }

        /// <summary> Get a public post </summary>
        /// <response code="200"> Return a post </response>
        [AllowAnonymous]
        [HttpGet("public/{postId}")]
        public ActionResult<Response<Post>> GetPublicPost(long postId)
        {
            try
            {
                var post = _context.Posts
                    .Where(p => p.Id == postId && p.IsPublic && p.Blog.IsPublic)
                    .Include(p=>p.Blog)
                    .FirstOrDefault();

                if (post == null)
                {
                    return NotFound(new Response(message: "Post not found.", success: false));
                }

                return Ok(new Response<Post>(data: post));
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response(
                        message: "An internal error occurred while accessing post.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
        }

        /// <summary> Get public posts </summary>
        /// <response code="200"> Return a page of posts </response>
        [AllowAnonymous]
        [HttpGet("public")]
        public ActionResult<PageResponse<Post>> GetPublicPosts([FromQuery] Pagination pagination, [FromQuery] string? search)
        {
            try
            {
                Pagination validPagination = new(pagination.Page, pagination.Size);
                var posts = new List<Post>();
                var totalRecords = 0;

                if (String.IsNullOrEmpty(search))
                {
                    posts = _context.Posts
                          .Where(p => p.IsPublic && p.Blog.IsPublic)
                          .Include(p => p.Blog)
                          .OrderByDescending(p => p.Date)
                          .Skip((validPagination.Page - 1) * validPagination.Size)
                          .Take(validPagination.Size)
                          .ToList();

                    totalRecords = _context.Posts.Where(p => p.IsPublic && p.Blog.IsPublic).Count();
                }
                else
                {
                    posts = _context.Posts
                           .Where(p => p.IsPublic && p.Blog.IsPublic && p.Title.Contains(search))
                           .Include(p => p.Blog)
                           .OrderByDescending(p => p.Date)
                           .Skip((validPagination.Page - 1) * validPagination.Size)
                           .Take(validPagination.Size)
                           .ToList();

                    totalRecords = _context.Posts.Where(p => p.IsPublic && p.Blog.IsPublic && p.Title.Contains(search)).Count();
                }

                PageResponse<List<Post>> pagedResponse = new(
                    data: posts,
                    page: validPagination.Page,
                    size: validPagination.Size,
                    totalRecords: totalRecords
                );

                return Ok(pagedResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response(
                        message: "An internal error occurred when accessing posts.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
        }

        /// <summary> Get public blog posts </summary>
        /// <response code="200"> Return a page of posts </response>
        [AllowAnonymous]
        [HttpGet("public/blog-posts/{blogId}")]
        public ActionResult<PageResponse<Post>> GetPublicBlogPosts([FromQuery] Pagination pagination, long blogId, [FromQuery] string? search)
        {
            try
            {
                Pagination validPagination = new(pagination.Page, pagination.Size);
                var posts = new List<Post>();
                var totalRecords = 0;
                var blog = _context.Blogs
                   .Where(b => b.Id == blogId && b.IsPublic)
                   .FirstOrDefault();

                if (blog == null)
                {
                    return NotFound(new Response(message: "Blog not found.", success: false));
                }

                if (String.IsNullOrEmpty(search))
                {
                    posts = _context.Posts
                        .Where(p => p.IsPublic && p.Blog.IsPublic && p.BlogId == blogId)
                        .Include(p=>p.Blog)
                        .OrderByDescending(p => p.Date)
                        .Skip((validPagination.Page - 1) * validPagination.Size)
                        .Take(validPagination.Size)
                        .ToList();

                    totalRecords = _context.Posts
                       .Where(p => p.IsPublic && p.Blog.IsPublic && p.BlogId == blogId)
                       .Count();
                }
                else
                {
                    posts = _context.Posts
                        .Where(p => p.IsPublic && p.Blog.IsPublic && p.BlogId == blogId && p.Title.Contains(search))
                        .Include(p=>p.Blog)
                        .OrderByDescending(p => p.Date)
                        .Skip((validPagination.Page - 1) * validPagination.Size)
                        .Take(validPagination.Size)
                        .ToList();

                    totalRecords = _context.Posts
                       .Where(p => p.IsPublic && p.Blog.IsPublic && p.BlogId == blogId && p.Title.Contains(search))
                       .Count();
                }

                PageResponse<List<Post>> pagedResponse = new(
                    data: posts,
                    page: validPagination.Page,
                    size: validPagination.Size,
                    totalRecords: totalRecords
                );

                return Ok(pagedResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response(
                        message: "An internal error occurred when accessing posts.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
        }

        /// <summary> Get post cover picture </summary>
        /// <response code="200"> Return a file </response>
        [HttpGet("cover-picture/{fileName}")]
        public IActionResult GetCoverImage(string fileName)
        {
            try
            {
                var userId = Convert.ToInt64(User.Identity?.Name);
                var post = _context.Posts.Where(p => p.CoverFileName == fileName).Include(p => p.Blog).FirstOrDefault();
                if (
                    post == null
                    || (!post.Blog.IsPublic && post.UserAuthId != userId)
                    || (!post.IsPublic && post.UserAuthId != userId)
                )
                {
                    return NotFound(new Response(message: "There is no post with the image provided.", success: false));
                }

                return _filesService.GetFile(fileName);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new Response(message: ex.Message, success: false));
            }
            catch (Exception ex)
            {
                return StatusCode(
                     StatusCodes.Status500InternalServerError,
                     new Response(
                         message: "An internal server error occurred while retrieving the file.",
                         success: false,
                         details: ex.Message
                     )
                 );
            }
        }

    }
}