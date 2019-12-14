﻿using Helpdesk.Common;
using Helpdesk.Common.DTOs;
using Helpdesk.Common.Extensions;
using Helpdesk.Common.Requests;
using Helpdesk.Common.Requests.CheckIn;
using Helpdesk.Common.Requests.Students;
using Helpdesk.Common.Responses;
using Helpdesk.Common.Responses.CheckIn;
using Helpdesk.Common.Responses.Students;
using Helpdesk.DataLayer;
using NLog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Text;
using Helpdesk.Common.Requests.Queue;
using Helpdesk.DataLayer.Contracts;

namespace Helpdesk.Services
{
    /// <summary>
    /// This class is used to handle the business logic of checking in
    /// </summary>
    public class CheckInFacade
    {
        private static Logger s_logger = LogManager.GetCurrentClassLogger();

        private ICheckInDataLayer _checkInDataLayer;
        private IStudentDataLayer _studentDataLayer;
        private IQueueDataLayer _queueDataLayer;

        public CheckInFacade(ICheckInDataLayer checkInDataLayer, IStudentDataLayer studentDataLayer, IQueueDataLayer queueDataLayer)
        {
            _checkInDataLayer = checkInDataLayer;
            _studentDataLayer = studentDataLayer;
            _queueDataLayer = queueDataLayer;

        }

        /// <summary>
        /// This method is used to check in into the helpdesk system
        /// </summary>
        /// <param name="request">The request containing the specified UnitID</param>
        /// <returns>A response indicating success or failure</returns>
        public CheckInResponse CheckIn(CheckInRequest request)
        {
            CheckInResponse response = new CheckInResponse();

            try
            {
                response = (CheckInResponse)request.CheckValidation(response);

                if (response.Status == HttpStatusCode.BadRequest)
                    return response;

                StudentFacade studentFacade = new StudentFacade(_studentDataLayer);

                if (!request.StudentID.HasValue)
                {
                    if (studentFacade.GetStudentByNickname(request.Nickname).Status != HttpStatusCode.NotFound)
                    {
                        response.Status = HttpStatusCode.BadRequest;
                        return response;
                    }

                    AddStudentRequest addStudentRequest = new AddStudentRequest()
                    {
                        SID = request.SID,
                        Nickname = request.Nickname
                    };

                    AddStudentResponse addStudentResponse = studentFacade.AddStudentNickname(addStudentRequest);

                    request.StudentID = addStudentResponse.StudentID;
                }

                var existingByID = _studentDataLayer.GetStudentNicknameByStudentID(request.StudentID.Value);

                if (existingByID == null)
                    throw new NotFoundException("No student found for id " + request.StudentID);

                int checkInID = _checkInDataLayer.CheckIn(request);

                response.StudentID = request.StudentID.Value;
                response.CheckInID = checkInID;
                response.Status = HttpStatusCode.OK;
            }
            catch (NotFoundException ex)
            {
                s_logger.Warn(ex, "No student found for id " + request.SID);
                response.Status = HttpStatusCode.NotFound;
                response.StatusMessages.Add(new StatusMessage(HttpStatusCode.NotFound, "No student found for id " + request.SID));
            }
            catch (Exception ex)
            {
                s_logger.Error(ex, "Unable to check in");
                response.Status = HttpStatusCode.InternalServerError;
                response.StatusMessages.Add(new StatusMessage(HttpStatusCode.InternalServerError, "Unable to check in"));
            }
            return response;
        }

        /// <summary>
        /// This method is used to check a check in item out of the helpdesk system
        /// </summary>
        /// <param name="id">Specified CheckInID</param>
        /// <returns>A response indicating success or failure</returns>
        public CheckOutResponse CheckOut(CheckOutRequest request, int id)
        {
            CheckOutResponse response = new CheckOutResponse();

            try
            {
                bool result = _checkInDataLayer.CheckOut(request, id);

                if (result == false)
                    throw new NotFoundException("Unable to find check in item!");

                var queueItems = _queueDataLayer.GetQueueItemsByCheckIn(id);
                UpdateQueueItemStatusRequest removeRequest = new UpdateQueueItemStatusRequest()
                {
                    TimeRemoved = DateTime.Now
                };

                foreach (var item in queueItems)
                {
                    _queueDataLayer.UpdateQueueItemStatus(item.ItemId, removeRequest);
                }

                response.Result = result;
                response.Status = HttpStatusCode.OK;
            }
            catch (NotFoundException ex)
            {
                s_logger.Warn(ex, "Unable to find check out item");
                response.Status = HttpStatusCode.NotFound;
                response.StatusMessages.Add(new StatusMessage(HttpStatusCode.NotFound, "Unable to find check out item"));
            }
            catch (Exception ex)
            {
                s_logger.Error(ex, "Unable to check out");
                response.Status = HttpStatusCode.InternalServerError;
                response.StatusMessages.Add(new StatusMessage(HttpStatusCode.InternalServerError, "Unable to check out"));
            }
            return response;
        }

        /// <summary>
        /// Used to get the check ins for a helpdesk
        /// </summary>
        /// <param name="helpdeskId">The id of the helpdesk</param>
        /// <returns>Response with checkins if found and the success result</returns>
        public GetCheckInsResponse GetCheckInsByHelpdeskId(int helpdeskId)
        {
            var response = new GetCheckInsResponse();

            try
            {
                response.CheckIns = _checkInDataLayer.GetCheckinsByHelpdeskId(helpdeskId);
                response.Status = HttpStatusCode.OK;
            }
            catch (NotFoundException ex)
            {
                s_logger.Trace(ex, "No check ins found");
                response.Status = HttpStatusCode.NotFound;
                response.StatusMessages.Add(new StatusMessage(HttpStatusCode.NotFound, "No check ins found"));
            }
            catch (Exception ex)
            {
                s_logger.Error(ex, "Unable to get check ins");
                response.Status = HttpStatusCode.InternalServerError;
                response.StatusMessages.Add(new StatusMessage(HttpStatusCode.InternalServerError, "Unable to get check ins"));
            }

            return response;
        }
    }
}
