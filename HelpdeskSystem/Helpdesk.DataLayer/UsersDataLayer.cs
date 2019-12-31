using Helpdesk.Common.DTOs;
using Helpdesk.Data.Models;
using Helpdesk.Common.Requests.Users;
using System;
using System.Collections.Generic;
using NLog;
using System.Text;
using System.Linq;
using System.Data;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using Helpdesk.DataLayer.Contracts;

namespace Helpdesk.DataLayer
{
    public class UsersDataLayer : IUsersDataLayer, IDisposable
    {
        private static Logger s_Logger = LogManager.GetCurrentClassLogger();
        private helpdesksystemContext context;

        public UsersDataLayer(helpdesksystemContext _context)
        {
            context = _context;
        }

        public void AddUser(User user)
        {
            context.User.Add(user);
        }

        public User GetUser(int id)
        {
            return context.User.FirstOrDefault(u => u.UserId == id);
        }

        public List<User> GetUsers()
        {
            return context.User.ToList();
        }

        public DataTable GetUsersAsDataTable()
        {
            DataTable users = new DataTable();
            DbConnection conn = context.Database.GetDbConnection();
            ConnectionState state = conn.State;

            try
            {
                if (state != ConnectionState.Open)
                    conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "GetAllUsers";
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        users.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (state != ConnectionState.Closed)
                    conn.Close();
            }

            return users;
        }

        public bool DeleteUser(User user)
        {
            context.User.Remove(user);
            return true;
        }

        public User GetUserByUsername(string username)
        {
            return context.User.FirstOrDefault(u => u.Username == username);
        }

        public void Dispose()
        {
            context.Dispose();
        }

        public void Save()
        {
            context.SaveChanges();
        }
    }
}
