using BlogAPI.Context;
using BlogAPI.CustomExceptions;
using BlogAPI.DTOs;
using BlogAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
                var userId = Convert.ToInt64(User?.Identity?.Name);

                Blog blog = new()
                {
                    Name = blogDto.Name,
                    Matter = blogDto.Matter,
                    ColorPrimary = blogDto.ColorPrimary,
                    ColorSecondary = blogDto.ColorSecondary,
                    IsPublic = blogDto.IsPublic,
                    UserAuthId = userId
                };

                _context.Blogs.Add(blog);
                _context.SaveChanges();

                return CreatedAtAction(
                    nameof(GetUserBlogById), new { id = blog.Id },
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

        /// <summary> Get a user blog by id </summary>
        /// <response code="200"> Return a blog </response>
        [Authorize]
        [HttpGet("user/{id}")]
        public ActionResult<Response<Blog>> GetUserBlogById(long id)
        {
            try
            {
                var userId = Convert.ToInt64(User?.Identity?.Name);

                var blog = _context.Blogs.Where(b => b.Id == id && b.UserAuthId == userId).FirstOrDefault();
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
        [HttpGet("user")]
        public ActionResult<PageResponse<Blog>> GetUserBlogs([FromQuery] Pagination pagination)
        {
            try
            {
                var userId = Convert.ToInt64(User?.Identity?.Name);

                Pagination validPagination = new(pagination.Page, pagination.Size);

                var blogs = _context.Blogs
                    .Where(b => b.UserAuthId == userId)
                    .Skip((validPagination.Page - 1) * validPagination.Size)
                    .Take(validPagination.Size)
                    .ToList();

                var totalRecords = _context.Blogs.Where(b => b.UserAuthId == userId).Count();

                string baseUri = $"{Request.Scheme}://{Request.Host}{Request.Path.Value}";

                PageResponse<List<Blog>> pagedResponse = new(
                    data: blogs,
                    page: validPagination.Page,
                    size: validPagination.Size,
                    totalRecords: totalRecords,
                    uri: baseUri
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

        /// <summary> Find blog among user blogs </summary>
        /// <response code="200"> Return a blog page </response> 
        [Authorize]
        [HttpGet("user/find-by-name/{name}")]
        public ActionResult<PageResponse<Blog>> FindUserBlog(string name, [FromQuery] Pagination pagination)
        {
            try
            {
                var userId = Convert.ToInt64(User.Identity?.Name);

                Pagination validPagination = new(pagination.Page, pagination.Size);

                var blogs = _context.Blogs
                    .Where(b => b.UserAuthId == userId && b.Name.Contains(name))
                    .Skip((validPagination.Page - 1) * validPagination.Size)
                    .Take(validPagination.Size)
                    .ToList();

                var totalRecords = _context.Blogs.Where(b => b.UserAuthId == userId && b.Name.Contains(name)).Count();

                string baseUri = $"{Request.Scheme}://{Request.Host}{Request.Path.Value}";

                PageResponse<List<Blog>> pagedResponse = new(
                    data: blogs,
                    page: validPagination.Page,
                    size: validPagination.Size,
                    totalRecords: totalRecords,
                    uri: baseUri
                );

                return Ok(pagedResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response(
                        message: "An internal error occurred when searching for blog.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
        }

        /// <summary> Find public blog by matter </summary>
        /// <response code="200"> Return a blog page </response>
        [Authorize]
        [HttpGet("user/find-by-matter/{matter}")]
        public ActionResult<PageResponse<Blog>> FindUserBlogByMatter(string matter, [FromQuery] Pagination pagination)
        {
            try
            {
                var userId = Convert.ToInt64(User.Identity?.Name);

                Pagination validPagination = new(pagination.Page, pagination.Size);

                var blogs = _context.Blogs
                    .Where(b => b.Matter.Contains(matter) && b.UserAuthId == userId)
                    .Skip((validPagination.Page - 1) * validPagination.Size)
                    .Take(validPagination.Size)
                    .ToList();

                var totalRecords = _context.Blogs.Where(b => b.Matter.Contains(matter) && b.UserAuthId == userId).Count();

                string baseUri = $"{Request.Scheme}://{Request.Host}{Request.Path.Value}";

                PageResponse<List<Blog>> pagedResponse = new(
                    data: blogs,
                    page: validPagination.Page,
                    size: validPagination.Size,
                    totalRecords: totalRecords,
                    uri: baseUri
                );

                return Ok(pagedResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response(
                        message: "An internal error occurred when searching for blogs.",
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
                    Name = blogDto.Name,
                    Matter = blogDto.Matter,
                    ColorPrimary = blogDto.ColorPrimary,
                    ColorSecondary = blogDto.ColorSecondary,
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

        /// <summary> Delete a blog </summary>
        /// <response code="204"> Not a content is returned </response>
        [Authorize]
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        public IActionResult DeleteBlog(long id)
        {
            try
            {
                var userId = Convert.ToInt64(User?.Identity?.Name);

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

        /// <summary> Get a public blog by id </summary>
        /// <response code="200"> Return a blog </response>
        [AllowAnonymous]
        [HttpGet("public/{id}")]
        public ActionResult<Response<Blog>> GetPublicBlogById(long id)
        {
            try
            {

                var blog = _context.Blogs.Where(b => b.Id == id && b.IsPublic).FirstOrDefault();
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
        public ActionResult<PageResponse<Blog>> GetPublicBlogs([FromQuery] Pagination pagination)
        {
            try
            {
                Pagination validPagination = new(pagination.Page, pagination.Size);

                var blogs = _context.Blogs
                    .Where(b => b.IsPublic)
                    .Skip((validPagination.Page - 1) * validPagination.Size)
                    .Take(validPagination.Size)
                    .ToList();

                var totalRecords = _context.Blogs.Where(b => b.IsPublic).Count();

                string baseUri = $"{Request.Scheme}://{Request.Host}{Request.Path.Value}";

                PageResponse<List<Blog>> pagedResponse = new(
                    data: blogs,
                    page: validPagination.Page,
                    size: validPagination.Size,
                    totalRecords: totalRecords,
                    uri: baseUri
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

        /// <summary> Find public blog by name </summary>
        /// <response code="200"> Return a blog page </response> 
        [AllowAnonymous]
        [HttpGet("public/find-by-name/{name}")]
        public ActionResult<PageResponse<Blog>> FindPublicBlogByName(string name, [FromQuery] Pagination pagination)
        {
            try
            {
                Pagination validPagination = new(pagination.Page, pagination.Size);

                var blogs = _context.Blogs
                    .Where(b => b.Name.Contains(name) && b.IsPublic)
                    .Skip((validPagination.Page - 1) * validPagination.Size)
                    .Take(validPagination.Size)
                    .ToList();

                var totalRecords = _context.Blogs.Where(b => b.Name.Contains(name) && b.IsPublic).Count();

                string baseUri = $"{Request.Scheme}://{Request.Host}{Request.Path.Value}";

                PageResponse<List<Blog>> pagedResponse = new(
                    data: blogs,
                    page: validPagination.Page,
                    size: validPagination.Size,
                    totalRecords: totalRecords,
                    uri: baseUri
                );

                return Ok(pagedResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response(
                        message: "An internal error occurred when searching for blog.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
        }

        /// <summary> Find public blog by matter </summary>
        /// <response code="200"> Return a blog page </response>
        [AllowAnonymous]
        [HttpGet("public/find-by-matter/{matter}")]
        public ActionResult<PageResponse<Blog>> FindPublicBlogByMatter(string matter, [FromQuery] Pagination pagination)
        {
            try
            {
                Pagination validPagination = new(pagination.Page, pagination.Size);

                var blogs = _context.Blogs
                    .Where(b => b.Matter.Contains(matter) && b.IsPublic)
                    .Skip((validPagination.Page - 1) * validPagination.Size)
                    .Take(validPagination.Size)
                    .ToList();

                var totalRecords = _context.Blogs.Where(b => b.Matter.Contains(matter) && b.IsPublic).Count();

                string baseUri = $"{Request.Scheme}://{Request.Host}{Request.Path.Value}";

                PageResponse<List<Blog>> pagedResponse = new(
                    data: blogs,
                    page: validPagination.Page,
                    size: validPagination.Size,
                    totalRecords: totalRecords,
                    uri: baseUri
                );

                return Ok(pagedResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response(
                        message: "An internal error occurred when searching for blogs.",
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
            var userId = Convert.ToInt64(User?.Identity?.Name);
            var blog = _context.Blogs.Where(b => b.Id == id && b.UserAuthId == userId).FirstOrDefault();
            if (blog == null)
            {
                throw new NotFoundException($"The logged in user does not have a blog where the id is {id}.");
            }

            if (!String.IsNullOrEmpty(blogDto.Name))
            {
                blog.Name = blogDto.Name;
            }
            if (!String.IsNullOrEmpty(blogDto.Matter))
            {
                blog.Matter = blogDto.Matter;
            }
            if (!String.IsNullOrEmpty(blogDto.ColorPrimary))
            {
                blog.ColorPrimary = blogDto.ColorPrimary;
            }
            if (!String.IsNullOrEmpty(blogDto.ColorSecondary))
            {
                blog.ColorSecondary = blogDto.ColorSecondary;
            }
            if (blogDto.IsPublic.HasValue)
            {
                blog.IsPublic = blogDto.IsPublic.Value;
            }

            _context.Blogs.Update(blog);
            _context.SaveChanges();

            return blog;
        }

    }
}