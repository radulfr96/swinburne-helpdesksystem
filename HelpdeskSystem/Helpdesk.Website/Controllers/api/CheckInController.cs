﻿using Helpdesk.Common.Requests.CheckIn;
using Helpdesk.Common.Responses.CheckIn;
using Helpdesk.DataLayer;
using Helpdesk.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Helpdesk.Website.Controllers.api
{
    /// <summary>
    /// Used as the access point for any features relating to checking in
    /// </summary>
    [Route("api/checkin")]
    [ApiController]
    public class CheckInController : BaseApiController
    {
        /// <summary>
        /// Checks into the database
        /// </summary>
        /// <param name="request">Request containing the specific unit to be associated with the check in</param>
        /// <returns>A response indicating success or failure</returns>
        [HttpPost]
        [Route("")]
        public IActionResult CheckIn([FromBody] CheckInRequest request)
        {
            try
            {
                CheckInResponse response = new CheckInResponse();

                using (CheckInDataLayer checkInDataLayer = new CheckInDataLayer())
                using (StudentDataLayer studentDataLayer = new StudentDataLayer())
                using (QueueDataLayer queueDataLayer = new QueueDataLayer())
                {
                    var facade = new CheckInFacade(checkInDataLayer, studentDataLayer, queueDataLayer);
                    response = facade.CheckIn(request);

                }

                switch (response.Status)
                {
                    case HttpStatusCode.OK:
                        return Ok(response);
                    case HttpStatusCode.BadRequest:
                        return BadRequest(BuildBadRequestMessage(response));
                    case HttpStatusCode.NotFound:
                        return NotFound();
                    case HttpStatusCode.InternalServerError:
                        return StatusCode(StatusCodes.Status500InternalServerError);
                }
                s_logger.Fatal("This code should be unreachable, unknown result has occured.");
            }
            catch (Exception ex)
            {
                s_logger.Error(ex, "Unable to check in.");
            }
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        /// <summary>
        /// Checks out of the database
        /// </summary>
        /// <param name="request">Request containing the specific CheckInID to be associated with checking out</param>
        /// <returns>A response indicating success or failure</returns>
        [HttpPatch]
        [Route("")]
        public IActionResult CheckOut([FromBody] CheckOutRequest request)
        {
            try
            {
                CheckOutResponse response = new CheckOutResponse();

                using (CheckInDataLayer checkInDataLayer = new CheckInDataLayer())
                using (StudentDataLayer studentDataLayer = new StudentDataLayer())
                using (QueueDataLayer queueDataLayer = new QueueDataLayer())
                {
                    var facade = new CheckInFacade(new CheckInDataLayer(), new StudentDataLayer(), new QueueDataLayer());
                    response = facade.CheckOut(request);
                }

                switch (response.Status)
                {
                    case HttpStatusCode.OK:
                        return Ok(response);
                    case HttpStatusCode.BadRequest:
                        return BadRequest(BuildBadRequestMessage(response));
                    case HttpStatusCode.NotFound:
                        return NotFound();
                    case HttpStatusCode.InternalServerError:
                        return StatusCode(StatusCodes.Status500InternalServerError);
                }
                s_logger.Fatal("This code should be unreachable, unknown result has occured.");
            }
            catch (Exception ex)
            {
                s_logger.Error(ex, "Unable to check out.");
            }
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        /// <summary>
        /// Used to get the check ins for a helpdesk
        /// </summary>
        /// <param name="id">The id of the helpdesk</param>
        /// <returns>A response with the list of check ins and success indications</returns>
        [HttpGet]
        [Route("{id}")]
        public IActionResult GetCheckInsByHelpdeskID([FromRoute] int id)
        {
            try
            {
                GetCheckInsResponse response = new GetCheckInsResponse();

                using (CheckInDataLayer checkInDataLayer = new CheckInDataLayer())
                using (StudentDataLayer studentDataLayer = new StudentDataLayer())
                using (QueueDataLayer queueDataLayer = new QueueDataLayer())
                {
                    var facade = new CheckInFacade(new CheckInDataLayer(), new StudentDataLayer(), new QueueDataLayer());
                    response = facade.GetCheckInsByHelpdeskId(id);
                }

                switch (response.Status)
                {
                    case HttpStatusCode.OK:
                        return Ok(response);
                    case HttpStatusCode.BadRequest:
                        return BadRequest(BuildBadRequestMessage(response));
                    case HttpStatusCode.NotFound:
                        return NotFound();
                    case HttpStatusCode.InternalServerError:
                        return StatusCode(StatusCodes.Status500InternalServerError);
                }
                s_logger.Fatal("This code should be unreachable, unknown result has occured.");
            }
            catch (Exception ex)
            {
                s_logger.Error(ex, "Unable to get check ins.");
            }
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
