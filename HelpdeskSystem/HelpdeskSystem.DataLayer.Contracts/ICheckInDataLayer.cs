using Helpdesk.Common.DTOs;
using Helpdesk.Common.Requests.CheckIn;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Helpdesk.DataLayer.Contracts
{
    /// <summary>
    /// Used to handle any database interactions involving checking in and out of a helpdesk
    /// </summary>
    public interface ICheckInDataLayer
    {
        /// <summary>
        /// Checks a new item into the database
        /// </summary>
        /// <param name="request">Request containing the unit id of the check in item</param>
        /// <returns>The id of the new check in item</returns>
        int CheckIn(CheckInRequest request);

        /// <summary>
        /// Checks a check in item out of the database
        /// </summary>
        /// <param name="id">CheckInID of the check in item to be checked out</param>
        /// <returns>A boolean indicating success or failure</returns>
        bool CheckOut(CheckOutRequest request, int id);

        /// <summary>
        /// Used to retreive the check ins by the helpdesk id
        /// </summary>
        /// <param name="helpdeskId">The id of the helpdesk</param>
        /// <returns>The list of checkins</returns>
        List<CheckInDTO> GetCheckinsByHelpdeskId(int helpdeskId);

        /// Used to get a datatable with all of the checkin records
        /// </summary>
        /// <returns>Datatable with the checkin records</returns>
        DataTable GetCheckInsAsDataTable();

        /// Used to get a datatable with all of the checkinqueueitem records
        /// </summary>
        /// <returns>Datatable with the checkinqueueitem records</returns>
        DataTable GetCheckInQueueItemsAsDataTable();
    }
}
