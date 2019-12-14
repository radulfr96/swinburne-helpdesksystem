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

    public class HelpdeskDataLayer : IHelpdeskDataLayer
    {
        public int? AddHelpdesk(AddHelpdeskRequest request)
        {
            int? helpdeskId = null;

            Helpdesksettings helpdesk = new Helpdesksettings();
            helpdesk.Name = request.Name;
            helpdesk.HasCheckIn = request.HasCheckIn;
            helpdesk.HasQueue = request.HasQueue;

            using (helpdesksystemContext context = new helpdesksystemContext())
            {
                context.Add(helpdesk);
                context.SaveChanges();
                helpdeskId = helpdesk.HelpdeskId;
            }

            return helpdeskId;
        }
        public HelpdeskDTO GetHelpdesk(int id)
        {
            HelpdeskDTO helpdeskDTO = null;

            using (helpdesksystemContext context = new helpdesksystemContext())
            {
                var helpdesk = context.Helpdesksettings.FirstOrDefault(h => h.HelpdeskId == id);

                if (helpdesk == null)
                    throw new NotFoundException("Helpdesk does not exist");

                helpdeskDTO = DAO2DTO(helpdesk);
            }
                return helpdeskDTO;
        }

        public List<HelpdeskDTO> GetHelpdesks()
        {
            List<HelpdeskDTO> helpdeskDTOs = new List<HelpdeskDTO>();

            using (helpdesksystemContext context = new helpdesksystemContext())
            {
                var helpdesks = context.Helpdesksettings.OrderBy(h => h.IsDeleted).OrderBy(h => h.Name).ToList();

                if (helpdesks.Count == 0)
                    throw new NotFoundException("No helpdesks found");

                foreach (var helpdesk in helpdesks)
                    helpdeskDTOs.Add(DAO2DTO(helpdesk));
            }
            return helpdeskDTOs;
        }

        public DataTable GetHelpdesksAsDataTable()
        {
            DataTable helpdesks = new DataTable();

            using (helpdesksystemContext context = new helpdesksystemContext())
            {
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
            }

            return helpdesks;
        }

        public DataTable GetHelpdeskUnitsAsDataTable()
        {
            DataTable helpdeskunits = new DataTable();

            using (helpdesksystemContext context = new helpdesksystemContext())
            {
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
            }

            return helpdeskunits;
        }

        public DataTable GetTimeSpansAsDataTable()
        {
            DataTable timespans = new DataTable();

            using (helpdesksystemContext context = new helpdesksystemContext())
            {
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
            }

            return timespans;
        }

        public List<HelpdeskDTO> GetActiveHelpdesks()
        {
            List<HelpdeskDTO> helpdeskDTOs = new List<HelpdeskDTO>();

            using (helpdesksystemContext context = new helpdesksystemContext())
            {
                var helpdesks = context.Helpdesksettings.Where(h => !h.IsDeleted).OrderBy(h => h.Name).ToList();

                if (helpdesks.Count == 0)
                    throw new NotFoundException("No helpdesks found");

                foreach (var helpdesk in helpdesks)
                    helpdeskDTOs.Add(DAO2DTO(helpdesk));
            }
            return helpdeskDTOs;
        }

        public bool UpdateHelpdesk(UpdateHelpdeskRequest request)
        {
            bool result = false;

            using (helpdesksystemContext context = new helpdesksystemContext())
            {
                Helpdesksettings helpdesk = context.Helpdesksettings.FirstOrDefault(p => p.HelpdeskId == request.HelpdeskID);

                if (helpdesk == null)
                    throw new NotFoundException($"Helpdesk with id [{request.HelpdeskID}] not found.");

                helpdesk.Name = request.Name;
                helpdesk.HasCheckIn = request.HasCheckIn;
                helpdesk.HasQueue = request.HasQueue;

                context.SaveChanges();
                result = true;
            }

            return result;
        }

        public bool ForceCheckoutQueueRemove(int id)
        {
            using (helpdesksystemContext context = new helpdesksystemContext())
            {
                DateTime time = DateTime.Now;
                
                var unitIds = context.Helpdeskunit.Where(hu => hu.HelpdeskId == id).Select(hu => hu.UnitId).ToList();

                foreach(int unitId in unitIds)
                {
                    List<Checkinhistory> checkins = context.Checkinhistory.Where(c => c.CheckoutTime == null && c.UnitId == unitId).ToList();
                    foreach (Checkinhistory checkin in checkins)
                    {
                        if (checkin.CheckoutTime == null)
                        {
                            checkin.CheckoutTime = time;
                            checkin.ForcedCheckout = true;
                        }
                    }

                    var topicIds = context.Topic.Where(t => t.UnitId == unitId).Select(t => t.TopicId).ToList();

                    foreach (int topicId in topicIds)
                    {
                        List<Queueitem> queueItems = context.Queueitem.Where(q => q.TimeRemoved == null && q.TopicId == topicId).ToList();
                        foreach (Queueitem queueItem in queueItems)
                        {
                            if (queueItem.TimeRemoved == null)
                            {
                                queueItem.TimeRemoved = time;
                            }
                        }
                    }
                }
                context.SaveChanges();
            }
            return true;
        }

        public int AddTimeSpan(AddTimeSpanRequest request)
        {
            int spanId;

            Timespans timespan = new Timespans
            {
                HelpdeskId = request.HelpdeskId,
                Name = request.Name,
                StartDate = request.StartDate,
                EndDate = request.EndDate
            };
            using (var context = new helpdesksystemContext())
            {
                Timespans existingTimespan = null;
                existingTimespan = context.Timespans.FirstOrDefault(t => t.Name == timespan.Name);

                if (existingTimespan == null)
                {
                    context.Timespans.Add(timespan);
                    context.SaveChanges();
                    spanId = timespan.SpanId;
                }
                else
                {
                    throw new DuplicateNameException("The nickname " + request.Name + " already exists!");
                }
            }
            return spanId;
        }

        /// <summary>
        /// Used to retreve a timespan by its id
        /// </summary>
        /// <param name="id">The id of the timespan</param>
        /// <returns>The timespan DTO</returns>
        public TimeSpanDTO GetTimeSpan(int id)
        {
            TimeSpanDTO timespanDTO = null;

            using (helpdesksystemContext context = new helpdesksystemContext())
            {
                var timespan = context.Timespans.FirstOrDefault(t => t.SpanId == id);

                if (timespan != null)
                    timespanDTO = timespanDAO2DTO(timespan);
            }
            return timespanDTO;
        }

        public List<TimeSpanDTO> GetTimeSpans()
        {
            List<TimeSpanDTO> timespanDTOs = new List<TimeSpanDTO>();

            using (helpdesksystemContext context = new helpdesksystemContext())
            {
                var timespans = context.Timespans.ToList();

                if (timespans.Count == 0)
                    throw new NotFoundException("No timespans found!");

                foreach (Timespans timespan in timespans)
                {
                    if (timespan != null)
                    {
                        TimeSpanDTO timespanDTO = timespanDAO2DTO(timespan);
                        timespanDTOs.Add(timespanDTO);
                    }
                }
            }
            return timespanDTOs;
        }

        public bool UpdateTimeSpan(UpdateTimeSpanRequest request)
        {

            using (helpdesksystemContext context = new helpdesksystemContext())
            {
                Timespans timespan = context.Timespans.FirstOrDefault(t => t.SpanId == request.TimeSpanID);

                if (timespan == null)
                    return false;

                Timespans existingTimespan = null;
                existingTimespan = context.Timespans.FirstOrDefault(t => t.Name == request.Name);

                // Update if no timespan exists matching the requesting name.
                // Update anyway if the names match but the the existing timespan is the timespan we want to update.
                if (existingTimespan == null || existingTimespan.SpanId == request.TimeSpanID)
                {
                    timespan.Name = request.Name;
                    timespan.StartDate = request.StartDate;
                    timespan.EndDate = request.EndDate;
                    context.SaveChanges();
                }
                else
                {
                    throw new DuplicateNameException("The nickname " + request.Name + " already exists!");
                }
            }
            return true;
        }

        public bool DeleteTimeSpan(int id)
        {
            using (helpdesksystemContext context = new helpdesksystemContext())
            {
                Timespans timespan = context.Timespans.FirstOrDefault(ts => ts.SpanId == id);

                if (timespan == null)
                    return true;

                context.Timespans.Remove(timespan);
                context.SaveChanges();
            }
            return true;
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
