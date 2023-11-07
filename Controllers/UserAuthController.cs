using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using BlogAPI.Context;
using BlogAPI.DTOs;
using BlogAPI.Models;
using System.Net.Mail;
using System.Net;
using BlogAPI.Services;
using BlogAPI.CustomExceptions;

namespace BlogAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class UserAuthController : ControllerBase
    {
        private readonly BlogContext _context;
        private readonly IConfiguration _configuration;
        private readonly FilesService _filesService;

        public UserAuthController(BlogContext context, IConfiguration configuration, FilesService filesService)
        {
            _context = context;
            _configuration = configuration;
            _filesService = filesService;
        }

        /// <summary> User register </summary>
        /// <response code="201"> Returns the user created </response>
        /// <response code="400"> If any field is missing, invalid or there is already a user with the registered email </response>
        [AllowAnonymous]
        [HttpPost("register")]
        [ProducesResponseType(201)]
        public async Task<ActionResult<Response<UserAuth>>> Register([FromForm] UserAuthDto userAuthDto)
        {
            try
            {
                var userExists = _context.Users.Where(user => user.Email == userAuthDto.Email).FirstOrDefault();
                if (userExists != null)
                {
                    return StatusCode(
                        StatusCodes.Status400BadRequest,
                        new Response(message: $"There is already a user with email address {userAuthDto.Email}.", success: false)
                    );
                }

                var role = _context.Roles.Where(role => role.RoleName == "USER").FirstOrDefault();
                if (role == null)
                {
                    role = new() { RoleName = "USER" };
                    _context.Roles.Add(role);
                    _context.SaveChanges();
                }

                string password = ToHash(userAuthDto.Password);

                UserAuth userAuth = new()
                {
                    Email = userAuthDto.Email,
                    Name = userAuthDto.Name,
                    Password = password
                };

                userAuth.Roles.Add(role);

                if (userAuthDto.FileProfilePicture != null)
                {
                    userAuth.FileProfilePictureName = await _filesService.SaveFile(userAuthDto.FileProfilePicture);
                }

                _context.Users.Add(userAuth);
                _context.SaveChanges();

                return CreatedAtAction(
                    nameof(Login),
                    new Response<UserAuth>(
                        data: userAuth,
                        message: "Registration completed successfully.",
                        success: true
                    ));

            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response(
                        message: "An internal error occurred while registering.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
        }

        /// <summary> User login </summary>
        /// <response code="200"> Return a token </response>
        /// <response code="401"> If the credentials are wrong </response>
        /// <response code="404"> If user not exists </response>
        [AllowAnonymous]
        [HttpPost("login")]
        public ActionResult<Response<string>> Login(LoginDto loginDto)
        {
            try
            {
                var user = _context.Users.Where(user => user.Email == loginDto.Email).FirstOrDefault();
                if (user == null)
                {
                    return NotFound(new Response(message: "User not found.", success: false));
                }

                if (ToHash(loginDto.Password) != user.Password)
                {
                    return StatusCode(
                        StatusCodes.Status401Unauthorized,
                        new Response(message: "Invalid password.", success: false)
                    );
                }

                return Ok(
                    new Response<string>(
                        data: $"Bearer {GetToken(user)}",
                        message: "Login was successful. Use the token in the 'Authorization: Bearer token' header.",
                        success: true
                    )
                );
            }
            catch (System.Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response(
                        message: "An internal error occurred while logging in.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
        }

        /// <summary> Get account data that is authenticated </summary>
        /// <response code="200"> Return account data </response>
        /// <response code="401"> If not authenticated </response>
        [Authorize]
        [HttpGet("account-data")]
        public ActionResult<Response<UserAuth>> GetAccountData()
        {
            try
            {
                var id = Convert.ToInt64(User.Identity?.Name);
                var user = _context.Users.Find(id);
                if (user == null)
                {
                    return NotFound(new Response(message: "User not found.", success: false));
                }

                var roles = _context.Roles.Where(x => x.Users.Any(user => user.Id == id));
                user.Roles = roles.ToList();

                return Ok(new Response<UserAuth>(data: user));
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response(
                        message: "An internal error occurred while retrieving account data.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
        }

        /// <summary> Get user profile picture </summary>
        /// <response code="200"> Return a file </response>
        [AllowAnonymous]
        [HttpGet("profile-picture/{fileName}")]
        public IActionResult GetUserProfileImage(string fileName)
        {
            try
            {
                var user = _context.Users.Where(u => u.FileProfilePictureName == fileName).FirstOrDefault();
                if (user == null)
                {
                    return NotFound(new Response(message: "There is not even a user with the given image.", success: false));
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
                         message: "An internal server error occurred while getting the file.",
                         success: false,
                         details: ex.Message
                     )
                 );
            }
        }

        /// <summary> Fully update user account data </summary>
        /// <response code="200"> Returns updated account data </response>
        /// <response code="401"> If not authenticated </response>
        [Authorize]
        [HttpPut("update-account")]
        public async Task<ActionResult<Response<UserAuth>>> PutLoggedUser([FromForm] UserAuthDto userDto)
        {
            try
            {
                var id = Convert.ToInt64(User.Identity?.Name);
                UserDtoNoValidation userDtoNoValidation = new()
                {
                    Name = userDto.Name,
                    Password = userDto.Password,
                    FileProfilePicture = userDto.FileProfilePicture
                };

                return Ok(new Response<UserAuth>(
                    data: await UpdateUser(id, userDtoNoValidation),
                    message: "User updated successfully.",
                    success: true
                ));
            }
            catch (BadHttpRequestException ex)
            {
                return StatusCode(
                    StatusCodes.Status400BadRequest,
                    new Response(
                        message: "Error updating account data, check the data entered is correct.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
            catch (System.Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response(
                        message: "An internal server error occurred while updating your account.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
        }

        /// <summary> Partially update user account data </summary>
        /// <response code="200"> Returns updated account data </response>
        /// <response code="401"> If not authenticated </response>
        [Authorize]
        [HttpPatch("update-account")]
        public async Task<ActionResult<Response<UserAuth>>> PatchLoggedUser([FromForm] UserDtoNoValidation userDtoNoValidation)
        {
            try
            {
                var id = Convert.ToInt64(User.Identity?.Name);
                userDtoNoValidation.Email = null;
                return Ok(new Response<UserAuth>(
                    data: await UpdateUser(id, userDtoNoValidation),
                    message: "User updated successfully.",
                    success: true
                ));
            }
            catch (BadHttpRequestException ex)
            {
                return StatusCode(
                    StatusCodes.Status400BadRequest,
                    new Response(
                        message: "Error updating account data, check the data entered is correct!",
                        success: false,
                        details: ex.Message
                    )
                );
            }
            catch (System.Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response(
                        message: "An internal server error occurred while updating your account.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
        }

        /// <summary> Authenticated user deletes their profile photo </summary>
        /// <response code="204"> If profile photo deleted success </response>
        /// <response code="401"> if unauthenticated </response>
        [Authorize]
        [HttpDelete("delete-profile-image")]
        [ProducesResponseType(204)]
        public IActionResult DeleteProfilePhoto()
        {
            try
            {
                var id = Convert.ToInt64(User.Identity?.Name);
                var user = _context.Users.Find(id);
                if (user == null)
                {
                    return NotFound(new Response(message: "User not found.", success: false));
                }

                if (String.IsNullOrEmpty(user.FileProfilePictureName))
                {
                    return NotFound(new Response(
                          message: "There is no photo to remove.",
                          success: false
                    ));
                }

                _filesService.DeleteFile(user.FileProfilePictureName);

                user.FileProfilePictureName = null;
                _context.Users.Update(user);
                _context.SaveChanges();

                return StatusCode(StatusCodes.Status204NoContent);
            }
            catch (System.Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response(
                        message: "An internal server error occurred while deleting the profile photo.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
        }

        /// <summary> Authenticated user delete own account </summary>
        /// <response code="204"> If account deleted success </response>
        /// <response code="401"> if unauthenticated </response>
        [Authorize]
        [HttpDelete("delete-account")]
        [ProducesResponseType(204)]
        public IActionResult DeleteAccount()
        {
            try
            {
                var id = Convert.ToInt64(User.Identity?.Name);
                var user = _context.Users.Find(id);
                if (user == null)
                {
                    return NotFound(new Response(message: "User not found.", success: false));
                }

                if (!String.IsNullOrEmpty(user.FileProfilePictureName))
                {
                    _filesService.DeleteFile(user.FileProfilePictureName);
                }

                _context.Users.Remove(user);
                _context.SaveChanges();

                return StatusCode(StatusCodes.Status204NoContent);
            }
            catch (System.Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response(
                        message: "An internal server error occurred while deleting the account.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
        }

        /// <summary> Send forgotten password reset code to email </summary>
        /// <response code="200"> Returns message notifying that the email was sent </response>
        /// <response code="404"> If there is no user with the entered email </response>
        [AllowAnonymous]
        [HttpPost("forgotten-password/send-email-code")]
        public async Task<ActionResult<Response>> SendEmailCodeToForgottenPassword(EmailToDto emailToDto)
        {
            try
            {
                var user = _context.Users.Where(user => user.Email == emailToDto.Email).FirstOrDefault();
                if (user == null)
                {
                    return NotFound(new Response(message: $"Email user {emailToDto.Email} not found.", success: false));
                }

                Random random = new Random();
                long code = random.Next(1000000, 2000000);

                AuthorizationCode authorizationCode = new()
                {
                    Code = code,
                    Email = user.Email
                };

                _context.AuthorizationCodes.Add(authorizationCode);
                _context.SaveChanges();

                var (isSent, message) = await SendEmailAsync(
                    email: emailToDto.Email,
                    subject: "Blogs - Your code reset forgotten password",
                    body: $"Your password reset code is {code}, valid for 15 minutes."
                );

                if (isSent)
                {
                    string uri = $"{Request.Scheme}://{Request.Host}/api/auth/forgotten-password/change-password";
                    return Ok(
                        new Response(
                            message: $"Code send to {emailToDto.Email}",
                            details: $"Use code in {uri}",
                            success: true
                        )
                    );
                }

                _context.AuthorizationCodes.Remove(authorizationCode);
                _context.SaveChanges();

                return StatusCode(
                   StatusCodes.Status400BadRequest,
                   new Response(
                        message: "Error sending email.",
                        success: false,
                        details: message
                    )
                );
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response(
                        message: "An internal server error occurred while sending email.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
        }

        /// <summary> Reset password using code sent to email </summary>
        /// <response code="200"> Returns message notifying that the password was successfully reset </response>
        /// <response code="404"> If the entered code is incorrect or does not exist, or if there is no user with the entered email </response>
        [AllowAnonymous]
        [HttpPut("forgotten-password/change-password")]
        public ActionResult<Response<UserAuth>> ChangeFogottenPassword(DataToUpdateForgottenPassword dataToUpdateForgottenPassword)
        {
            try
            {
                var user = _context.Users.Where(user => user.Email == dataToUpdateForgottenPassword.Email).FirstOrDefault();
                if (user == null)
                {
                    return NotFound(new Response(message: $"Email user {dataToUpdateForgottenPassword.Email} not found.", success: false));
                }

                var authorizationCode = _context.AuthorizationCodes.Where(codeEntity =>
                    codeEntity.Code == dataToUpdateForgottenPassword.Code
                    && codeEntity.Email == dataToUpdateForgottenPassword.Email
                ).FirstOrDefault();

                /* Checks if the entered code exists and belongs to the entered email user */
                if (authorizationCode == null)
                {
                    return StatusCode(
                        StatusCodes.Status400BadRequest,
                        new Response(message: "Incorrect authorization code.", success: false
                    ));
                }

                /* Checks that the code has not expired */
                if (DateTime.UtcNow > authorizationCode.CodeExpires)
                {
                    _context.AuthorizationCodes.Remove(authorizationCode);
                    _context.SaveChanges();

                    return StatusCode(
                        StatusCodes.Status400BadRequest,
                        new Response(message: "Code expired.", success: false
                    ));
                }

                user.Password = ToHash(dataToUpdateForgottenPassword.NewPassword);
                _context.Users.Update(user);
                _context.SaveChanges();

                _context.AuthorizationCodes.Remove(authorizationCode);
                _context.SaveChanges();

                return Ok(new Response<UserAuth>(data: user, message: "Password changed successfully.", success: true));

            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response(
                        message: "An internal server error occurred while updating the password.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
        }

        /// <summary> Admin: Get all users </summary>
        /// <response code="200"> Return a page of users </response>
        /// <response code="401"> if unauthenticated </response>
        /// <response code="403"> if non-admin  </response>
        [Authorize(Roles = "ADMIN")]
        [HttpGet("list-users")]
        public ActionResult<PageResponse<List<UserAuth>>> GetUsers([FromQuery] Pagination pagination, [FromQuery] string? search)
        {
            try
            {
                var userId = Convert.ToInt64(User.Identity?.Name);
                var validPagination = new Pagination(pagination.Page, pagination.Size);
                var users = new List<UserAuth>();
                var totalRecords = 0;

                if (String.IsNullOrEmpty(search))
                {
                    users = _context.Users
                        .Where(u => u.Id != userId)
                        .Skip((validPagination.Page - 1) * validPagination.Size)
                        .Take(validPagination.Size)
                        .ToList();

                    totalRecords = _context.Users.Where(u => u.Id != userId).Count();
                }
                else
                {
                    users = _context.Users
                        .Where(u => u.Id != userId && u.Email.Contains(search))
                        .Skip((validPagination.Page - 1) * validPagination.Size)
                        .Take(validPagination.Size)
                        .ToList();

                    totalRecords = _context.Users.Where(u => u.Id != userId && u.Email.Contains(search)).Count();
                }

                PageResponse<List<UserAuth>> pagedResponse = new(
                    data: users,
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
                        message: "An internal server error occurred while getting the user.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
        }

        /// <summary> Admin: Delete a user's profile photo</summary>
        /// <response code="204"> If profile photo deleted success </response>
        /// <response code="401"> if unauthenticated </response>
        [Authorize(Roles = "ADMIN")]
        [HttpDelete("delete-a-user-photo/{userId}")]
        [ProducesResponseType(204)]
        public IActionResult DeleteAUserProfilePhoto(long userId)
        {
            try
            {
                var user = _context.Users.Find(userId);
                if (user == null)
                {
                    return NotFound(new Response(message: "User not found.", success: false));
                }

                bool isAdmin = user.Roles.Any(role => role.RoleName == "ADMIN");
                if (isAdmin)
                {
                    return StatusCode(
                        StatusCodes.Status403Forbidden,
                        new Response(message: "An admin cannot delete profile photo another admin.", success: false)
                    );
                }

                if (String.IsNullOrEmpty(user.FileProfilePictureName))
                {
                    return NotFound(new Response(
                          message: "There is no photo to remove.",
                          success: false
                    ));
                }

                _filesService.DeleteFile(user.FileProfilePictureName);

                user.FileProfilePictureName = null;
                _context.Users.Update(user);
                _context.SaveChanges();

                return StatusCode(StatusCodes.Status204NoContent);
            }
            catch (System.Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response(
                        message: "An internal server error occurred while deleting the profile photo.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
        }

        /// <summary> Admin: Delete a user </summary>
        /// <response code="204"> If user deleted success </response>
        /// <response code="401"> if unauthenticated </response>
        /// <response code="403"> if non-admin  </response>
        [Authorize(Roles = "ADMIN")]
        [HttpDelete("delete-a-user/{userId}")]
        [ProducesResponseType(204)]
        public IActionResult DeleteAUser(long userId)
        {
            try
            {
                var user = _context.Users.Find(userId);
                if (user == null)
                {
                    return NotFound(new Response(message: "User not found.", success: false));
                }

                bool isAdmin = user.Roles.Any(role => role.RoleName == "ADMIN");
                if (isAdmin)
                {
                    return StatusCode(
                        StatusCodes.Status403Forbidden,
                        new Response(message: "An admin cannot delete another admin.", success: false)
                    );
                }

                if (!String.IsNullOrEmpty(user.FileProfilePictureName))
                {
                    _filesService.DeleteFile(user.FileProfilePictureName);
                }

                _context.Users.Remove(user);
                _context.SaveChanges();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response(
                        message: "An internal server error occurred while deleting the user.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
        }

        /// <summary> Admin: Fully updates a user's data </summary>
        /// <response code="200"> Returns updated user data </response>
        /// <response code="401"> if unauthenticated </response>
        /// <response code="403"> if non-admin  </response>
        [Authorize(Roles = "ADMIN")]
        [HttpPut("update-a-user/{userId}")]
        public async Task<ActionResult<Response<UserAuth>>> PutUpdateAUser(long userId, [FromForm] UserAuthDto userDto)
        {
            try
            {
                var user = _context.Users.Find(userId);
                if (user == null)
                {
                    return NotFound(new Response(message: "User not found.", success: false));
                }

                bool isAdmin = user.Roles.Any(role => role.RoleName == "ADMIN");
                if (isAdmin)
                {
                    return StatusCode(
                        StatusCodes.Status403Forbidden,
                        new Response(message: "An admin cannot update another admin.", success: false)
                    );
                }

                UserDtoNoValidation userDtoNotValidate = new()
                {
                    Email = userDto.Email,
                    Name = userDto.Name,
                    Password = userDto.Password,
                    FileProfilePicture = userDto.FileProfilePicture
                };

                return Ok(
                     new Response<UserAuth>(
                         data: await UpdateUser(user.Id, userDtoNotValidate),
                         message: "User updated successfully.",
                         success: true
                     )
                 );
            }
            catch (BadHttpRequestException ex)
            {
                return StatusCode(
                    StatusCodes.Status400BadRequest,
                    new Response(
                        message: "Error updating user data, check the data entered is correct.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
            catch (System.Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response(
                        message: "An internal server error occurred while updating the user.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
        }

        /// <summary> Admin: Partially updates a user's data </summary>
        /// <response code="200"> Returns updated user data </response>
        /// <response code="401"> if unauthenticated </response>
        /// <response code="403"> if non-admin  </response>
        [Authorize(Roles = "ADMIN")]
        [HttpPatch("update-a-user/{userId}")]
        public async Task<ActionResult<Response<UserAuth>>> PatchUpdateAUser(long userId, [FromForm] UserDtoNoValidation userDtoNotValidate)
        {
            try
            {
                var user = _context.Users.Find(userId);
                if (user == null)
                {
                    return NotFound(new Response(message: "User not found.", success: false));
                }

                bool isAdmin = user.Roles.Any(role => role.RoleName == "ADMIN");
                if (isAdmin)
                {
                    return StatusCode(
                        StatusCodes.Status403Forbidden,
                        new Response(message: "An admin cannot update another admin.", success: false)
                    );
                }

                return Ok(
                     new Response<UserAuth>(
                         data: await UpdateUser(user.Id, userDtoNotValidate),
                         message: "User updated successfully.",
                         success: true
                     )
                );
            }
            catch (BadHttpRequestException ex)
            {
                return StatusCode(
                    StatusCodes.Status400BadRequest,
                    new Response(
                        message: "Error updating user data, check the data entered is correct.",
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
                        message: "An internal server error occurred while updating the user.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
        }

        /* Function to update user, either by Put or Patch */
        private async Task<UserAuth> UpdateUser(long userId, UserDtoNoValidation userDto)
        {
            var user = _context.Users.Find(userId);
            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }

            if (!userDto.Name.IsNullOrEmpty())
            {
                user.Name = userDto.Name;
            }

            if (!userDto.Email.IsNullOrEmpty())
            {
                string strModel = "^([0-9a-zA-Z]([-.\\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\\w]*[0-9a-zA-Z]\\.)+[a-zA-Z]{2,9})$";
                if (!System.Text.RegularExpressions.Regex.IsMatch(userDto.Email, strModel))
                {
                    throw new BadHttpRequestException("Email must be well-formed");
                }

                var userExists = _context.Users.Where(user => user.Email == userDto.Email).FirstOrDefault();
                if (userExists is not null && userExists.Email != user.Email)
                {
                    throw new BadHttpRequestException($"User with {userDto.Email} already exists!");
                }

                user.Email = userDto.Email;
            }

            if (!userDto.Password.IsNullOrEmpty())
            {
                if (userDto.Password.Length < 6)
                {
                    throw new BadHttpRequestException("The password must have at least 6 characters");
                }

                user.Password = ToHash(userDto.Password);
            }

            if (userDto.FileProfilePicture != null)
            {
                if (!user.FileProfilePictureName.IsNullOrEmpty())
                {
                    _filesService.DeleteFile(user.FileProfilePictureName);
                }

                user.FileProfilePictureName = await _filesService.SaveFile(userDto.FileProfilePicture);
            }

            _context.Users.Update(user);
            _context.SaveChanges();

            return user;
        }

        /* Function to send email */
        private async Task<(bool Success, string Message)> SendEmailAsync(string email, string subject, string body)
        {
            try
            {
                var client = new SmtpClient(_configuration["Email:Host"], Convert.ToInt32(_configuration["Email:Port"]))
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_configuration["Email:EmailFrom"], _configuration["Email:PasswordFrom"])
                };

                await client.SendMailAsync(
                     new MailMessage(
                         from: _configuration["Email:EmailFrom"],
                         to: email,
                         subject: subject,
                         body: body
                     ));

                return (true, "Email successfully sent.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /* Convert text to hash */
        private string ToHash(string text)
        {
            MD5 md5 = new MD5CryptoServiceProvider();

            //compute hash from the bytes of text  
            md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(text));

            //get hash result after compute it  
            byte[] result = md5.Hash;

            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
            {
                //change it into 2 hexadecimal digits  
                //for each byte  
                strBuilder.Append(result[i].ToString("x2"));
            }

            return strBuilder.ToString();
        }

        /* Generate token jwt */
        private string GetToken(UserAuth user)
        {

            var authClaims = new List<Claim>
            {
                new (ClaimTypes.Name, user.Id.ToString()),
                new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var roles = _context.Roles.Where(x => x.Users.Any(userRole => userRole.Id == user.Id));
            foreach (var role in roles)
            {
                authClaims.Add(new(ClaimTypes.Role, role.RoleName));
            }

            var authSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            var token = new JwtSecurityToken(
                expires: DateTime.UtcNow.AddDays(30),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}