using Helpdesk.Common.DTOs;
using Helpdesk.Common.Extensions;
using Helpdesk.Common.Requests.CheckIn;
using Helpdesk.Data.Models;
using Helpdesk.DataLayer.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Helpdesk.DataLayer
{
    public class CheckInDataLayer : ICheckInDataLayer, IDisposable
    {
        private helpdesksystemContext context;

        public CheckInDataLayer()
        {
            context = new helpdesksystemContext();
        }

        public void CheckIn(Checkinhistory checkIn)
        {
            context.Checkinhistory.Add(checkIn);
        }

        public List<Checkinhistory> GetCheckinsByHelpdeskId(int helpdeskId)
        {
            List<CheckInDTO> checkInDTOs = new List<CheckInDTO>();
            var unitIds = context.Helpdeskunit.Where(u => u.HelpdeskId == helpdeskId).Select(u => u.UnitId).ToList();

            return context
                             .Checkinhistory
                             .Include("Student")
                             .Where(c =>
                                 unitIds.Contains(c.UnitId)
                                 && !c.CheckoutTime.HasValue
                                 && (!c.ForcedCheckout.HasValue || !c.ForcedCheckout.Value)
                                 ).ToList();
        }

        public Checkinhistory GetCheckIn(int id)
        {
            return context.Checkinhistory.FirstOrDefault(c => c.CheckInId == id);
        }

        public DataTable GetCheckInsAsDataTable()
        {
            DataTable checkIns = new DataTable();
            DbConnection conn = context.Database.GetDbConnection();
            ConnectionState state = conn.State;

            try
            {
                if (state != ConnectionState.Open)
                    conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "GetAllCheckins";
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        checkIns.Load(reader);
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
            return checkIns;
        }

        public DataTable GetCheckInQueueItemsAsDataTable()
        {
            DataTable checkInQueueItems = new DataTable();
            DbConnection conn = context.Database.GetDbConnection();
            ConnectionState state = conn.State;

            try
            {
                if (state != ConnectionState.Open)
                    conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "GetAllCheckInQueueItems";
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        checkInQueueItems.Load(reader);
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
            return checkInQueueItems;
        }

        public void AddCheckinQueueItem(Checkinqueueitem checkinqueueitem)
        {
            context.Checkinqueueitem.Add(checkinqueueitem);
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
