using Helpdesk.Common.DTOs;
using Helpdesk.Common.Requests.Helpdesk;
using System;
using Helpdesk.Data.Models;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Helpdesk.Common.Extensions;
using System.Data;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using Helpdesk.DataLayer.Contracts;

namespace Helpdesk.DataLayer
{

    public class HelpdeskDataLayer : IHelpdeskDataLayer, IDisposable
    {
        private helpdesksystemContext context;

        public HelpdeskDataLayer()
        {
            context = new helpdesksystemContext();
        }

        public void AddHelpdesk(Helpdesksettings helpdesk)
        {
            context.Add(helpdesk);
        }

        public Helpdesksettings GetHelpdesk(int id)
        {
            return context.Helpdesksettings.FirstOrDefault(h => h.HelpdeskId == id);
        }

        public List<Helpdesksettings> GetHelpdesks()
        {
            return context.Helpdesksettings.OrderBy(h => h.IsDeleted).OrderBy(h => h.Name).ToList();
        }

        public DataTable GetHelpdesksAsDataTable()
        {
            DataTable helpdesks = new DataTable();
            DbConnection conn = context.Database.GetDbConnection();
            ConnectionState state = conn.State;

            try
            {
                if (state != ConnectionState.Open)
                    conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "GetAllHelpdesks";
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        helpdesks.Load(reader);
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
            return helpdesks;
        }

        public DataTable GetHelpdeskUnitsAsDataTable()
        {
            DataTable helpdeskunits = new DataTable();

            DbConnection conn = context.Database.GetDbConnection();
            ConnectionState state = conn.State;

            try
            {
                if (state != ConnectionState.Open)
                    conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "GetAllHelpdeskUnits";
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        helpdeskunits.Load(reader);
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
            return helpdeskunits;
        }

        public DataTable GetTimeSpansAsDataTable()
        {
            DataTable timespans = new DataTable();

            DbConnection conn = context.Database.GetDbConnection();
            ConnectionState state = conn.State;

            try
            {
                if (state != ConnectionState.Open)
                    conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "GetAllTimespans";
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        timespans.Load(reader);
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
            return timespans;
        }

        public List<Helpdesksettings> GetActiveHelpdesks()
        {
            return context.Helpdesksettings.Where(h => !h.IsDeleted).OrderBy(h => h.Name).ToList();
        }

        public void AddTimeSpan(Timespans timespan)
        {
            context.Timespans.Add(timespan);
        }

        /// <summary>
        /// Used to retreve a timespan by its id
        /// </summary>
        /// <param name="id">The id of the timespan</param>
        /// <returns>The timespan DTO</returns>
        public Timespans GetTimeSpan(int id)
        {
            return context.Timespans.FirstOrDefault(t => t.SpanId == id);
        }

        public Timespans GetTimeSpanByName(string name)
        {
            return context.Timespans.FirstOrDefault(t => t.Name == name);
        }

        public List<Timespans> GetTimeSpans()
        {
            return context.Timespans.ToList();
        }

        public bool DeleteTimeSpan(Timespans timespan)
        {
            context.Timespans.Remove(timespan);
            return true;
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
