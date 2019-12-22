using Helpdesk.Common.DTOs;
using Helpdesk.Common.Requests.CheckIn;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Helpdesk.Data.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace Helpdesk.DataLayer.Contracts
{
    /// <summary>
    /// Used to handle any database interactions involving checking in and out of a helpdesk
    /// </summary>
    public interface ICheckInDataLayer : IDisposable
    {
        /// <summary>
        /// Checks a new item into the database
        /// </summary>
        /// <param name="request">Request containing the unit id of the check in item</param>
        /// <returns>The id of the new check in item</returns>
        void CheckIn(Checkinhistory checkIn);

        /// <summary>
        /// Used to add a record that links a queue item to a parent check in
        /// </summary>
        /// <param name="checkinqueueitem">The pair to be added</param>
        void AddCheckinQueueItem(Checkinqueueitem checkinqueueitem);

        /// <summary>
        /// Used to retreive the check ins by the helpdesk id
        /// </summary>
        /// <param name="helpdeskId">The id of the helpdesk</param>
        /// <returns>The list of checkins</returns>
        List<Checkinhistory> GetCheckinsByHelpdeskId(int helpdeskId);

        Checkinhistory GetCheckIn(int id);

        /// Used to get a datatable with all of the checkin records
        /// </summary>
        /// <returns>Datatable with the checkin records</returns>
        DataTable GetCheckInsAsDataTable();

        /// Used to get a datatable with all of the checkinqueueitem records
        /// </summary>
        /// <returns>Datatable with the checkinqueueitem records</returns>
        DataTable GetCheckInQueueItemsAsDataTable();

        void Save();

        IDbContextTransaction GetTransaction();
    }
}
