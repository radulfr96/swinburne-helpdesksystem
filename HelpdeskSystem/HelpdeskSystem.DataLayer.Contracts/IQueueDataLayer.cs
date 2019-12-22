using Helpdesk.Common.DTOs;
using Helpdesk.Common.Requests.Queue;
using Helpdesk.Data.Models;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Helpdesk.DataLayer.Contracts
{
    /// <summary>
    /// Used to handle any databases interactions for queues including CRUD, login and logout
    /// </summary>D:\Projects\SwinburneHelpdeskSystem\HelpdeskSystem\HelpdeskSystem.DataLayer.Contracts\IQueueDataLayer.cs
    public interface IQueueDataLayer : IDisposable
    {
        /// <summary>
        /// Used to add a queue item to the database
        /// </summary>
        /// <param name="request">The request with the information to be added</param>
        /// <returns>The id of the queue item or null if not successful</returns>
        void AddToQueue(Queueitem request);

        /// <summary>
        /// This method retreives all queue items in a specific helpdesk from the database
        /// </summary>
        /// <param name="id">ID of the helpdesk to retrieve queue items from</param>
        /// <returns>A list of the queue items</returns>
        List<Queueitem> GetQueueItemsByHelpdeskID(int id);

        /// <summary>
        /// Used to retreive all of the queue items for a check in
        /// </summary>
        /// <param name="checkInId">The id of the students check in</param>
        /// <returns>The list of queue items for that check in</returns>
        List<Queueitem> GetQueueItemsByCheckIn(int checkInId);

        Queueitem GetQueueitem(int id);

        /// <summary>
        /// Used to get a datatable with all of the helpdesk records
        /// </summary>
        /// <returns>Datatable with the helpdesk records</returns>
        DataTable GetQueueItemsAsDataTable();

        void Save();

        IDbContextTransaction GetTransaction();
    }
}
