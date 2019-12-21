using Helpdesk.Common.DTOs;
using Helpdesk.Common.Requests.Users;
using Helpdesk.Data.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Helpdesk.DataLayer.Contracts
{
    /// <summary>
    /// Used to handle any databases interactions for users including CRUD, login and logout
    /// </summary>
    public interface IUsersDataLayer : IDisposable
    {
        /// <summary>
        /// Used to add a user to the database.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        void AddUser(User request);

        /// <summary>
        /// Used to retreve a user by their id
        /// </summary>
        /// <param name="id">The id of the user</param>
        /// <returns>The user DTO</returns>
        User GetUser(int id);

        /// <summary>
        /// This method retrieves a list of all the users in the database
        /// </summary>
        /// <returns>A list of users retrieved from the database</returns>
        List<User> GetUsers();

        /// <summary>
        /// Used to get a datatable with all of the helpdesk records
        /// </summary>
        /// <returns>Datatable with the helpdesk records</returns>
        DataTable GetUsersAsDataTable();

        /// <summary>
        /// Used to delete the specified user from the database
        /// </summary>
        /// <param name="id">The id of the user to be deleted</param>
        /// <returns>An indication of whether or not the deletion was successful</returns>
        bool DeleteUser(User user);

        /// <summary>
        /// Used to get a user by their username initially made for the login function
        /// </summary>
        /// <param name="username">The username of the user</param>
        /// <returns>The object that represents the user</returns>
        User GetUserByUsername(string username);

        void Save();
    }
}
