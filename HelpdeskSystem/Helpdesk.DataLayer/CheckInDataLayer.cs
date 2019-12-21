using Helpdesk.Common.DTOs;
using Helpdesk.Common.Extensions;
using Helpdesk.Common.Requests.CheckIn;
using Helpdesk.Data.Models;
using Helpdesk.DataLayer.Contracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Helpdesk.DataLayer
{
    public class CheckInDataLayer : ICheckInDataLayer
    {
        public int CheckIn(CheckInRequest request)
        {
            int id = 0;

            using (helpdesksystemContext context = new helpdesksystemContext())
            {
                Checkinhistory checkIn = new Checkinhistory()
                {
                    UnitId = request.UnitID,
                    CheckInTime = DateTime.Now,
                    StudentId = request.StudentID
                };

                context.Checkinhistory.Add(checkIn);
                context.SaveChanges();

                id = checkIn.CheckInId;
            }
            return id;
        }

        public bool CheckOut(CheckOutRequest request)
        {
            using (helpdesksystemContext context = new helpdesksystemContext())
            {
                Checkinhistory checkOut = context.Checkinhistory.FirstOrDefault(co => co.CheckInId == request.CheckInID);

                if (checkOut == null)
                    return false;

                checkOut.CheckoutTime = DateTime.Now;
                checkOut.ForcedCheckout = request.ForcedCheckout;

                context.SaveChanges();
            }
            return true;
        }

        public List<CheckInDTO> GetCheckinsByHelpdeskId(int helpdeskId)
        {
            List<CheckInDTO> checkInDTOs = new List<CheckInDTO>();

            using (helpdesksystemContext context = new helpdesksystemContext())
            {
                var unitIds = context.Helpdeskunit.Where(u => u.HelpdeskId == helpdeskId).Select(u => u.UnitId).ToList();

                var checkIns = context
                                .Checkinhistory
                                .Include("Student")
                                .Where(c =>
                                    unitIds.Contains(c.UnitId)
                                    && !c.CheckoutTime.HasValue
                                    && (!c.ForcedCheckout.HasValue || !c.ForcedCheckout.Value)
                                    ).ToList();

                if (checkIns.Count == 0)
                    throw new NotFoundException("Helpdesk has no check ins");

                foreach (var checkIn in checkIns)
                {
                    checkInDTOs.Add(DAO2DTO(checkIn));
                }
            }
            return checkInDTOs;
        }

        public CheckInDTO DAO2DTO(Checkinhistory checkIn)
        {
            CheckInDTO dto = new CheckInDTO()
            {
                CheckInId = checkIn.CheckInId,
                Nickname = checkIn.Student.NickName,
                UnitId = checkIn.UnitId,
                StudentId = checkIn.StudentId.Value
            };

            return dto;
        }

        public DataTable GetCheckInsAsDataTable()
        {
            DataTable checkIns = new DataTable();

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
            }

            return checkIns;
        }

        public DataTable GetCheckInQueueItemsAsDataTable()
        {
            DataTable checkInQueueItems = new DataTable();

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
            }

            return checkInQueueItems;
        }
    }
}
