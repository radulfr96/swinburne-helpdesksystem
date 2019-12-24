using Helpdesk.Common;
using Helpdesk.Common.DTOs;
using Helpdesk.Common.Extensions;
using Helpdesk.Common.Requests.Queue;
using Helpdesk.Common.Requests.Students;
using Helpdesk.Common.Responses;
using Helpdesk.Common.Responses.Queue;
using Helpdesk.Data.Models;
using Helpdesk.DataLayer;
using Helpdesk.DataLayer.Contracts;
using Microsoft.EntityFrameworkCore.Storage;
using NLog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Helpdesk.Services
{
    /// <summary>
    /// This class is used to handle the business logic of queues
    /// </summary>
    public class QueueFacade
    {
        private static Logger s_logger = LogManager.GetCurrentClassLogger();
        private IStudentDataLayer _studentDataLayer;
        private IQueueDataLayer _queueDataLayer;
        private ICheckInDataLayer _checkInDataLayer;

        public QueueFacade(IQueueDataLayer queueDataLayer, IStudentDataLayer studentDataLayer, ICheckInDataLayer checkInDataLayer)
        {
            _studentDataLayer = studentDataLayer;
            _queueDataLayer = queueDataLayer;
            _checkInDataLayer = checkInDataLayer;
        }

        /// <summary>
        /// This method is used to add an item to a queue
        /// </summary>
        /// <param name="request">The information of the queue item</param>
        /// <returns>Response which indicates success or failure</returns>
        public AddToQueueResponse AddToQueue(AddToQueueRequest request)
        {
            AddToQueueResponse response = new AddToQueueResponse();

            using (IDbContextTransaction trans = _queueDataLayer.GetTransaction())
            {
                try
                {
                    response = (AddToQueueResponse)request.CheckValidation(response);

                    if (response.Status == HttpStatusCode.BadRequest)
                        return response;

                    StudentFacade studentFacade = new StudentFacade(_studentDataLayer);

                    if (!request.StudentID.HasValue)
                    {
                        var addResponse = studentFacade.AddStudentNickname(new AddStudentRequest()
                        {
                            Nickname = request.Nickname,
                            SID = request.SID
                        });

                        if (addResponse.Status != HttpStatusCode.OK)
                        {
                            throw new Exception("Unable to add student nickname");
                        }
                        request.StudentID = addResponse.StudentID;
                    }

                    var item = new Queueitem()
                    {
                        Description = request.Description,
                        StudentId = request.StudentID.Value,
                        TimeAdded = DateTime.Now,
                        TopicId = request.TopicID
                    };

                    _queueDataLayer.AddToQueue(item);
                    _queueDataLayer.Save();

                    if (request.CheckInID.HasValue)
                    {
                        var checkIn = _checkInDataLayer.GetCheckIn(request.CheckInID.Value);

                        if (checkIn == null)
                        {
                            trans.Rollback();
                            response.Status = HttpStatusCode.NotFound;
                            response.StatusMessages.Add(new StatusMessage(HttpStatusCode.NotFound, "Check in not in database."));
                            return response;
                        }

                        Checkinqueueitem checkinqueueitem = new Checkinqueueitem()
                        {
                            CheckInId = request.CheckInID.Value,
                            QueueItemId = item.ItemId
                        };

                        _checkInDataLayer.AddCheckinQueueItem(checkinqueueitem);
                        _checkInDataLayer.Save();
                    }

                    response.ItemId = item.ItemId;
                    response.Status = HttpStatusCode.OK;

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    s_logger.Error(ex, "Unable to add queue item");
                    response.Status = HttpStatusCode.InternalServerError;
                    response.StatusMessages.Add(new StatusMessage(HttpStatusCode.InternalServerError, "Unable to add queue item"));
                }
            }
            return response;
        }

        /// <summary>
        /// This method is used to update an existing item in the queue
        /// </summary>
        /// <param name="id">ID of the queue item to be updated</param>
        /// <param name="request">Request that contains the new queue item information</param>
        /// <returns>Response which indicates success or failure</returns>
        public UpdateQueueItemResponse UpdateQueueItem(UpdateQueueItemRequest request)
        {
            UpdateQueueItemResponse response = new UpdateQueueItemResponse();

            try
            {
                response = (UpdateQueueItemResponse)request.CheckValidation(response);

                if (response.Status == HttpStatusCode.BadRequest)
                    return response;

                var item = _queueDataLayer.GetQueueitem(request.QueueItemID);

                if (item == null)
                {
                    response.Status = HttpStatusCode.NotFound;
                    response.StatusMessages.Add(new StatusMessage(HttpStatusCode.NotFound, "Unable to find queue item"));
                }
                else
                {
                    item.Description = request.Description;
                    item.TopicId = request.TopicID;
                    _queueDataLayer.Save();
                    response.Status = HttpStatusCode.OK;
                }

            }
            catch (Exception ex)
            {
                s_logger.Error(ex, "Unable to update queue item");
                response.Status = HttpStatusCode.InternalServerError;
                response.StatusMessages.Add(new StatusMessage(HttpStatusCode.InternalServerError, "Unable to update queue item"));
            }
            return response;
        }

        /// <summary>
        /// This method is used to update a queue item's status
        /// </summary>
        /// <param name="id">ID of the specific queue item to be updated</param>
        /// <param name="request">Request which contains the new queue item status</param>
        /// <returns>Response which indicates success or failure</returns>
        public UpdateQueueItemStatusResponse UpdateQueueItemStatus(UpdateQueueItemStatusRequest request)
        {
            UpdateQueueItemStatusResponse response = new UpdateQueueItemStatusResponse();

            try
            {
                response = (UpdateQueueItemStatusResponse)request.CheckValidation(response);

                if (response.Status == HttpStatusCode.BadRequest)
                {
                    return response;
                }

                var item = _queueDataLayer.GetQueueitem(request.QueueID);

                if (item == null)
                {
                    response.Status = HttpStatusCode.NotFound;
                    response.StatusMessages.Add(new StatusMessage(HttpStatusCode.NotFound, "Unable to find queue item."));
                }
                else
                {
                    if (item.TimeHelped == null && request.TimeHelped != null)
                        item.TimeHelped = request.TimeHelped;

                    if (item.TimeRemoved == null && request.TimeRemoved != null)
                        item.TimeRemoved = request.TimeRemoved;

                    _queueDataLayer.Save();
                }
            }
            catch (Exception ex)
            {
                s_logger.Error(ex, "Unable to update queue item status.");
                response.Status = HttpStatusCode.InternalServerError;
                response.StatusMessages.Add(new StatusMessage(HttpStatusCode.InternalServerError, "Unable to update queue item status."));
            }
            return response;
        }

        /// <summary>
        /// This method gets all the queue items in a specific helpdesk
        /// </summary>
        /// <param name="id">The id of the helpdesk</param>
        /// <returns>Response which indicates success or failure</returns>
        public GetQueueItemsByHelpdeskIDResponse GetQueueItemsByHelpdeskID(int id)
        {
            s_logger.Info("Getting queue items by helpdesk id...");

            GetQueueItemsByHelpdeskIDResponse response = new GetQueueItemsByHelpdeskIDResponse();

            try
            {
                List<Queueitem> queueItems = _queueDataLayer.GetQueueItemsByHelpdeskID(id);

                if (queueItems.Count == 0)
                {
                    response.Status = HttpStatusCode.NotFound;
                    response.StatusMessages.Add(new StatusMessage(HttpStatusCode.NotFound, "No queue items found."));
                }
                else
                {
                    foreach (Queueitem item in queueItems)
                    {
                        response.QueueItems.Add(DAO2DTO(item));
                    }
                    response.Status = HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                s_logger.Error(ex, "Unable to get queue items!");
                response.Status = HttpStatusCode.InternalServerError;
                response.StatusMessages.Add(new StatusMessage(HttpStatusCode.InternalServerError, "Unable to get queue items!"));
            }
            return response;
        }

        /// <summary>
        /// Converts the queue item DAO to a DTO to send to the front end
        /// </summary>
        /// <param name="queueItem">The DAO for the queue item</param>
        /// <returns>The DTO for the queue item</returns>
        private QueueItemDTO DAO2DTO(Queueitem queueItem)
        {
            QueueItemDTO queueItemDTO = null;

            queueItemDTO = new QueueItemDTO();
            queueItemDTO.ItemId = queueItem.ItemId;
            queueItemDTO.StudentId = queueItem.StudentId;
            queueItemDTO.Nickname = queueItem.Student.NickName;
            queueItemDTO.TopicId = queueItem.TopicId;
            queueItemDTO.Topic = queueItem.Topic.Name;
            queueItemDTO.Unit = queueItem.Topic.Unit.Name;
            queueItemDTO.TimeAdded = queueItem.TimeAdded;
            queueItemDTO.TimeHelped = queueItem.TimeHelped;
            queueItemDTO.TimeRemoved = queueItem.TimeRemoved;
            queueItemDTO.Description = queueItem.Description;

            return queueItemDTO;
        }

        /// <summary>
        /// Converts the queue item DTO to a DAO to interact with the database
        /// </summary>
        /// <param name="queueItemDTO">The DTO for the queue item</param>
        /// <returns>The DAO for the queue item</returns>
        private Queueitem DTO2DAO(QueueItemDTO queueItemDTO)
        {
            Queueitem queueItem = new Queueitem
            {
                ItemId = queueItemDTO.ItemId,
                StudentId = queueItemDTO.StudentId,
                TopicId = queueItemDTO.TopicId,
                Description = queueItemDTO.Description,
                TimeAdded = queueItemDTO.TimeAdded,
                TimeHelped = queueItemDTO.TimeHelped,
                TimeRemoved = queueItemDTO.TimeRemoved
            };

            return queueItem;
        }
    }
}
