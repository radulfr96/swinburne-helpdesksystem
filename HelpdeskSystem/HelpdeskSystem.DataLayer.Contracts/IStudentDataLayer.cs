using Helpdesk.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace HelpdeskSystem.DataLayer.Contracts
{
    /// <summary>
    /// Used to handle CRUD for student records in the database
    /// </summary>
    public interface IStudentDataLayer
    {
        /// <summary>
        /// used to retreive all nicknames in the database
        /// </summary>
        /// <returns>The list of nicknames as DTOs</returns>
        public List<NicknameDTO> GetAllNicknames();

        /// <summary>
        /// Used to get a student nickname by the nickname
        /// </summary>
        /// <param name="nickname">The nickname to look up</param>
        /// <returns>The nickname</returns>
        public NicknameDTO GetStudentNicknameByNickname(string nickname);

        /// <summary>
        /// Used to get a student nickname by their studentId
        /// </summary>
        /// <param name="studentId">The studentId to look up</param>
        /// <returns>The nickname</returns>
        public NicknameDTO GetStudentNicknameByStudentID(string studentId);

        public DataTable GetStudentsAsDataTable();
    }
}
