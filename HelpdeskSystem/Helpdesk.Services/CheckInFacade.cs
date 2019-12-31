using Helpdesk.Common;
using Helpdesk.Common.DTOs;
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
using Helpdesk.Data.Models;
using Microsoft.EntityFrameworkCore.Storage;

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

                using (var trans = _checkInDataLayer.GetTransaction())
                {
                    try
                    {
                        if (!request.StudentID.HasValue)
                        {
                            if (_studentDataLayer.GetStudentNicknameByNickname(request.Nickname) != null)
                            {
                                response.Status = HttpStatusCode.BadRequest;
                                response.StatusMessages.Add(new StatusMessage(HttpStatusCode.BadRequest, "Nickname already taken."));
                                return response;
                            }

                            Nicknames nickname = new Nicknames()
                            {
                                NickName = request.Nickname,
                                Sid = request.SID
                            };

                            _studentDataLayer.AddStudentNickname(nickname);
                            _studentDataLayer.Save();
                            request.StudentID = nickname.StudentId;
                        }
                        else
                        {
                            var existingNickname = _studentDataLayer.GetStudentNicknameByStudentID(request.StudentID.Value);
                            if (existingNickname == null)
                            {
                                response.Status = HttpStatusCode.NotFound;
                                response.StatusMessages.Add(new StatusMessage(HttpStatusCode.NotFound, "Unable to find student"));
                                return response;
                            }
                        }

                        Checkinhistory checkIn = new Checkinhistory()
                        {
                            CheckInTime = DateTime.Now,
                            StudentId = request.StudentID,
                            UnitId = request.UnitID,
                        };

                        _checkInDataLayer.CheckIn(checkIn);
                        _checkInDataLayer.Save();

                        response.StudentID = request.StudentID.Value;
                        response.CheckInID = checkIn.CheckInId;
                        response.Status = HttpStatusCode.OK;
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw ex;
                    }
                }
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
        public CheckOutResponse CheckOut(CheckOutRequest request)
        {
            CheckOutResponse response = new CheckOutResponse();

            try
            {
                var checkIn = _checkInDataLayer.GetCheckIn(request.CheckInID);

                if (checkIn == null)
                {
                    response.Status = HttpStatusCode.NotFound;
                    response.StatusMessages.Add(new StatusMessage(HttpStatusCode.NotFound, "Unable to find check in."));
                    return response;
                }

                using (IDbContextTransaction trans = _checkInDataLayer.GetTransaction())
                {
                    try
                    {
                        checkIn.CheckoutTime = DateTime.Now;
                        if (request.ForcedCheckout.HasValue)
                            checkIn.ForcedCheckout = request.ForcedCheckout;

                        var queueItems = _queueDataLayer.GetQueueItemsByCheckIn(request.CheckInID);

                        foreach (var item in queueItems)
                        {
                            item.TimeRemoved = DateTime.Now;
                        }
                        _queueDataLayer.Save();
                        _checkInDataLayer.Save();
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw ex;
                    }
                }
                response.Status = HttpStatusCode.OK;
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
                var checkIns = _checkInDataLayer.GetCheckinsByHelpdeskId(helpdeskId);

                if (checkIns.Count == 0)
                {
                    response.Status = HttpStatusCode.NotFound;
                    response.StatusMessages.Add(new StatusMessage(HttpStatusCode.NotFound, "No checkins found."));
                }
                else
                {
                    foreach (Checkinhistory checkin in checkIns)
                    {
                        response.CheckIns.Add(DAO2DTO(checkin));
                    }
                    response.Status = HttpStatusCode.OK;
                }

            }
            catch (Exception ex)
            {
                s_logger.Error(ex, "Unable to get check ins");
                response.Status = HttpStatusCode.InternalServerError;
                response.StatusMessages.Add(new StatusMessage(HttpStatusCode.InternalServerError, "Unable to get check ins"));
            }

            return response;
        }

        public CheckInDTO DAO2DTO(Checkinhistory checkIn)
        {
            CheckInDTO dto = new CheckInDTO()
            {
                CheckInId = checkIn.CheckInId,
                Nickname = checkIn.Student.NickName,
                UnitId = checkIn.UnitId,
                StudentId = checkIn.StudentId.Value
            };

            return dto;
        }
    }
}
