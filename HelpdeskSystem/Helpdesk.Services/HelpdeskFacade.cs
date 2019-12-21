using Helpdesk.Common;
using Helpdesk.Common.DTOs;
using Helpdesk.Common.Extensions;
using Helpdesk.Common.Requests.Helpdesk;
using Helpdesk.Common.Responses;
using Helpdesk.Common.Responses.Helpdesk;
using Helpdesk.Common.Utilities;
using Helpdesk.Data.Models;
using Helpdesk.DataLayer;
using Helpdesk.DataLayer.Contracts;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;

namespace Helpdesk.Services
{
    /// <summary>
    /// Used to handle business logic related to helpdesks and their report timespans
    /// </summary>
    public class HelpdeskFacade
    {
        private static Logger s_logger = LogManager.GetCurrentClassLogger();

        private readonly AppSettings _appSettings;

        private IHelpdeskDataLayer _helpdeskDataLayer;
        private IUsersDataLayer _usersDataLayer;
        private IUnitsDataLayer _unitsDataLayer;
        private ITopicsDataLayer _topicsDataLayer;
        private IStudentDataLayer _studentsDataLayer;
        private IQueueDataLayer _queueDataLayer;
        private ICheckInDataLayer _checkInDataLayer;

        public HelpdeskFacade(
            IHelpdeskDataLayer helpdeskDataLayer,
            IUsersDataLayer usersDataLayer,
            IUnitsDataLayer unitsDataLayer,
            ITopicsDataLayer topicsDataLayer,
            IStudentDataLayer studentsDataLayer,
            IQueueDataLayer queueDataLayer,
            ICheckInDataLayer checkInDataLayer
            )
        {
            _appSettings = new AppSettings();
            _helpdeskDataLayer = helpdeskDataLayer;
            _usersDataLayer = usersDataLayer;
            _unitsDataLayer = unitsDataLayer;
            _topicsDataLayer = topicsDataLayer;
            _studentsDataLayer = studentsDataLayer;
            _queueDataLayer = queueDataLayer;
            _checkInDataLayer = checkInDataLayer;
        }

        /// <summary>
        /// This method is to handle adding helpdesk business logic
        /// </summary>
        /// <param name="request">This is the request with the info to add the helpdesk</param>
        /// <returns>Returns a response with the id and indications of success</returns>
        public AddHelpdeskResponse AddHelpdesk(AddHelpdeskRequest request)
        {
            var response = new AddHelpdeskResponse();

            try
            {
                response = (AddHelpdeskResponse)request.CheckValidation(response);
                if (response.Status == HttpStatusCode.BadRequest)
                    return response;

                Helpdesksettings helpdesk = new Helpdesksettings()
                {
                    HasCheckIn = request.HasCheckIn,
                    HasQueue = request.HasQueue,
                    IsDeleted = false,
                    Name = request.Name
                };

                _helpdeskDataLayer.AddHelpdesk(helpdesk);
                _helpdeskDataLayer.Save();

                if (helpdesk.HelpdeskId > 0)
                {
                    response.HelpdeskID = helpdesk.HelpdeskId;
                    response.Status = HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                response.Status = HttpStatusCode.InternalServerError;
                response.StatusMessages.Add(new StatusMessage(HttpStatusCode.InternalServerError, "Unable to add helpdesk"));
                s_logger.Error(ex, "Unable to add helpdesk.");
            }
            return response;
        }

        /// <summary>
        /// Used to return all of the active helpdesks
        /// </summary>
        /// <returns>A response indicating the success and a list of active helpdesks</returns>
        public GetHelpdesksResponse GetActiveHelpdesks()
        {
            var response = new GetHelpdesksResponse();

            try
            {
                var helpdesks = _helpdeskDataLayer.GetActiveHelpdesks();

                foreach (Helpdesksettings helpdesk in helpdesks)
                {
                    response.Helpdesks.Add(DAO2DTO(helpdesk));
                }

                response.Status = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                response.Status = HttpStatusCode.InternalServerError;
                response.StatusMessages.Add(new StatusMessage(HttpStatusCode.InternalServerError, "Unable to get helpdesks"));
                s_logger.Error(ex, "Unable to get helpdesks.");
            }
            return response;
        }

        /// <summary>
        /// Used to return all of the helpdesks
        /// </summary>
        /// <returns>A response indicating the success and a list of helpdesks</returns>
        public GetHelpdesksResponse GetHelpdesks()
        {
            var response = new GetHelpdesksResponse();

            try
            {
                var helpdesks = _helpdeskDataLayer.GetHelpdesks();

                foreach (Helpdesksettings helpdesk in helpdesks)
                {
                    response.Helpdesks.Add(DAO2DTO(helpdesk));
                }

                response.Status = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                response.Status = HttpStatusCode.InternalServerError;
                response.StatusMessages.Add(new StatusMessage(HttpStatusCode.InternalServerError, "Unable to get helpdesks"));
                s_logger.Error(ex, "Unable to get helpdesks.");
            }
            return response;
        }

        /// <summary>
        /// Used to get a helpdesk using it's id
        /// </summary>
        /// <param name="id">The id of the helpdesk to be retreived</param>
        /// <returns>A response indicating the success and helpdesk DTO or null</returns>
        public GetHelpdeskResponse GetHelpdesk(int id)
        {
            var response = new GetHelpdeskResponse();

            try
            {
                var helpdesk = _helpdeskDataLayer.GetHelpdesk(id);
                if (helpdesk == null)
                {
                    response.Status = HttpStatusCode.NotFound;
                    response.StatusMessages.Add(new StatusMessage(HttpStatusCode.NotFound, "Unable to find helpdesk"));
                }
                else
                {
                    response.Helpdesk = DAO2DTO(helpdesk);
                    response.Status = HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                response.Status = HttpStatusCode.InternalServerError;
                response.StatusMessages.Add(new StatusMessage(HttpStatusCode.InternalServerError, "Unable to get helpdesk"));
                s_logger.Error(ex, $"Unable to get helpdesk with id [{id}].");
            }
            return response;
        }

        /// <summary>
        /// This method is to handle updating helpdesk business logic
        /// </summary>
        /// <param name="id">The id of the helpdesk to be updated</param>
        /// <param name="request">This is the request with the info to update the helpdesk</param>
        /// <returns>Returns a response which indicate the result</returns>
        public UpdateHelpdeskResponse UpdateHelpdesk(UpdateHelpdeskRequest request)
        {
            var response = new UpdateHelpdeskResponse();

            try
            {
                response = (UpdateHelpdeskResponse)request.CheckValidation(response);

                if (response.Status == HttpStatusCode.BadRequest)
                    return response;

                var helpdesk = _helpdeskDataLayer.GetHelpdesk(request.HelpdeskID);
                helpdesk.HasCheckIn = request.HasCheckIn;
                helpdesk.HasQueue = request.HasQueue;
                helpdesk.Name = request.Name;

                _helpdeskDataLayer.Save();
            }
            catch (Exception ex)
            {
                response.Status = HttpStatusCode.InternalServerError;
                response.StatusMessages.Add(new StatusMessage(HttpStatusCode.InternalServerError, "Unable to update helpdesk"));
                s_logger.Error(ex, "Unable to update helpdesk.");
            }

            return response;
        }

        /// <summary>
        /// This method is responsible for retrieving all timespans from the helpdesk system
        /// </summary>
        /// <returns>The response that indicates if the operation was a success,
        /// and the list of timespans</returns>
        public GetTimeSpansResponse GetTimeSpans()
        {
            s_logger.Info("Getting timespans...");

            GetTimeSpansResponse response = new GetTimeSpansResponse();

            try
            {
                var timespans = _helpdeskDataLayer.GetTimeSpans();

                if (timespans.Count > 0)
                {
                    foreach (Timespans timespan in timespans)
                    {
                        response.Timespans.Add(timespanDAO2DTO(timespan));
                    }
                    response.Status = HttpStatusCode.OK;
                }
                else
                {
                    response.Status = HttpStatusCode.BadRequest;
                    response.StatusMessages.Add(new StatusMessage(HttpStatusCode.BadRequest, "Unable to find timespans"));
                }
            }
            catch (Exception ex)
            {
                s_logger.Error(ex, "Unable to get timespans!");
                response.Status = HttpStatusCode.InternalServerError;
                response.StatusMessages.Add(new StatusMessage(HttpStatusCode.InternalServerError, "Unable to get timespans!"));
            }
            return response;
        }

        /// <summary>
        /// This method is responsible for getting a specific timespan from the helpdesk system
        /// </summary>
        /// <param name="id">The SpanId of the specific timespan to be retrieved</param>
        /// <returns>The response that indicates if the operation was a success,
        /// and the details of the retrieved timespan if it was</returns>
        public GetTimeSpanResponse GetTimeSpan(int id)
        {
            s_logger.Info("Getting timespan...");

            GetTimeSpanResponse response = new GetTimeSpanResponse();

            try
            {
                Timespans timespan = _helpdeskDataLayer.GetTimeSpan(id);

                if (timespan == null)
                {
                    response.Status = HttpStatusCode.NotFound;
                    response.StatusMessages.Add(new StatusMessage(HttpStatusCode.NotFound, "Unable to find timespan."));
                }
                else
                {
                    response.Timespan = timespanDAO2DTO(timespan);
                    response.Status = HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                s_logger.Error(ex, "Unable to get timespan!");
                response.Status = HttpStatusCode.InternalServerError;
                response.StatusMessages.Add(new StatusMessage(HttpStatusCode.InternalServerError, "Unable to get timespan!"));
            }
            return response;
        }

        /// <summary>
        /// This method is responsible for adding a new timespan.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public AddTimeSpanResponse AddTimeSpan(AddTimeSpanRequest request)
        {
            s_logger.Info("Adding timespan...");

            AddTimeSpanResponse response = new AddTimeSpanResponse();

            try
            {
                response = (AddTimeSpanResponse)request.CheckValidation(response);
                if (response.Status == HttpStatusCode.BadRequest)
                {
                    return response;
                }

                Timespans timespan = new Timespans()
                {
                    EndDate = request.EndDate,
                    Name = request.Name,
                    StartDate = request.StartDate,
                    HelpdeskId = request.HelpdeskId
                };

                _helpdeskDataLayer.AddTimeSpan(timespan);
                _helpdeskDataLayer.Save();

                response.SpanId = timespan.SpanId;
                response.Status = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                s_logger.Error(ex, "Unable to add timespan!");
                response.Status = HttpStatusCode.InternalServerError;
                response.StatusMessages.Add(new StatusMessage(HttpStatusCode.InternalServerError, "Unable to add timespan!"));
            }
            return response;
        }

        /// <summary>
        /// This method is responsible for updating a specific timespan's information
        /// </summary>
        /// <param name="id">The SpanId of the timespan to be updated</param>
        /// <param name="request">The timespan's new information</param>
        /// <returns>The response that indicates if the operation was a success</returns>
        public UpdateTimeSpanResponse UpdateTimeSpan(UpdateTimeSpanRequest request)
        {
            s_logger.Info("Updating timespan...");

            UpdateTimeSpanResponse response = new UpdateTimeSpanResponse();

            try
            {
                response = (UpdateTimeSpanResponse)request.CheckValidation(response);

                if (response.Status == HttpStatusCode.BadRequest)
                    return response;

                var timespan = _helpdeskDataLayer.GetTimeSpan(request.TimeSpanID);

                timespan.EndDate = request.EndDate;
                timespan.Name = request.Name;
                timespan.StartDate = request.StartDate;

                _helpdeskDataLayer.Save();
                response.Status = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                s_logger.Error(ex, "Unable to update timespan!");
                response.Status = HttpStatusCode.InternalServerError;
                response.StatusMessages.Add(new StatusMessage(HttpStatusCode.InternalServerError, "Unable to update timespan!"));
            }
            return response;
        }


        /// <summary>
        /// Use to generate a database export and return it as a stream
        /// </summary>
        /// <returns>Response containing the stream of the file</returns>
        public DatabaseExportResponse ExportDatabaseManual()
        {
            var response = new DatabaseExportResponse();

            try
            {
                response = ExportDatabase();

                if (response.Status != HttpStatusCode.OK)
                    return response;

                response.File = FileToBytes(response.Path);
            }
            catch (Exception ex)
            {
                s_logger.Error(ex, "Unable to perform database export");
                response.Status = HttpStatusCode.InternalServerError;
            }

            return response;
        }

        /// <summary>
        /// Used to get a Zip file of all of the database tables as CSVs
        /// </summary>
        public DatabaseExportResponse ExportDatabase()
        {
            var response = new DatabaseExportResponse();

            try
            {
                FileProccessing proccessing = new FileProccessing();

                DateTime now = DateTime.Now;

                string exportName = $"databaseexport_{now.ToString("yyyyddMM_HHmmss")}";

                string fullZipPath = proccessing.CreateZip(_appSettings.DatabaseBackupDestination, exportName);

                if (string.IsNullOrEmpty(fullZipPath))
                {
                    s_logger.Error("Unable to create empty zip");
                    response.Status = HttpStatusCode.InternalServerError;
                    return response;
                }
                else
                {
                    DataTable helpdesks = _helpdeskDataLayer.GetHelpdesksAsDataTable();
                    proccessing.SaveToZIPAsCSV(fullZipPath, "helpdesks", helpdesks);

                    DataTable timespans = _helpdeskDataLayer.GetTimeSpansAsDataTable();
                    proccessing.SaveToZIPAsCSV(fullZipPath, "timespans", timespans);

                    DataTable helpdeskUnits = _helpdeskDataLayer.GetHelpdeskUnitsAsDataTable();
                    proccessing.SaveToZIPAsCSV(fullZipPath, "helpdeskunits", helpdeskUnits);

                    DataTable users = _usersDataLayer.GetUsersAsDataTable();
                    proccessing.SaveToZIPAsCSV(fullZipPath, "users", users);

                    DataTable units = _unitsDataLayer.GetUnitsAsDataTable();
                    proccessing.SaveToZIPAsCSV(fullZipPath, "units", units);

                    DataTable topics = _topicsDataLayer.GetTopicsAsDataTable();
                    proccessing.SaveToZIPAsCSV(fullZipPath, "topics", topics);

                    DataTable students = _studentsDataLayer.GetStudentsAsDataTable();
                    proccessing.SaveToZIPAsCSV(fullZipPath, "students", students);

                    DataTable queuesItems = _queueDataLayer.GetQueueItemsAsDataTable();
                    proccessing.SaveToZIPAsCSV(fullZipPath, "queueItems", queuesItems);

                    DataTable checkIns = _checkInDataLayer.GetCheckInsAsDataTable();
                    proccessing.SaveToZIPAsCSV(fullZipPath, "checkInHistory", checkIns);

                    DataTable checkInQueueItems = _checkInDataLayer.GetCheckInQueueItemsAsDataTable();
                    proccessing.SaveToZIPAsCSV(fullZipPath, "checkinqueueitem", checkInQueueItems);

                    response.Path = fullZipPath;
                    response.Status = HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                s_logger.Error(ex, "Unable to generate database export");
                response.Status = HttpStatusCode.InternalServerError;
            }
            return response;
        }

        /// <summary>
        /// Used to load the file at the path specified into a stream
        /// </summary>
        /// <param name="path">The full location of the file</param>
        /// <returns>The file as a stream</returns>
        private byte[] FileToBytes(string path)
        {
            byte[] file = null;

            try
            {
                file = File.ReadAllBytes(path);
            }
            catch (Exception ex)
            {
                s_logger.Error(ex, "Unable to load file to stream");
                file = null;
            }

            return file;
        }

        /// <summary>
        /// Used to force-checkout users and remove queue items.
        /// Takes optional DateTime parameter. If not provided, data layer will use DateTime.Now.
        /// Used by DailyCleanupJob.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public ForceCheckoutQueueRemoveResponse ForceCheckoutQueueRemove(int id)
        {
            ForceCheckoutQueueRemoveResponse response = new ForceCheckoutQueueRemoveResponse();

            try
            {
                var queueItems = _queueDataLayer.GetQueueItemsByHelpdeskID(id);
                var checkIns = _checkInDataLayer.GetCheckinsByHelpdeskId(id);

                foreach(Queueitem queueitem in queueItems)
                {
                    queueitem.TimeRemoved = DateTime.Now;
                }

                foreach (Checkinhistory item in checkIns)
                {
                    item.ForcedCheckout = true;
                    item.CheckoutTime = DateTime.Now;
                }

                _helpdeskDataLayer.Save();
                response.Status = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                s_logger.Error(ex, "Unable to force checkout and remove queue items for Helpdesk " + id);
                response.Status = HttpStatusCode.InternalServerError;
                response.StatusMessages.Add(new StatusMessage(HttpStatusCode.InternalServerError, "Unable to force checkout and remove queue items for Helpdesk " + id));
            }

            return response;
        }

        /// <summary>
        /// This method is responsible for handling the deletion of a timespan from the system
        /// </summary>
        /// <param name="id">The SpanID of the timespan to be deleted</param>
        /// <returns>A response indicating success or failure</returns>
        public DeleteTimeSpanResponse DeleteTimeSpan(int id)
        {
            DeleteTimeSpanResponse response = new DeleteTimeSpanResponse();

            try
            {
                var timespan = _helpdeskDataLayer.GetTimeSpan(id);
                if (timespan == null)
                {
                    response.Status = HttpStatusCode.BadRequest;
                    response.StatusMessages.Add(new StatusMessage(HttpStatusCode.BadRequest, "Unable to find timespan"));
                }
                else
                {
                    _helpdeskDataLayer.DeleteTimeSpan(timespan);
                    _helpdeskDataLayer.Save();
                    response.Status = HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                s_logger.Error(ex, "Unable to delete the timespan.");
                response.Status = HttpStatusCode.InternalServerError;
            }
            return response;
        }

        /// <summary>
        /// Used to convert a helpdesk DAO to DTO
        /// </summary>
        /// <param name="helpdesk">The DAO to be converted</param>
        /// <returns>The resulting DTO</returns>
        public HelpdeskDTO DAO2DTO(Helpdesksettings helpdesk)
        {
            HelpdeskDTO helpdeskDTO = new HelpdeskDTO()
            {
                HelpdeskID = helpdesk.HelpdeskId,
                Name = helpdesk.Name,
                HasCheckIn = helpdesk.HasCheckIn,
                HasQueue = helpdesk.HasQueue,
                IsDisabled = helpdesk.IsDeleted
            };

            return helpdeskDTO;
        }

        /// <summary>
        /// Used to convert a helpdesk DTO to DAO
        /// </summary>
        /// <param name="helpdeskDTO">The DTO to be converted</param>
        /// <returns>The resulting DAO</returns>
        public Helpdesksettings DTO2DAO(HelpdeskDTO helpdeskDTO)
        {
            Helpdesksettings helpdesk = new Helpdesksettings()
            {
                HelpdeskId = helpdeskDTO.HelpdeskID,
                Name = helpdeskDTO.Name,
                HasCheckIn = helpdeskDTO.HasCheckIn,
                HasQueue = helpdeskDTO.HasQueue,
                IsDeleted = helpdeskDTO.IsDisabled
            };

            return helpdesk;
        }

        /// <summary>
        /// Converts the timespan DAO to a DTO to send to the front end
        /// </summary>
        /// <param name="timespan">The DAO for the timespan</param>
        /// <returns>The DTO for the timespan</returns>
        private TimeSpanDTO timespanDAO2DTO(Timespans timespan)
        {
            TimeSpanDTO timespanDTO = new TimeSpanDTO
            {
                SpanId = timespan.SpanId,
                HelpdeskId = timespan.HelpdeskId,
                Name = timespan.Name,
                StartDate = timespan.StartDate,
                EndDate = timespan.EndDate
            };

            return timespanDTO;
        }

        /// <summary>
        /// Converts the timespan DTO to a DAO to interact with the database
        /// </summary>
        /// <param name="timespanDTO">The DTO for the timespan</param>
        /// <returns>The DAO for the timespan</returns>
        private Timespans timespanDTO2DAO(TimeSpanDTO timespanDTO)
        {
            Timespans timespan = new Timespans
            {
                SpanId = timespanDTO.SpanId,
                HelpdeskId = timespanDTO.HelpdeskId,
                Name = timespanDTO.Name,
                StartDate = timespanDTO.StartDate,
                EndDate = timespanDTO.EndDate
            };

            return timespan;
        }
    }
}
