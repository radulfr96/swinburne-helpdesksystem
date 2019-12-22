using System;
using System.Collections.Generic;
using System.Net;
using Helpdesk.Common;
using Helpdesk.Common.DTOs;
using Helpdesk.Common.Extensions;
using Helpdesk.Common.Responses;
using Helpdesk.Common.Responses.Topics;
using Helpdesk.Data.Models;
using Helpdesk.DataLayer;
using Helpdesk.DataLayer.Contracts;
using Microsoft.Extensions.Logging;
using NLog;

namespace Helpdesk.Services
{
    /// <summary>
    /// This class is used to handle the business logic of topics
    /// </summary>
    public class TopicsFacade : ILoginClass
    {
        private static Logger s_logger = LogManager.GetCurrentClassLogger();

        private ITopicsDataLayer _topicsDataLayer;

        private readonly AppSettings _appSettings;

        public TopicsFacade(ITopicsDataLayer topicsDataLayer)
        {
            _topicsDataLayer = topicsDataLayer;
            _appSettings = new AppSettings();
        }

        /// <summary>
        /// This method is used to get all topics of a specific unit
        /// </summary>
        /// <param name="id">ID of the unit to get topics from</param>
        /// <returns>Response which indicates success or failure</returns>
        public GetTopicsByUnitIDResponse GetTopicsByUnitID(int id)
        {
            var response = new GetTopicsByUnitIDResponse();

            try
            {
                List<Topic> topics = _topicsDataLayer.GetTopicsByUnitID(id);

                if (topics.Count == 0)
                {
                    response.Status = HttpStatusCode.NotFound;
                    response.StatusMessages.Add(new StatusMessage(HttpStatusCode.NotFound, "No topics found."));
                }
                else
                {
                    foreach (Topic topic in topics)
                    {
                        response.Topics.Add(DAO2DTO(topic));
                    }
                    response.Status = HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                s_logger.Error(ex, "Unable to get topics!");
                response.Status = HttpStatusCode.InternalServerError;
                response.StatusMessages.Add(new StatusMessage(HttpStatusCode.InternalServerError, "Unable to get topics!"));
            }
            return response;
        }

        /// <summary>
        /// Converts the topic DAO to a DTO to send to the front end
        /// </summary>
        /// <param name="topic">The DAO for the topic</param>
        /// <returns>The DTO for the topic</returns>
        private TopicDTO DAO2DTO(Topic topic)
        {
            TopicDTO topicDTO = new TopicDTO
            {
                TopicId = topic.TopicId,
                UnitId = topic.UnitId,
                Name = topic.Name,
                IsDeleted = topic.IsDeleted
            };
            return topicDTO;
        }

        /// <summary>
        /// Converts the topic DTO to a DAO to interact with the database
        /// </summary>
        /// <param name="topic">The DTO for the topic</param>
        /// <returns>The DAO for the topic</returns>
        private Topic DTO2DAO(TopicDTO topicDTO)
        {
            Topic topic = new Topic
            {
                TopicId = topicDTO.TopicId,
                UnitId = topicDTO.UnitId,
                Name = topicDTO.Name,
                IsDeleted = topicDTO.IsDeleted
            };
            return topic;
        }
    }
}
