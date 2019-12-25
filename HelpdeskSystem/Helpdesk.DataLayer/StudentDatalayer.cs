using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using Helpdesk.Common.DTOs;
using Helpdesk.Common.Requests.Students;
using Helpdesk.Data.Models;
using Microsoft.EntityFrameworkCore;
using Helpdesk.DataLayer.Contracts;
using Microsoft.EntityFrameworkCore.Storage;

namespace Helpdesk.DataLayer
{
    public class StudentDataLayer : IStudentDataLayer, IDisposable
    {
        private helpdesksystemContext context;

        public StudentDataLayer()
        {
            context = new helpdesksystemContext();
        }

        public List<Nicknames> GetAllNicknames()
        {
            return context.Nicknames.ToList();
        }

        public Nicknames GetStudentNicknameByNickname(string nickname)
        {
            return context.Nicknames.FirstOrDefault(p => p.NickName == nickname);
        }

        public Nicknames GetStudentNicknameBySID(string sid)
        {
            return context.Nicknames.FirstOrDefault(p => p.Sid == sid);
        }

        public Nicknames GetStudentNicknameByStudentID(int studentId)
        {
            return context.Nicknames.FirstOrDefault(p => p.StudentId == studentId);
        }

        public DataTable GetStudentsAsDataTable()
        {
            DataTable nicknames = new DataTable();
            DbConnection conn = context.Database.GetDbConnection();
            ConnectionState state = conn.State;

            try
            {
                if (state != ConnectionState.Open)
                    conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "GetAllNicknames";
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        nicknames.Load(reader);
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
            return nicknames;
        }

        public void AddStudentNickname(Nicknames nickname)
        {
            context.Nicknames.Add(nickname);
        }

        public IDbContextTransaction GetTransaction()
        {
            return context.Database.BeginTransaction();
        }

        public void Save()
        {
            context.SaveChanges();
        }

        public void Dispose()
        {
            context.Dispose();
        }
    }
}
