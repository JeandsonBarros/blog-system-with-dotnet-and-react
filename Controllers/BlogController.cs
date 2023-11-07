using BlogAPI.Context;
using BlogAPI.CustomExceptions;
using BlogAPI.DTOs;
using BlogAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogAPI.Controllers
{

    [ApiController]
    [Route("api/blog")]
    public class BlogController : ControllerBase
    {
        private readonly BlogContext _context;

        public BlogController(BlogContext context)
        {
            _context = context;
        }

        /// <summary> Create a new blog </summary>
        /// <response code="201"> Return the blog created </response>
        /// <response code="401"> If not authenticated </response>
        [Authorize]
        [HttpPost]
        [ProducesResponseType(201)]
        public ActionResult<Response<Blog>> CreateBlog(BlogDto blogDto)
        {
            try
            {
                var userId = Convert.ToInt64(User.Identity?.Name);

                Blog blog = new()
                {
                    Title = blogDto.Title,
                    Description = blogDto.Description,
                    HeaderColor = blogDto.HeaderColor,
                    TitleColor = blogDto.TitleColor,
                    IsPublic = blogDto.IsPublic,
                    UserAuthId = userId
                };

                _context.Blogs.Add(blog);
                _context.SaveChanges();

                return CreatedAtAction(
                    nameof(GetUserBlog), new { id = blog.Id },
                    new Response<Blog>(data: blog)
                );
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response(
                        message: "An internal error occurred while saving the blog.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
        }

        /// <summary> Get user blog by id </summary>
        /// <response code="200"> Return a blog </response>
        [Authorize]
        [HttpGet("of-user/{id}")]
        public ActionResult<Response<Blog>> GetUserBlog(long id)
        {
            try
            {
                var userId = Convert.ToInt64(User.Identity?.Name);
                var blog = _context.Blogs
                .Where(b => b.Id == id && b.UserAuthId == userId)
                .Include(b => b.UserAuth)
                .FirstOrDefault();

                if (blog == null)
                {
                    return NotFound(new Response(message: "Blog not found.", success: false));
                }

                return Ok(new Response<Blog>(data: blog));
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response(
                        message: "An internal error occurred when accessing the blog.",
                        success: false,
                        details: ex.ToString()
                    )
                );
            }
        }

        /// <summary> List all blogs of logged in user </summary>
        /// <response code="200"> Return a blog page </response> 
        [Authorize]
        [HttpGet("of-user")]
        public ActionResult<PageResponse<Blog>> GetUserBlogs([FromQuery] Pagination pagination, [FromQuery] string? search)
        {
            try
            {
                Pagination validPagination = new(pagination.Page, pagination.Size);
                var userId = Convert.ToInt64(User.Identity?.Name);
                var blogs = new List<Blog>();
                var totalRecords = 0;

                if (String.IsNullOrEmpty(search))
                {
                    blogs = _context.Blogs
                        .Where(b => b.UserAuthId == userId)
                        .Include(b => b.UserAuth)
                        .Skip((validPagination.Page - 1) * validPagination.Size)
                        .Take(validPagination.Size)
                        .ToList();

                    totalRecords = _context.Blogs.Where(b => b.UserAuthId == userId).Count();
                }
                else
                {
                    blogs = _context.Blogs
                        .Where(b => b.UserAuthId == userId && b.Title.Contains(search))
                        .Include(b => b.UserAuth)
                        .Skip((validPagination.Page - 1) * validPagination.Size)
                        .Take(validPagination.Size)
                        .ToList();

                    totalRecords = _context.Blogs.Where(b => b.UserAuthId == userId && b.Title.Contains(search)).Count();
                }

                PageResponse<List<Blog>> pagedResponse = new(
                    data: blogs,
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
                        message: "An internal error occurred while listing blogs.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
        }

        /// <summary> Fully update a blog </summary>
        /// <response code="200"> Returns the updated blog </response> 
        [Authorize]
        [HttpPut("{id}")]
        public ActionResult<Response<Blog>> PutBlog(long id, BlogDto blogDto)
        {
            try
            {
                BlogDtoNoValidation blogDtoNoValidation = new()
                {
                    Title = blogDto.Title,
                    Description = blogDto.Description,
                    HeaderColor = blogDto.HeaderColor,
                    TitleColor = blogDto.TitleColor,
                    IsPublic = blogDto.IsPublic,
                };

                return Ok(new Response<Blog>(data: UpdateBlog(id, blogDtoNoValidation)));
            }
            catch (NotFoundException ex)
            {
                return StatusCode(
                    StatusCodes.Status404NotFound,
                    new Response(
                        message: "Blog not found.",
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
                        message: "An internal error occurred while updating blog.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
        }

        /// <summary> Partially update a blog </summary>
        /// <response code="200"> Returns the updated blog </response>
        [Authorize]
        [HttpPatch("{id}")]
        public ActionResult<Response<Blog>> PatchBlog(long id, BlogDtoNoValidation blogDto)
        {
            try
            {
                return Ok(new Response<Blog>(data: UpdateBlog(id, blogDto)));
            }
            catch (NotFoundException ex)
            {
                return StatusCode(
                    StatusCodes.Status404NotFound,
                    new Response(
                        message: "Blog not found.",
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
                        message: "An internal error occurred while updating blog.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
        }

        /// <summary>
        /// Function for partial or total update of a blog
        /// </summary>
        /// <param name="id"> Id of the blog that will be updated </param>
        /// <param name="blogDto"> Dto containing the new data </param>
        /// <returns> Returns the updated blog </returns>
        private Blog UpdateBlog(long id, BlogDtoNoValidation blogDto)
        {
            var userId = Convert.ToInt64(User.Identity?.Name);
            var blog = _context.Blogs
                .Where(b => b.Id == id && b.UserAuthId == userId)
                .Include(b => b.UserAuth)
                .FirstOrDefault();

            if (blog == null)
            {
                throw new NotFoundException($"The logged in user does not have a blog where the id is {id}.");
            }

            if (!String.IsNullOrEmpty(blogDto.Title))
            {
                blog.Title = blogDto.Title;
            }
            if (!String.IsNullOrEmpty(blogDto.Description))
            {
                blog.Description = blogDto.Description;
            }
            if (!String.IsNullOrEmpty(blogDto.HeaderColor))
            {
                blog.HeaderColor = blogDto.HeaderColor;
            }
            if (!String.IsNullOrEmpty(blogDto.TitleColor))
            {
                blog.TitleColor = blogDto.TitleColor;
            }
            if (blogDto.IsPublic.HasValue)
            {
                blog.IsPublic = blogDto.IsPublic.Value;
            }

            _context.Blogs.Update(blog);
            _context.SaveChanges();

            return blog;
        }

        /// <summary> Delete a blog </summary>
        /// <response code="204"> Not a content is returned </response>
        [Authorize]
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        public IActionResult DeleteBlog(long id)
        {
            try
            {
                var userId = Convert.ToInt64(User.Identity?.Name);

                var blog = _context.Blogs.Where(b => b.Id == id && b.UserAuthId == userId).FirstOrDefault();
                if (blog == null)
                {
                    return NotFound(new Response(message: "Blog not found.", success: false));
                }

                _context.Blogs.Remove(blog);
                _context.SaveChanges();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response(
                        message: "An internal error occurred when deleting blog.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
        }

        /// <summary> Get public blog by id </summary>
        /// <response code="200"> Return a blog </response>
        [AllowAnonymous]
        [HttpGet("public/{id}")]
        public ActionResult<Response<Blog>> GetPublicBlog(long id)
        {
            try
            {
                var blog = _context.Blogs.Where(b => b.Id == id && b.IsPublic)
                    .Include(b => b.UserAuth)
                    .FirstOrDefault();

                if (blog == null)
                {
                    return NotFound(new Response(message: "Blog not found.", success: false));
                }

                return Ok(new Response<Blog>(data: blog));
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response(
                        message: "An internal error occurred when accessing the blog.",
                        success: false,
                        details: ex.ToString()
                    )
                );
            }
        }

        /// <summary> List public blogs </summary>
        /// <response code="200"> Return a blog page </response>        
        [AllowAnonymous]
        [HttpGet("public")]
        public ActionResult<PageResponse<Blog>> GetPublicBlogs([FromQuery] Pagination pagination, [FromQuery] string? search)
        {
            try
            {
                Pagination validPagination = new(pagination.Page, pagination.Size);
                var blogs = new List<Blog>();
                var totalRecords = 0;

                if (String.IsNullOrEmpty(search))
                {
                    blogs = _context.Blogs
                        .Where(b => b.IsPublic)
                        .Include(b => b.UserAuth)
                        .Skip((validPagination.Page - 1) * validPagination.Size)
                        .Take(validPagination.Size)
                        .ToList();

                    totalRecords = _context.Blogs.Where(b => b.IsPublic).Count();
                }
                else
                {
                    blogs = _context.Blogs
                        .Where(b => b.IsPublic && b.Title.Contains(search))
                        .Include(b => b.UserAuth)
                        .Skip((validPagination.Page - 1) * validPagination.Size)
                        .Take(validPagination.Size)
                        .ToList();

                    totalRecords = _context.Blogs.Where(b => b.IsPublic && b.Title.Contains(search)).Count();
                }

                PageResponse<List<Blog>> pagedResponse = new(
                    data: blogs,
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
                        message: "An internal error occurred while listing blogs.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
        }

    }
}