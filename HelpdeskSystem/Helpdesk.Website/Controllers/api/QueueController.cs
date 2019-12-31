﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Helpdesk.Common.Requests.Queue;
using Helpdesk.Common.Responses.Queue;
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
	/// Used as the access point for any features relating to the queue
	/// </summary>
	[Route("api/queue")]
	[ApiController]
	public class QueueController : BaseApiController
	{
		public QueueController(helpdesksystemContext _context) : base (_context) { }

		/// <summary>
		/// Adds a queue item to the queue.
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[HttpPost]
        [Route("")]
		public IActionResult AddToQueue([FromBody] AddToQueueRequest request)
		{
			try
			{
				var response = new AddToQueueResponse();

				using (QueueDataLayer queueDataLayer = new QueueDataLayer(context))
				using (StudentDataLayer studentDataLayer = new StudentDataLayer(context))
				using (CheckInDataLayer checkInDataLayer = new CheckInDataLayer(context))
				using (TopicsDataLayer topicsDataLayer = new TopicsDataLayer(context))
				{
					var facade = new QueueFacade(
						queueDataLayer
						, studentDataLayer
						, checkInDataLayer
						, topicsDataLayer);
					response = facade.AddToQueue(request);
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
				s_logger.Error(ex, "Unable to add queue item.");
			}
			return StatusCode(StatusCodes.Status500InternalServerError);
		}

		/// <summary>
		/// Retrieves queue items from the database by helpdesk id
		/// </summary>
		/// <param name="id">The id of the helpdesk</param>
		/// <returns>Response which indicates success or failure</returns>
		[HttpGet]
		[Route("helpdesk/{id}")]
		public IActionResult GetQueueItemsByHelpdeskID([FromRoute] int id)
		{
			try
			{
				var response = new GetQueueItemsByHelpdeskIDResponse();

				using (QueueDataLayer queueDataLayer = new QueueDataLayer(context))
				using (StudentDataLayer studentDataLayer = new StudentDataLayer(context))
				using (CheckInDataLayer checkInDataLayer = new CheckInDataLayer(context))
				using (TopicsDataLayer topicsDataLayer = new TopicsDataLayer(context))
				{
					var facade = new QueueFacade(
						queueDataLayer
						, studentDataLayer
						, checkInDataLayer
						, topicsDataLayer);
					response = facade.GetQueueItemsByHelpdeskID(id);
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
				s_logger.Error(ex, "Unable to get queue items.");
			}
			return StatusCode(StatusCodes.Status500InternalServerError);
		}

		/// <summary>
		/// Updates a queue item (Topic).
		/// </summary>
		/// <param name="id"></param>
		/// <param name="request"></param>
		/// <returns></returns>
		[HttpPatch]
		[Route("")]
		public IActionResult UpdateQueueItem([FromBody] UpdateQueueItemRequest request)
		{
			try
			{
				var response = new UpdateQueueItemResponse();
				using (QueueDataLayer queueDataLayer = new QueueDataLayer(context))
				using (StudentDataLayer studentDataLayer = new StudentDataLayer(context))
				using (CheckInDataLayer checkInDataLayer = new CheckInDataLayer(context))
				using (TopicsDataLayer topicsDataLayer = new TopicsDataLayer(context))
				{
					var facade = new QueueFacade (
						queueDataLayer
						, studentDataLayer
						, checkInDataLayer
						, topicsDataLayer
					);
					response = facade.UpdateQueueItem(request);
				}

				switch (response.Status)
				{
					case HttpStatusCode.OK:
						return Ok();
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
				s_logger.Error(ex, "Unable to update queue item.");
			}
			return StatusCode(StatusCodes.Status500InternalServerError);
		}

		/// <summary>
		/// Updates a queue item status (TimeHelped and TimeRemoved DateTimes).
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[HttpPatch]
		[Route("UpdateQueueItemStatus")]
		public IActionResult UpdateUpdateQueueItemStatus([FromBody] UpdateQueueItemStatusRequest request)
		{
			try
			{
				var response = new UpdateQueueItemStatusResponse();

				using (QueueDataLayer queueDataLayer = new QueueDataLayer(context))
				using (StudentDataLayer studentDataLayer = new StudentDataLayer(context))
				using (CheckInDataLayer checkInDataLayer = new CheckInDataLayer(context))
				using (TopicsDataLayer topicsDataLayer = new TopicsDataLayer(context))
				{
					var facade = new QueueFacade(
						queueDataLayer
						, studentDataLayer
						, checkInDataLayer
						, topicsDataLayer
					);
					response = facade.UpdateQueueItemStatus(request);
				}

				switch (response.Status)
				{
					case HttpStatusCode.OK:
						return Ok();
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
				s_logger.Error(ex, "Unable to update queue item status.");
			}
			return StatusCode(StatusCodes.Status500InternalServerError);
		}
	}
}
