using Helpdesk.Common.DTOs;
using Helpdesk.Common.Requests.Users;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Helpdesk.DataLayer.Contracts
{
    /// <summary>
    /// Used to handle any databases interactions for users including CRUD, login and logout
    /// </summary>
    public interface IUsersDataLayer
    {
        /// <summary>
        /// Used to add a user to the database.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        int? AddUser(AddUserRequest request);

        /// <summary>
        /// Used to retreve a user by their id
        /// </summary>
        /// <param name="id">The id of the user</param>
        /// <returns>The user DTO</returns>
        UserDTO GetUser(int id);

        /// <summary>
        /// This method retrieves a list of all the users in the database
        /// </summary>
        /// <returns>A list of users retrieved from the database</returns>
        List<UserDTO> GetUsers();

        /// <summary>
        /// Used to get a datatable with all of the helpdesk records
        /// </summary>
        /// <returns>Datatable with the helpdesk records</returns>
        DataTable GetUsersAsDataTable();

        /// <summary>
        /// Used to update the specified user in the databse with the request's information
        /// </summary>
        /// <param name="id">The UserId of the user to be updated</param>
        /// <param name="request">The request that contains the user's new information</param>
        /// <returns>A boolean that indicates whether the operation was a success</returns>
        bool UpdateUser(UpdateUserRequest request);

        /// <summary>
        /// Used to delete the specified user from the database
        /// </summary>
        /// <param name="id">The id of the user to be deleted</param>
        /// <returns>An indication of whether or not the deletion was successful</returns>
        bool DeleteUser(int id);

        /// <summary>
        /// Used to get a user by their username initially made for the login function
        /// </summary>
        /// <param name="username">The username of the user</param>
        /// <returns>The object that represents the user</returns>
        UserDTO GetUserByUsername(string username);
    }
}
