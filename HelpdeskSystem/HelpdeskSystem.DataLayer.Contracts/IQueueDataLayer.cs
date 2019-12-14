using Helpdesk.Common.DTOs;
using Helpdesk.Common.Requests.Queue;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Helpdesk.DataLayer.Contracts
{
    /// <summary>
    /// Used to handle any databases interactions for queues including CRUD, login and logout
    /// </summary>
    public interface IQueueDataLayer
    {
        /// <summary>
        /// Used to add a queue item to the database
        /// </summary>
        /// <param name="request">The request with the information to be added</param>
        /// <returns>The id of the queue item or null if not successful</returns>
        int AddToQueue(AddToQueueRequest request);

        /// <summary>
        /// Used to update a queue item status in the database.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        bool UpdateQueueItemStatus(UpdateQueueItemStatusRequest request);

        /// <summary>
        /// Edits the queue details in the database.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        bool UpdateQueueItem(UpdateQueueItemRequest request);

        /// <summary>
        /// This method retreives all queue items in a specific helpdesk from the database
        /// </summary>
        /// <param name="id">ID of the helpdesk to retrieve queue items from</param>
        /// <returns>A list of the queue items</returns>
        List<QueueItemDTO> GetQueueItemsByHelpdeskID(int id);

        /// <summary>
        /// Used to retreive all of the queue items for a check in
        /// </summary>
        /// <param name="checkInId">The id of the students check in</param>
        /// <returns>The list of queue items for that check in</returns>
        List<QueueItemDTO> GetQueueItemsByCheckIn(int checkInId);

        /// <summary>
        /// Used to get a datatable with all of the helpdesk records
        /// </summary>
        /// <returns>Datatable with the helpdesk records</returns>
        DataTable GetQueueItemsAsDataTable();
    }
}
