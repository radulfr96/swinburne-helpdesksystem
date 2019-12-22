using Helpdesk.Common.DTOs;
using Helpdesk.Common.Requests.Students;
using Helpdesk.Data.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Helpdesk.DataLayer.Contracts
{
    /// <summary>
    /// Used to handle CRUD for student records in the database
    /// </summary>
    public interface IStudentDataLayer : IDisposable
    {
        /// <summary>
        /// used to retreive all nicknames in the database
        /// </summary>
        /// <returns>The list of nicknames as DTOs</returns>
        List<Nicknames> GetAllNicknames();

        /// <summary>
        /// Used to get a student nickname by the nickname
        /// </summary>
        /// <param name="nickname">The nickname to look up</param>
        /// <returns>The nickname</returns>
        Nicknames GetStudentNicknameByNickname(string nickname);

        /// <summary>
        /// Used to get a student nickname by their studentId
        /// </summary>
        /// <param name="studentId">The studentId to look up</param>
        /// <returns>The nickname</returns>
        Nicknames GetStudentNicknameByStudentID(int studentId);

        /// <summary>
        /// Used to get a student nickname by their Swinburne ID
        /// </summary>
        /// <param name="sid">The sid to look up</param>
        /// <returns>The nickname</returns>
        Nicknames GetStudentNicknameBySID(string sid);

        /// <summary>
        /// Used to get a datatable with all of the helpdesk records
        /// </summary>
        /// <returns>Datatable with the helpdesk records</returns>
        DataTable GetStudentsAsDataTable();

        /// <summary>
        /// Used to add a nickname to the database
        /// </summary>
        /// <param name="request">The nickname information</param>
        /// <returns>The id of the nickname added</returns>
        void AddStudentNickname(Nicknames Nickname);

        void Save();
    }
}
