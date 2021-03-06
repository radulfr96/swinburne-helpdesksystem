﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Helpdesk.Common.Requests.Students;
using Helpdesk.Common.Responses.Students;
using Helpdesk.Data.Models;
using Helpdesk.DataLayer;
using Helpdesk.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Helpdesk.Website.Controllers.api
{
    /// <summary>
    /// Used as the access point for any features relating to students
    /// </summary>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/student")]
    [ApiController]
    public class StudentController : BaseApiController
    {

        public StudentController(helpdesksystemContext _context) : base (_context) { }

        /// <summary>
        /// Used to get all of the student nicknames
        /// </summary>
        /// <returns>The response with the nickname list</returns>
        [HttpGet]
        [Route("")]
        public IActionResult GetAllNicknames()
        {
            if (!IsAuthorized())
                return Unauthorized();

            try
            {
                var response = new GetAllNicknamesResponse();

                using (StudentDataLayer studentDataLayer = new StudentDataLayer(context))
                {
                    var facade = new StudentFacade(new StudentDataLayer(context));
                    response = facade.GetAllNicknames();
                }

                switch (response.Status)
                {
                    case HttpStatusCode.OK:
                        return Ok(response);
                    case HttpStatusCode.BadRequest:
                        return BadRequest(BuildBadRequestMessage(response));
                    case HttpStatusCode.InternalServerError:
                        return StatusCode(StatusCodes.Status500InternalServerError);
                }
                s_logger.Fatal("This code should be unreachable, unknown result has occured.");
            }
            catch (Exception ex)
            {
                s_logger.Error(ex, "Unable to get student by nickname.");
            }
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        /// <summary>
        /// Gets a student by their nickname.
        /// </summary>
        /// <param name="nickname"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("nickname")]
        public IActionResult GetStudentByNickname([FromRoute] string nickname)
        {
            if (!IsAuthorized())
                return Unauthorized();

            try
            {
                var response = new GetStudentResponse();
                using (StudentDataLayer studentDataLayer = new StudentDataLayer(context))
                {
                    var facade = new StudentFacade(new StudentDataLayer(context));
                    response = facade.GetStudentByNickname(nickname);
                }

                switch (response.Status)
                {
                    case HttpStatusCode.OK:
                        return Ok(response);
                    case HttpStatusCode.BadRequest:
                        return BadRequest(BuildBadRequestMessage(response));
                    case HttpStatusCode.InternalServerError:
                        return StatusCode(StatusCodes.Status500InternalServerError);
                }
                s_logger.Fatal("This code should be unreachable, unknown result has occured.");
            }
            catch (Exception ex)
            {
                s_logger.Error(ex, "Unable to get student by nickname.");
            }
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        /// <summary>
        /// Edits a specific student's nickname with the given information
        /// </summary>
        /// <param name="id">ID of the student to be updated</param>
        /// <param name="request">Request that contains the new nickname</param>
        /// <returns>A response which indicates success or failure</returns>
        [HttpPatch]
        [Route("nickname")]
        public IActionResult EditStudentNickname([FromBody] EditStudentNicknameRequest request)
        {
            if (!IsAuthorized())
                return Unauthorized();

            try
            {
                var response = new EditStudentNicknameResponse();

                using (StudentDataLayer studentDataLayer = new StudentDataLayer(context))
                {
                    var facade = new StudentFacade(new StudentDataLayer(context));
                    response = facade.EditStudentNickname(request);
                }

                switch (response.Status)
                {
                    case HttpStatusCode.OK:
                        return Ok(response);
                    case HttpStatusCode.BadRequest:
                        return BadRequest(BuildBadRequestMessage(response));
                    case HttpStatusCode.InternalServerError:
                        return StatusCode(StatusCodes.Status500InternalServerError);
                    case HttpStatusCode.NotFound:
                        return NotFound();
                }
                s_logger.Fatal("This code should be unreachable, unknown result has occured.");
            }
            catch (Exception ex)
            {
                s_logger.Error(ex, "Unable to edit student's nickname.");
            }
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        /// <summary>
        /// Used to validate a student's nickname
        /// </summary>
        /// <param name="request">Request that contains the nickname information</param>
        /// <returns>Response which indicates success or failure</returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("validate")]
        public IActionResult ValidateNickname([FromBody] ValidateNicknameRequest request)
        {
            try
            {
                var response = new ValidateNicknameResponse();
                using (StudentDataLayer studentDataLayer = new StudentDataLayer(context))
                {
                    var facade = new StudentFacade(new StudentDataLayer(context));
                    response = facade.ValidateNickname(request);
                }

                switch (response.Status)
                {
                    case HttpStatusCode.OK:
                        return Ok(response);
                    case HttpStatusCode.Accepted:
                        return Accepted(response);
                    case HttpStatusCode.BadRequest:
                        return BadRequest(BuildBadRequestMessage(response));
                    case HttpStatusCode.InternalServerError:
                        return StatusCode(StatusCodes.Status500InternalServerError);
                    case HttpStatusCode.NotFound:
                        return NotFound();
                }
                s_logger.Fatal("This code should be unreachable, unknown result has occured.");
            }
            catch (Exception ex)
            {
                s_logger.Error(ex, "Unable to validate student's nickname.");
            }
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        /// <summary>
        /// Used as an endpoint to generate nickname
        /// </summary>
        /// <returns>The response that indicates success</returns>
        [HttpGet]
        [Route("generate")]
        public IActionResult GenerateNickname()
        {
            try
            {
                var response = new GenerateNicknameResponse();

                using (StudentDataLayer studentDataLayer = new StudentDataLayer(context))
                {
                    var facade = new StudentFacade(new StudentDataLayer(context));
                    response = facade.GenerateNickname();
                }


                switch (response.Status)
                {
                    case HttpStatusCode.OK:
                        return Ok(response);
                    case HttpStatusCode.Accepted:
                        return Accepted(response);
                    case HttpStatusCode.BadRequest:
                        return BadRequest(BuildBadRequestMessage(response));
                    case HttpStatusCode.InternalServerError:
                        return StatusCode(StatusCodes.Status500InternalServerError);
                    case HttpStatusCode.NotFound:
                        return NotFound();
                }
                s_logger.Fatal("This code should be unreachable, unknown result has occured.");
            }
            catch (Exception ex)
            {
                s_logger.Error(ex, "Unable to generate nickname.");
            }
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
