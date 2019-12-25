using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Helpdesk.Common;
using Helpdesk.Common.DTOs;
using Helpdesk.Common.Extensions;
using Helpdesk.Common.Requests.Helpdesk;
using Helpdesk.Common.Requests.Units;
using Helpdesk.Common.Responses;
using Helpdesk.Common.Responses.Units;
using Helpdesk.Data.Models;
using Helpdesk.DataLayer;
using Helpdesk.DataLayer.Contracts;
using Microsoft.EntityFrameworkCore.Storage;
using NLog;

namespace Helpdesk.Services
{
    /// <summary>
    /// This class is used to handle the business logic of units
    /// </summary>
    public class UnitsFacade : ILoginClass
    {
        private static Logger s_logger = LogManager.GetCurrentClassLogger();

        private IUnitsDataLayer _unitsDataLayer;
        private ITopicsDataLayer _topicsDataLayer;

        public UnitsFacade(IUnitsDataLayer unitsDataLayer, ITopicsDataLayer topicsDataLayer)
        {
            _unitsDataLayer = unitsDataLayer;
            _topicsDataLayer = topicsDataLayer;
        }

        /// <summary>
        /// This method is used to add a new user or update an existing user
        /// </summary>
        /// <param name="id">The id of the user to be updated if requested</param>
        /// <param name="request">Request that contains the new user information</param>
        /// <returns>Response which indicates success or failure</returns>
        public AddUpdateUnitResponse AddOrUpdateUnit(AddUpdateUnitRequest request)
        {
            s_logger.Info("Adding unit to helpdesk");

            var response = new AddUpdateUnitResponse();

            try
            {
                response = (AddUpdateUnitResponse)request.CheckValidation(response);

                if (response.Status == HttpStatusCode.BadRequest)
                    return response;

                if (request.UnitID == 0)
                {

                    Unit existingUnit = _unitsDataLayer.GetUnitByNameAndHelpdeskId(request.Name, request.HelpdeskID);

                    if (existingUnit != null)
                    {
                        response.Status = HttpStatusCode.BadRequest;
                        response.StatusMessages.Add(new StatusMessage(HttpStatusCode.BadRequest, "Subject with that name already exists."));
                        return response;
                    }

                    existingUnit = _unitsDataLayer.GetUnitByCodeAndHelpdeskId(request.Code, request.HelpdeskID);

                    if (existingUnit != null)
                    {
                        response.Status = HttpStatusCode.BadRequest;
                        response.StatusMessages.Add(new StatusMessage(HttpStatusCode.BadRequest, "Subject with that code already exists."));
                        return response;
                    }

                    using (IDbContextTransaction trans = _unitsDataLayer.GetTransaction())
                    {
                        try
                        {

                            Unit unit = new Unit()
                            {
                                Code = request.Code,
                                Name = request.Name,
                                IsDeleted = request.IsDeleted,
                            };

                            _unitsDataLayer.AddUnit(unit);
                            _unitsDataLayer.Save();

                            if (unit.UnitId == 0)
                            {
                                response.Status = HttpStatusCode.InternalServerError;
                                response.StatusMessages.Add(new StatusMessage(HttpStatusCode.InternalServerError, "Unable to add unit, unknown error has occured."));
                            }

                            _unitsDataLayer.AddHelpdeskUnit(new Helpdeskunit()
                            {
                                HelpdeskId = request.HelpdeskID,
                                UnitId = unit.UnitId
                            });


                            foreach (string topic in request.Topics)
                            {
                                unit.Topic.Add(new Topic()
                                {
                                    IsDeleted = false,
                                    Name = topic,
                                    UnitId = unit.UnitId
                                });
                            }

                            _unitsDataLayer.Save();

                            response.UnitID = unit.UnitId;

                            trans.Commit();
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            throw ex;
                        }
                    }
                }
                else
                {
                    var existingUnit = _unitsDataLayer.GetUnit(request.UnitID);

                    if (existingUnit == null)
                    {
                        response.Status = HttpStatusCode.NotFound;
                        response.StatusMessages.Add(new StatusMessage(HttpStatusCode.NotFound, "Unable to find unit"));
                        return response;
                    }

                    Unit existingUnitWithName = _unitsDataLayer.GetUnitByNameAndHelpdeskId(request.Name, request.HelpdeskID);

                    if (existingUnitWithName != null && existingUnit.UnitId != request.UnitID)
                    {
                        response.Status = HttpStatusCode.BadRequest;
                        response.StatusMessages.Add(new StatusMessage(HttpStatusCode.BadRequest, "Unit with that name already exists"));
                        return response;
                    }

                    Unit existingUnitCode = _unitsDataLayer.GetUnitByCodeAndHelpdeskId(request.Code, request.HelpdeskID);

                    if (existingUnitCode != null && existingUnit.UnitId != request.UnitID)
                    {
                        response.Status = HttpStatusCode.BadRequest;
                        response.StatusMessages.Add(new StatusMessage(HttpStatusCode.BadRequest, "Unit with that code already exists"));
                        return response;
                    }

                    using (IDbContextTransaction trans = _unitsDataLayer.GetTransaction())
                    {
                        try
                        {
                            existingUnit.IsDeleted = request.IsDeleted;
                            existingUnit.Name = request.Name;
                            existingUnit.Code = request.Code;

                            _unitsDataLayer.Save();

                            foreach (Topic topic in existingUnit.Topic)
                            {
                                if (request.Topics.Contains(topic.Name))
                                {
                                    var realTopic = _topicsDataLayer.GetTopic(topic.TopicId);
                                    realTopic.IsDeleted = false;
                                }
                                else
                                {
                                    var realTopic = _topicsDataLayer.GetTopic(topic.TopicId);
                                    realTopic.IsDeleted = true;
                                }
                                _topicsDataLayer.Save();
                            }

                            foreach (string topic in request.Topics)
                            {
                                if (!existingUnit.Topic.Select(t => t.Name).Contains(topic))
                                {
                                    _topicsDataLayer.AddTopic(new Topic()
                                    {
                                        UnitId = request.UnitID,
                                        IsDeleted = false,
                                        Name = topic,
                                    });
                                }
                                _topicsDataLayer.Save();
                            }

                            response.UnitID = request.UnitID;
                            trans.Commit();
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            throw ex;
                        }
                    }
                }
                response.Status = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                s_logger.Error(ex, "Unable to add unit to system");
                response.Status = HttpStatusCode.InternalServerError;
                response.StatusMessages = new List<StatusMessage>();
            }

            return response;
        }

        /// <summary>
        /// Attempt to retrieve a unit from the database matching the provided id.
        /// </summary>
        /// <param name="id">The id of the unit to retrieve from the database.</param>
        /// <returns></returns>
        public GetUnitResponse GetUnit(int id)
        {
            s_logger.Info("Getting unit by id...");

            var response = new GetUnitResponse();

            try
            {
                Unit unit = _unitsDataLayer.GetUnit(id);

                if (unit == null)
                {
                    response.Status = HttpStatusCode.NotFound;
                    response.StatusMessages.Add(new StatusMessage(HttpStatusCode.NotFound, "Unit not found"));
                }
                else
                {
                    response.Unit = DAO2DTO(unit);
                    response.Status = HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                s_logger.Error(ex, "Unable to get unit!");
                response.Status = HttpStatusCode.InternalServerError;
                response.StatusMessages.Add(new StatusMessage(HttpStatusCode.InternalServerError, "Unable to get unit!"));
            }
            return response;
        }

        /// <summary>
        /// Attempts to retrieve all units under a specific helpdesk id from the helpdesk system
        /// </summary>
        /// <param name="id">ID of the helpdesk to be retrieved from</param>
        /// <returns>A response containing the list of units and status code representing the result</returns>
        public GetUnitsByHelpdeskIDResponse GetUnitsByHelpdeskID(int id, bool getActive)
        {
            s_logger.Info("Getting units by helpdesk id...");

            var response = new GetUnitsByHelpdeskIDResponse();

            try
            {
                List<Unit> units = _unitsDataLayer.GetUnitsByHelpdeskID(id, getActive);

                if (units.Count == 0)
                {
                    response.Status = HttpStatusCode.NotFound;
                    response.StatusMessages.Add(new StatusMessage(HttpStatusCode.NotFound, "No units found"));
                }
                else
                {
                    foreach (Unit unit in units)
                    {
                        response.Units.Add(DAO2DTO(unit));
                    }
                    response.Status = HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                s_logger.Error(ex, "Unable to get units!");
                response.Status = HttpStatusCode.InternalServerError;
                response.StatusMessages.Add(new StatusMessage(HttpStatusCode.InternalServerError, "Unable to get units!"));
            }

            return response;
        }

        /// <summary>
        /// Attempts to delete a specific unit from the helpdesk system
        /// </summary>
        /// <param name="id">ID of the unit to be deleted</param>
        /// <returns>A response indicating the result of the operation</returns>
        public DeleteUnitResponse DeleteUnit(int id)
        {
            var response = new DeleteUnitResponse();

            try
            {
                var unit = _unitsDataLayer.GetUnit(id);

                if (unit == null)
                {
                    response.Status = HttpStatusCode.NotFound;
                    response.StatusMessages.Add(new StatusMessage(HttpStatusCode.NotFound, "Unable to find user."));
                    return response;
                }

                foreach (Topic topic in unit.Topic)
                {
                    topic.IsDeleted = true;
                    _topicsDataLayer.Save();
                }

                unit.IsDeleted = true;

                _unitsDataLayer.Save();

                response.Status = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                s_logger.Error(ex, "Unable to delete the unit.");
                response.Status = HttpStatusCode.InternalServerError;
            }
            return response;
        }

        /// <summary>
        /// Converts the unit DAO to a DTO to send to the front end
        /// </summary>
        /// <param name="unit">Unit DAO object to be converted.</param>
        /// <returns></returns>
        private UnitDTO DAO2DTO(Unit unit)
        {
            UnitDTO unitDTO = new UnitDTO();
            unitDTO.UnitId = unit.UnitId;
            unitDTO.Code = unit.Code;
            unitDTO.Name = unit.Name;
            unitDTO.IsDeleted = unit.IsDeleted;

            foreach (Topic topic in unit.Topic)
            {
                if (!topic.IsDeleted)
                {
                    unitDTO.Topics.Add(
                        new TopicDTO()
                        {
                            Name = topic.Name,
                            IsDeleted = topic.IsDeleted,
                            TopicId = topic.TopicId,
                            UnitId = topic.UnitId
                        });
                }
            }

            return unitDTO;
        }
    }
}
