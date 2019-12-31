using Helpdesk.Common.DTOs;
using Helpdesk.Common.Requests;
using System;
using System.Collections.Generic;
using System.Text;
using Helpdesk.Data.Models;
using Helpdesk.Common.Requests.Queue;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;
using Helpdesk.DataLayer.Contracts;
using Microsoft.EntityFrameworkCore.Storage;

namespace Helpdesk.DataLayer
{
    public class QueueDataLayer : IQueueDataLayer, IDisposable
    {
        private helpdesksystemContext context;

        public QueueDataLayer(helpdesksystemContext _context)
        {
            context = _context;
        }

        public void AddToQueue(Queueitem item)
        {
            context.Queueitem.Add(item);
        }

        public List<Queueitem> GetQueueItemsByHelpdeskID(int id)
        {
            List<QueueItemDTO> queueItemDTOs = new List<QueueItemDTO>();

            var unitIDs = context.Helpdeskunit.Include("Helpdeskunit").Where(hu => hu.HelpdeskId == id).Select(u => u.UnitId);
            var topicIDs = context.Topic.Where(t => unitIDs.Contains(t.UnitId)).Select(ti => ti.TopicId).ToList();
            return context.Queueitem.Include("Topic.Unit").Include("Student").Where(qi => topicIDs.Contains(qi.TopicId)).ToList();
        }

        public List<Queueitem> GetQueueItemsByCheckIn(int checkInId)
        {
            List<Queueitem> queueItems = new List<Queueitem>();

            var itemIds = context.Checkinqueueitem.Where(cq => cq.CheckInId == checkInId).Select(cq => cq.QueueItemId);

            foreach (int id in itemIds)
            {
                var item = context.Queueitem.Include("Topic.Unit").Include("Student").Where(i => i.ItemId == id).FirstOrDefault();

                if (item != null && item.TimeRemoved == null)
                {
                    queueItems.Add(item);
                }
            }

            return queueItems;
        }

        public Queueitem GetQueueitem(int id)
        {
            return context.Queueitem.FirstOrDefault(q => q.ItemId == id);
        }

        public DataTable GetQueueItemsAsDataTable()
        {
            DataTable queueItems = new DataTable();
            DbConnection conn = context.Database.GetDbConnection();
            ConnectionState state = conn.State;

            try
            {
                if (state != ConnectionState.Open)
                    conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "GetAllQueueItems";
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        queueItems.Load(reader);
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
            return queueItems;
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

        public IDbContextTransaction GetTransaction()
        {
            return context.Database.BeginTransaction();
        }

        public void Save()
        {
            context.SaveChanges();
        }

        public void Dispose()
        {
            context.Dispose();
        }

    }
}
