using Helpdesk.Common;
using Helpdesk.Common.DTOs;
using Helpdesk.Common.Requests.Users;
using Helpdesk.Common.Responses;
using Helpdesk.Common.Responses.Users;
using Helpdesk.DataLayer;
using NLog;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Helpdesk.Common.Extensions;
using Helpdesk.DataLayer.Contracts;
using Helpdesk.Data.Models;

namespace Helpdesk.Services
{
    /// <summary>
    /// This class is used to handle the business logic of users
    /// </summary>
    public class UsersFacade : ILoginClass
    {
        private static Logger s_logger = LogManager.GetCurrentClassLogger();

        private readonly AppSettings _appSettings;

        private IUsersDataLayer _usersDataLayer;

        public UsersFacade(IUsersDataLayer usersDataLayer)
        {
            _appSettings = new AppSettings();
            _usersDataLayer = usersDataLayer;
        }

        /// <summary>
        /// This method is responsible for retrieving all users from the helpdesk system
        /// </summary>
        /// <returns>The response that indicates if the operation was a success,
        /// and the list of users</returns>
        public GetUsersResponse GetUsers()
        {
            s_logger.Info("Getting users...");

            GetUsersResponse response = new GetUsersResponse();

            try
            {
                var users = _usersDataLayer.GetUsers();

                if (users.Count > 0)
                {
                    response.Status = HttpStatusCode.OK;
                    foreach (User user in users)
                    {
                        response.Users.Add(DAO2DTO(user));
                    }
                }
                else
                {
                    response.Status = HttpStatusCode.NotFound;
                    response.StatusMessages.Add(new StatusMessage(HttpStatusCode.NotFound, "No users found."));
                }
            }
            catch (Exception ex)
            {
                s_logger.Error(ex, "Unable to get users!");
                response.Status = HttpStatusCode.InternalServerError;
                response.StatusMessages.Add(new StatusMessage(HttpStatusCode.InternalServerError, "Unable to get users!"));
            }
            return response;
        }

        /// <summary>
        /// This method is responsible for getting a specific user from the helpdesk system
        /// </summary>
        /// <param name="id">The UserId of the specific user to be retrieved</param>
        /// <returns>The response that indicates if the operation was a success,
        /// and the details of the retrieved user if it was</returns>
        public GetUserResponse GetUser(int id)
        {
            s_logger.Info("Getting user...");

            GetUserResponse response = new GetUserResponse();

            try
            {
                var user = _usersDataLayer.GetUser(id);

                if (user == null)
                {
                    response.Status = HttpStatusCode.NotFound;
                    response.StatusMessages.Add(new StatusMessage(HttpStatusCode.NotFound, "Unable to find user."));
                }
                else
                {
                    response.Status = HttpStatusCode.OK;
                    response.User = DAO2DTO(user);
                }
            }
            catch (Exception ex)
            {
                s_logger.Error(ex, "Unable to get user!");
                response.Status = HttpStatusCode.InternalServerError;
                response.StatusMessages.Add(new StatusMessage(HttpStatusCode.InternalServerError, "Unable to get user!"));
            }
            return response;
        }

        /// <summary>
        /// This method is responsible for adding a new user.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public AddUserResponse AddUser(AddUserRequest request)
        {
            s_logger.Info("Adding user...");

            AddUserResponse response = new AddUserResponse();

            try
            {
                response = (AddUserResponse)request.CheckValidation(response);
                if (response.Status == HttpStatusCode.BadRequest)
                {
                    return response;
                }

                if (string.IsNullOrEmpty(request.Password))
                    request.Password = request.Username;

                request.Password = HashText(request.Password);

                if (_usersDataLayer.GetUserByUsername(request.Username) != null)
                {
                    response.Status = HttpStatusCode.Forbidden;
                    response.StatusMessages.Add(new StatusMessage(HttpStatusCode.Forbidden, "Username already exists"));
                    return response;
                }

                User user = new User()
                {
                    FirstTime = true,
                    Password = request.Password,
                    Username = request.Username
                };

                _usersDataLayer.AddUser(user);
                _usersDataLayer.Save();

                response.UserId = user.UserId;
                response.Status = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                s_logger.Error(ex, "Unable to add user!");
                response.Status = HttpStatusCode.InternalServerError;
                response.StatusMessages.Add(new StatusMessage(HttpStatusCode.InternalServerError, "Unable to add user!"));
            }
            return response;
        }

        /// <summary>
        /// This method is responsible for updating a specific user's information, such
        /// as their username and password
        /// </summary>
        /// <param name="id">The UserId of the user to be updated</param>
        /// <param name="request">The user's new information</param>
        /// <returns>The response that indicates if the update was successfull</returns>
        public UpdateUserResponse UpdateUser(UpdateUserRequest request)
        {
            s_logger.Info("Updating user...");

            UpdateUserResponse response = new UpdateUserResponse();

            try
            {
                response = (UpdateUserResponse)request.CheckValidation(response);

                if (response.Status == HttpStatusCode.BadRequest)
                    return response;

                request.Password = HashText(request.Password);

                if (_usersDataLayer.GetUserByUsername(request.Username) != null && _usersDataLayer.GetUserByUsername(request.Username).UserId != request.UserID)
                {
                    response.Status = HttpStatusCode.Forbidden;
                    response.StatusMessages.Add(new StatusMessage(HttpStatusCode.BadRequest, "Unable to update user! User with username " + request.Username + "already exists!"));
                    return response;
                }

                var user = _usersDataLayer.GetUser(request.UserID);

                user.FirstTime = false;
                user.Password = request.Password;
                user.Username = request.Username;

                _usersDataLayer.Save();

                response.Status = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                s_logger.Error(ex, "Unable to update user!");
                response.Status = HttpStatusCode.Forbidden;
                response.StatusMessages.Add(new StatusMessage(HttpStatusCode.Forbidden, "Unable to update user!"));
            }
            return response;
        }

        /// <summary>
        /// This method is responsible for handling the deletion of a user from the system
        /// </summary>
        /// <param name="id">The id of the user to be deleted</param>
        /// <returns>A response that indicates whether or not the deletion was successful</returns>
        public DeleteUserResponse DeleteUser(int id, string currentUser)
        {
            var response = new DeleteUserResponse();

            try
            {
                User user = _usersDataLayer.GetUser(id);

                if (user == null)
                {
                    response.Status = HttpStatusCode.NotFound;
                    return response;
                }

                if (user.Username == currentUser)
                {
                    response.Status = HttpStatusCode.Forbidden;
                    return response;
                }

                bool result = _usersDataLayer.DeleteUser(user);
                _usersDataLayer.Save();

                if (result)
                    response.Status = HttpStatusCode.OK;
                else
                {
                    response.Status = HttpStatusCode.BadRequest;
                    response.StatusMessages.Add(new StatusMessage(HttpStatusCode.BadRequest, "Unable to delete user."));
                }
            }
            catch (Exception ex)
            {
                s_logger.Error(ex, "Unable to delete the user.");
                response.Status = HttpStatusCode.InternalServerError;
            }

            return response;
        }

        /// <summary>
        /// This method is responsable for handling the validation and verification of the users login attempt
        /// </summary>
        /// <param name="request">the users login information</param>
        /// <returns>The response which indicates if they are sucessful and the bearer token
        /// they will use for authentication on success</returns>
        public LoginResponse LoginUser(LoginRequest request)
        {
            s_logger.Info("Attempting to log in...");

            LoginResponse response = new LoginResponse();

            try
            {
                //Validate input
                response = (LoginResponse)request.CheckValidation(response);

                if (response.Status == HttpStatusCode.BadRequest)
                    return response;

                //Verify user exists
                User user = _usersDataLayer.GetUserByUsername(request.Username);
                if (user == null)
                {
                    response.Token = string.Empty;
                    response.Status = HttpStatusCode.BadRequest;
                    response.StatusMessages.Add(new StatusMessage(HttpStatusCode.BadRequest, "Unable to login username or password is incorrect"));
                    s_logger.Warn($"Unable to login as a username [ {request.Username} ], username or password is incorrect.");
                    return response;
                }

                // Ensure that their password is correct
                string hashedPassword = HashText(request.Password);
                if (user.Password != hashedPassword)
                {
                    response.Token = string.Empty;
                    response.Status = HttpStatusCode.BadRequest;
                    response.StatusMessages.Add(new StatusMessage(HttpStatusCode.BadRequest, "Unable to login username or password is incorrect"));
                    s_logger.Warn($"Unable to login as a username [ {request.Username} ], username or password is incorrect.");
                    return response;
                }

                // Generate users bearer token
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.AppSecret);

                var tokenDescriptior = new SecurityTokenDescriptor()
                {
                    Subject = new ClaimsIdentity(new Claim[] {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.Sid, user.UserId.ToString())
                    }),
                    Expires = DateTime.Now.AddHours(4),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
                };

                var rawToken = tokenHandler.CreateToken(tokenDescriptior);
                var token = tokenHandler.WriteToken(rawToken);

                response.Token = token;

                if (user.FirstTime)
                {
                    response.Status = HttpStatusCode.Accepted;
                    response.UserId = user.UserId;
                    return response;
                }

                response.Status = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                s_logger.Error(ex, "Unable to perform log in attempt.");
                response = new LoginResponse
                {
                    Token = string.Empty,
                    Status = HttpStatusCode.InternalServerError
                };
                response.StatusMessages.Add(new StatusMessage(HttpStatusCode.InternalServerError, "Unable to perform log in attempt."));
            }
            return response;
        }

        /// <summary>
        /// This is used to check that the user that is logged in is actually a valid user in the system 
        /// </summary>
        /// <param name="username">The username of the user</param>
        /// <param name="userId">The id of the user</param>
        /// <returns>An indicator of whether or not the user is valid</returns>
        public bool VerifyUser(string username, string userId)
        {
            bool result = false;

            try
            {
                int userID = -1;

                if (!int.TryParse(userId, out userID))
                    throw new Exception("Invalid user id received.");

                User userFromID = _usersDataLayer.GetUser(userID);

                User userFromUsername = _usersDataLayer.GetUserByUsername(username);

                if (!(userFromID.UserId == userFromUsername.UserId && userFromID.Username == userFromUsername.Username && (!userFromID.FirstTime)))
                {
                    s_logger.Warn("Unable to verify user.");
                    result = false;
                }
                else
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                s_logger.Error(ex, "Unable to perform log in attempt.");
            }
            return result;
        }

        /// <summary>
        /// Used to hash passwords when a user logs in, is added to the system or has their password changed
        /// </summary>
        /// <param name="text">The password in plain text</param>
        /// <returns>The hashed password</returns>
        private string HashText(string text)
        {
            string result = string.Empty;
            try
            {
                byte[] bytes = Encoding.ASCII.GetBytes(text);
                var sha1 = new SHA1CryptoServiceProvider();
                var sha1data = sha1.ComputeHash(bytes);
                result = Convert.ToBase64String(sha1data);
            }
            catch (Exception ex)
            {
                result = string.Empty;
                s_logger.Error(ex, "Unable to hash text");
            }
            return result;
        }

        /// <summary>
        /// Converts the user DAO to a DTO to send to the front end
        /// </summary>
        /// <param name="user">The DAO for the user</param>
        /// <returns>The DTO for the user</returns>
        private UserDTO DAO2DTO(User user)
        {
            UserDTO userDTO = new UserDTO();
            userDTO.UserId = user.UserId;
            userDTO.Username = user.Username;
            userDTO.Password = user.Password;
            userDTO.FirstTime = user.FirstTime;

            return userDTO;
        }

        /// <summary>
        /// Converts the user DTO to a DAO to interact with the database
        /// </summary>
        /// <param name="user">The DTO for the user</param>
        /// <returns>The DAO for the user</returns>
        private User DTO2DAO(UserDTO userDTO)
        {
            User user = null;
            user = new User();
            user.UserId = userDTO.UserId;
            user.Username = userDTO.Username;
            user.Password = userDTO.Password;
            user.FirstTime = userDTO.FirstTime;

            return user;
        }
    }
}
