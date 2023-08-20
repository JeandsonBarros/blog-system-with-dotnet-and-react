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
                    if (userExists.IsConfirmedEmail)
                    {
                        return StatusCode(
                            StatusCodes.Status400BadRequest,
                            new Response(message: $"There is already a user with email {userAuthDto.Email}!", success: false)
                        );
                    }
                    else
                    {
                        if (!userExists.FileProfilePictureName.IsNullOrEmpty())
                        {
                            _filesService.DeleteFile(userExists.FileProfilePictureName);
                        }

                        _context.Users.Remove(userExists);
                        _context.SaveChanges();
                    }
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

                Random random = new Random();
                long code = random.Next(1000000, 2000000);

                AuthorizationCode authorizationCode = new()
                {
                    Code = code,
                    UserAuthId = userAuth.Id
                };

                _context.AuthorizationCodes.Add(authorizationCode);
                _context.SaveChanges();

                var (isSentEmail, messageSendEmail) = await SendEmailAsync(
                    email: userAuth.Email,
                    subject: "Email confirmation code - Blog",
                    body: $"Your email confirmation code is {code}, valid for 15 minutes!"
                );

                if (isSentEmail)
                {
                    string uri = $"{Request.Scheme}://{Request.Host}/api/auth/confirm-email";
                    return Created(
                        nameof(ConfirmEmail),
                        new Response<UserAuth>(
                            data: userAuth,
                            message: $"Code for email confirmation has been sent to {userAuth.Email}.",
                            success: true,
                            details: $"Use code in {uri}"
                        )
                    );
                }

                _context.AuthorizationCodes.Remove(authorizationCode);
                _context.SaveChanges();

                return StatusCode(
                   StatusCodes.Status400BadRequest,
                   new Response(
                       message: "Error sending email with code to confirm email!",
                       success: false,
                       details: messageSendEmail
                    )
                );
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response(
                        message: "Error confirming email!",
                        success: false,
                        details: ex.Message
                    )
                );
            }
        }

        /// <summary> Confirm user email </summary>
        /// <response code="200"> Returns the user updated </response>
        /// <response code="404"> If there is no account with the email provided </response>
        [AllowAnonymous]
        [HttpPut("confirm-email")]
        public ActionResult<Response<UserAuth>> ConfirmEmail(AuthorizationCodeDto authorizationCodeDto)
        {
            try
            {
                var user = _context.Users.Where(user => user.Email == authorizationCodeDto.Email).FirstOrDefault();
                if (user == null)
                {
                    return NotFound(new Response(message: $"Email user {authorizationCodeDto.Email} not found!", success: false));
                }

                var (isValidCode, messageIsValidade) = IsValidAuthorizationCode(user.Id, authorizationCodeDto.Code);
                if (isValidCode)
                {
                    user.IsConfirmedEmail = true;
                    _context.Users.Update(user);
                    _context.SaveChanges();
                    return Ok(new Response<UserAuth>(data: user, message: "The email has been confirmed.", success: true));
                }
                else
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new Response(message: messageIsValidade, success: false));
                }

            }
            catch (System.Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response(
                        message: "Error confirming email!",
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
                    return NotFound(new Response(message: "User not found!", success: false));
                }

                if (ToHash(loginDto.Password) != user.Password)
                {
                    return StatusCode(
                        StatusCodes.Status401Unauthorized,
                        new Response(message: "Invalid password!", success: false)
                    );
                }

                if (!user.IsConfirmedEmail)
                {
                    return StatusCode(
                        StatusCodes.Status401Unauthorized,
                        new Response(
                            message: "You need to confirm your email to login!",
                            success: false,
                            details: "To confirm the email, you must inform the endpoint /api/auth/confirm-email the code that was sent to the email."
                        )
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
                        message: "Login error!",
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
                var id = Convert.ToInt64(User?.Identity?.Name);
                var user = _context.Users.Find(id);
                if (user == null)
                {
                    return NotFound(new Response(message: "User not found!", success: false));
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
                        message: "Error geting account data!",
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
                    return NotFound(new Response(message: "There is not even a user with the given image!", success: false));
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
                var id = Convert.ToInt64(User?.Identity?.Name);
                UserDtoNoValidation userDtoNoValidation = new()
                {
                    Name = userDto.Name,
                    Email = userDto.Email,
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
                        message: "There was an internal server error when updating your account.",
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
                var id = Convert.ToInt64(User?.Identity?.Name);
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
                        message: "There was an internal server error when updating your account.",
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
                var id = Convert.ToInt64(User?.Identity?.Name);
                var user = _context.Users.Find(id);
                if (user == null)
                {
                    return NotFound(new Response(message: "User not found!", success: false));
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
                        message: "There was an internal server error when deleting the account.",
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
                    return NotFound(new Response(message: $"Email user {emailToDto.Email} not found!", success: false));
                }

                Random random = new Random();
                long code = random.Next(1000000, 2000000);

                AuthorizationCode authorizationCode = new()
                {
                    Code = code,
                    UserAuthId = user.Id
                };

                _context.AuthorizationCodes.Add(authorizationCode);
                _context.SaveChanges();

                var (isSent, message) = await SendEmailAsync(
                    email: emailToDto.Email,
                    subject: "BlogHub - Your code reset forgotten password",
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
                        message: "There was an internal server error when sending email.",
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
        public ActionResult<Response<UserAuth>> ChangeFogottenPassword(ChangeForgottenPasswordDto changeForgottenPasswordDto)
        {
            try
            {
                var user = _context.Users.Where(user => user.Email == changeForgottenPasswordDto.Email).FirstOrDefault();
                if (user == null)
                {
                    return NotFound(new Response(message: $"Email user {changeForgottenPasswordDto.Email} not found!", success: false));
                }

                var (isValidCode, message) = IsValidAuthorizationCode(user.Id, changeForgottenPasswordDto.Code);

                if (isValidCode)
                {
                    user.Password = ToHash(changeForgottenPasswordDto.NewPassword);
                    _context.Users.Update(user);
                    _context.SaveChanges();
                    return Ok(new Response<UserAuth>(data: user, message: "Password changed successfully.", success: true));
                }
                else
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new Response(message: message, success: false));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response(
                        message: "There was an internal server error when updating password.",
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
        [HttpGet("list-all-users")]
        public ActionResult<PageResponse<List<UserAuth>>> GetAllUsers([FromQuery] Pagination pagination)
        {
            try
            {
                var validPagination = new Pagination(pagination.Page, pagination.Size);

                var users = _context.Users
                    .Skip((validPagination.Page - 1) * validPagination.Size)
                    .Take(validPagination.Size)
                    .ToList();

                var totalRecords = _context.Users.Count();

                string baseUri = $"{Request.Scheme}://{Request.Host}{Request.Path.Value}";

                PageResponse<List<UserAuth>> pagedResponse = new(
                    data: users,
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
                        message: "There was an internal server error when geting user.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
        }

        /// <summary> Admin: Find user by email </summary>
        /// <response code="200"> Return a page of users </response>
        /// <response code="401"> if unauthenticated </response>
        /// <response code="403"> if non-admin  </response>
        [Authorize(Roles = "ADMIN")]
        [HttpGet("find-by-email/{email}")]
        public ActionResult<PageResponse<List<UserAuth>>> FindUserByEmail([FromQuery] Pagination pagination, string email)
        {
            try
            {
                var validPagination = new Pagination(pagination.Page, pagination.Size);

                var users = _context.Users
                    .Where(user => user.Email.Contains(email))
                    .Skip((validPagination.Page - 1) * validPagination.Size)
                    .Take(validPagination.Size)
                    .ToList();

                var totalRecords = _context.Users.Where(user => user.Email.Contains(email)).Count();

                string baseUri = $"{Request.Scheme}://{Request.Host}{Request.Path.Value}";

                PageResponse<List<UserAuth>> pagedResponse = new(
                    data: users,
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
                       message: "There was an internal server error when finding user.",
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
                        new Response(message: "An admin cannot delete another admin!", success: false)
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
                        message: "There was an internal server error when deleting user.",
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
                    return NotFound(new Response(message: "User not found", success: false));
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
                        message: "Error updating user data, check the data entered is correct!",
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
                        message: "There was an internal server error when updating user.",
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
                        message: "There was an internal server error when updating user.",
                        success: false,
                        details: ex.Message
                    )
                );
            }
        }

        /* Function to update user, either by Put or Patch */
        private async Task<UserAuth> UpdateUser(long userAuthId, UserDtoNoValidation userDto)
        {
            var userAuth = _context.Users.Where(u => u.Id == userAuthId).FirstOrDefault();

            if (!userDto.Name.IsNullOrEmpty())
            {
                userAuth.Name = userDto.Name;
            }

            if (!userDto.Email.IsNullOrEmpty())
            {
                string strModel = "^([0-9a-zA-Z]([-.\\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\\w]*[0-9a-zA-Z]\\.)+[a-zA-Z]{2,9})$";
                if (!System.Text.RegularExpressions.Regex.IsMatch(userDto.Email, strModel))
                {
                    throw new BadHttpRequestException("Email must be well-formed");
                }

                var userExists = _context.Users.Where(user => user.Email == userDto.Email).FirstOrDefault();
                if (userExists is not null && userExists.Email != userAuth.Email)
                {
                    throw new BadHttpRequestException($"User with {userDto.Email} already exists!");
                }

                userAuth.Email = userDto.Email;
            }

            if (!userDto.Password.IsNullOrEmpty())
            {
                if (userDto.Password.Length < 6)
                {
                    throw new BadHttpRequestException("The password must have at least 6 characters");
                }

                userAuth.Password = ToHash(userDto.Password);
            }

            if (userDto.FileProfilePicture != null)
            {
                if (!userAuth.FileProfilePictureName.IsNullOrEmpty())
                {
                    _filesService.DeleteFile(userAuth.FileProfilePictureName);
                }

                userAuth.FileProfilePictureName = await _filesService.SaveFile(userDto.FileProfilePicture);
            }

            _context.Users.Update(userAuth);
            _context.SaveChanges();

            return userAuth;
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

        /* Verifies that an authorization code is valid.
        An authorization code is used to confirm email or change forgotten password. */
        private (bool IsValidade, string Message) IsValidAuthorizationCode(long userId, long code)
        {
            /* Checks if the entered code exists and belongs to the entered email user */
            var authorizationCode = _context.AuthorizationCodes.Where(codeEntity =>
                codeEntity.Code == code
                && codeEntity.UserAuthId == userId).FirstOrDefault();
            if (authorizationCode == null)
            {
                return (false, "Incorrect authorization code.");
            }

            /* Checks that the code has not expired */
            if (DateTime.UtcNow > authorizationCode.CodeExpires)
            {
                _context.AuthorizationCodes.Remove(authorizationCode);
                _context.SaveChanges();

                return (false, "Code expired.");
            }

            return (true, "Code is valid.");
        }
    }
}