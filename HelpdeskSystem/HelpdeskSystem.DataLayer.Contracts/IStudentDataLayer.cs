using Helpdesk.Common.DTOs;
using Helpdesk.Common.Requests.Students;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Helpdesk.DataLayer.Contracts
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
        List<NicknameDTO> GetAllNicknames();

        /// <summary>
        /// Used to get a student nickname by the nickname
        /// </summary>
        /// <param name="nickname">The nickname to look up</param>
        /// <returns>The nickname</returns>
        NicknameDTO GetStudentNicknameByNickname(string nickname);

        /// <summary>
        /// Used to get a student nickname by their studentId
        /// </summary>
        /// <param name="studentId">The studentId to look up</param>
        /// <returns>The nickname</returns>
        NicknameDTO GetStudentNicknameByStudentID(int studentId);

        /// <summary>
        /// Used to get a student nickname by their Swinburne ID
        /// </summary>
        /// <param name="sid">The sid to look up</param>
        /// <returns>The nickname</returns>
        NicknameDTO GetStudentNicknameBySID(string sid);

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
        int AddStudentNickname(AddStudentRequest request);

        /// <summary>
        /// Used to edit the specified student's nickname in the databse with the request's information
        /// </summary>
        /// <param name="id">The StudentID of the student to be updated</param>
        /// <param name="request">The request that contains the student's new nickname</param>
        /// <returns>A boolean that indicates whether the operation was a success</returns>
        bool EditStudentNickname(EditStudentNicknameRequest request);
    }
}
