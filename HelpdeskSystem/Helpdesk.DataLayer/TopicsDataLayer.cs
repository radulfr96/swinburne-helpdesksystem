using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Helpdesk.Common.DTOs;
using Helpdesk.Data.Models;
using Helpdesk.DataLayer.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NLog;

namespace Helpdesk.DataLayer
{
    public class TopicsDataLayer : ITopicsDataLayer, IDisposable
    {
        private static Logger s_Logger = LogManager.GetCurrentClassLogger();
        private helpdesksystemContext context;

        public TopicsDataLayer()
        {
            context = new helpdesksystemContext();
        }

        public void AddTopic(Topic topic)
        {
            context.Topic.Add(topic);
        }

        public Topic GetTopic(int id)
        {
            return context.Topic.FirstOrDefault(t => t.TopicId == id);
        }

        public List<Topic> GetTopicsByUnitID(int id)
        {
            List<Topic> topics = new List<Topic>();
            var unitTopics = context.Topic.Where(ut => ut.UnitId == id).ToList();

            foreach (Topic unitTopic in unitTopics)
            {
                topics.Add(unitTopic);
            }

            return topics;
        }

        public DataTable GetTopicsAsDataTable()
        {
            DataTable topics = new DataTable();
            DbConnection conn = context.Database.GetDbConnection();
            ConnectionState state = conn.State;

            try
            {
                if (state != ConnectionState.Open)
                    conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "GetAllTopics";
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        topics.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (state != ConnectionState.Closed)
                    conn.Close();
            }
            return topics;
        }

        public void DeleteTopic(Topic topic)
        {
            context.Topic.Remove(topic);
        }

        public void Dispose()
        {
            context.Dispose();
        }

        public void Save()
        {
            context.SaveChanges();
        }
    }
}
