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
using HelpdeskSystem.DataLayer.Contracts;

namespace Helpdesk.DataLayer
{
    public class StudentDatalayer : IStudentDataLayer
    {
        public List<NicknameDTO> GetAllNicknames()
        {
            List<NicknameDTO> nicknameDTOs = new List<NicknameDTO>();

            using (helpdesksystemContext context = new helpdesksystemContext())
            {
                var nicknames = context.Nicknames.ToList();

                foreach (var nickname in nicknames)
                {
                    nicknameDTOs.Add(DAO2DTO(nickname));
                }
            }

            return nicknameDTOs;
        }

        public NicknameDTO GetStudentNicknameByNickname(string nickname)
        {
            NicknameDTO nicknameDTO = null;

            using (helpdesksystemContext context = new helpdesksystemContext())
            {
                var nicknameDAO = context.Nicknames.FirstOrDefault(p => p.NickName == nickname);

                if (nicknameDAO == null)
                    return null;

                nicknameDTO = DAO2DTO(nicknameDAO);
            }

            return nicknameDTO;
        }

        public NicknameDTO GetStudentNicknameBySID(string sid)
        {
            NicknameDTO nicknameDTO = null;

            using (helpdesksystemContext context = new helpdesksystemContext())
            {
                var nicknameDAO = context.Nicknames.FirstOrDefault(p => p.Sid == sid);

                if (nicknameDAO == null)
                    return null;

                nicknameDTO = DAO2DTO(nicknameDAO);
            }

            return nicknameDTO;
        }

        public NicknameDTO GetStudentNicknameByStudentID(int studentId)
        {
            NicknameDTO nicknameDTO = null;

            using (helpdesksystemContext context = new helpdesksystemContext())
            {
                var nicknameDAO = context.Nicknames.FirstOrDefault(p => p.StudentId == studentId);

                if (nicknameDAO == null)
                    return null;

                nicknameDTO = DAO2DTO(nicknameDAO);
            }

            return nicknameDTO;
        }

        public DataTable GetStudentsAsDataTable()
        {
            DataTable nicknames = new DataTable();

            using (helpdesksystemContext context = new helpdesksystemContext())
            {
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
            }

            return nicknames;
        }

        public int AddStudentNickname(AddStudentRequest request)
        {

            Nicknames nickname = new Nicknames()
            {
                NickName = request.Nickname,
                Sid = request.SID
            };

            using (helpdesksystemContext context = new helpdesksystemContext())
            {
                context.Nicknames.Add(nickname);

                context.SaveChanges();
            }

            return nickname.StudentId;
        }

        public bool EditStudentNickname(int id, EditStudentNicknameRequest request)
        {
            using (helpdesksystemContext context = new helpdesksystemContext())
            {
                Nicknames nickname = context.Nicknames.FirstOrDefault(n => n.StudentId == id);

                if (nickname == null)
                    return false;
    
                nickname.NickName = request.Nickname;

                context.SaveChanges();
            }
            return true;
        }

        /// <summary>
        /// Converts the nickname DAO to a DTO to send to the front end
        /// </summary>
        /// <param name="nickname">The DAO for the nickname</param>
        /// <returns>The DTO for the nickname</returns>
        private NicknameDTO DAO2DTO(Nicknames nickname)
        {
            NicknameDTO nicknameDTO = null;

            nicknameDTO = new NicknameDTO();
            nicknameDTO.ID = nickname.StudentId;
            nicknameDTO.Nickname = nickname.NickName;
            nicknameDTO.StudentID = nickname.Sid;

            return nicknameDTO;
        }

        /// <summary>
        /// Converts the nickname DTO to a DAO to interact with the database
        /// </summary>
        /// <param name="nickname">The DTO for the nickname</param>
        /// <returns>The DAO for the nickname</returns>
        private Nicknames DTO2DAO(Nicknames nicknameDTO)
        {
            Nicknames nickname = null;
            nickname = new Nicknames()
            {
                NickName = nicknameDTO.NickName,
                Sid = nicknameDTO.Sid,
                StudentId = nickname.StudentId
            };

            return nickname;
        }
    }
}
