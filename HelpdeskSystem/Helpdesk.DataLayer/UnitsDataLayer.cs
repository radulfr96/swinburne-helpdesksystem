using System;
using System.Collections.Generic;
using Helpdesk.Data.Models;
using NLog;
using Helpdesk.Common.DTOs;
using System.Linq;
using Helpdesk.Common.Extensions;
using Microsoft.EntityFrameworkCore;
using Helpdesk.Common.Requests.Units;
using System.Data;
using System.Data.Common;
using Helpdesk.DataLayer.Contracts;
using Microsoft.EntityFrameworkCore.Storage;

namespace Helpdesk.DataLayer
{
    public class UnitsDataLayer : IUnitsDataLayer, IDisposable
    {
        private static Logger s_Logger = LogManager.GetCurrentClassLogger();
        private helpdesksystemContext context;

        public UnitsDataLayer()
        {
            context = new helpdesksystemContext();
        }

        public void AddUnit(Unit newUnit)
        {
            context.Add(newUnit);
        }

        public void AddHelpdeskUnit(Helpdeskunit helpdeskunit)
        {
            context.Helpdeskunit.Add(helpdeskunit);
        }

        public Unit GetUnit(int id)
        {
            return context.Unit.Include("Topic").FirstOrDefault(u => u.UnitId == id);
        }

        public Unit GetUnitByNameAndHelpdeskId(string name, int helpdeskId)
        {
            var unitIds = context.Helpdeskunit.Where(hu => hu.HelpdeskId == helpdeskId).Select(u => u.UnitId).ToList();
            return context.Unit.Include("Topic").Include("Helpdeskunit").FirstOrDefault(u => u.Name.Equals(name) && unitIds.Contains(u.UnitId));
        }

        public Unit GetUnitByCodeAndHelpdeskId(string code, int helpdeskId)
        {
            var unitIds = context.Helpdeskunit.Where(hu => hu.HelpdeskId == helpdeskId).Select(u => u.UnitId).ToList();
            return context.Unit.Include("Helpdeskunit").Include("Topic").FirstOrDefault(u => u.Code.Equals(code) && unitIds.Contains(u.UnitId));
        }

        public List<Unit> GetUnitsByHelpdeskID(int id, bool getActive)
        {
            List<Unit> units = new List<Unit>();

            using (helpdesksystemContext context = new helpdesksystemContext())
            {
                var helpdeskUnits = context.Helpdeskunit.Where(hu => hu.HelpdeskId == id).ToList();

                foreach (Helpdeskunit helpdeskUnit in helpdeskUnits)
                {
                    Unit unit = context.Unit.Include("Topic").Where(u => u.UnitId == helpdeskUnit.UnitId).FirstOrDefault();

                    if (getActive && !unit.IsDeleted)
                        units.Add(unit);
                    else if (!getActive)
                        units.Add(unit);
                }
            }
            return units;
        }

        public DataTable GetUnitsAsDataTable()
        {
            DataTable units = new DataTable();
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
                        units.Load(reader);
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
            return units;
        }

        public void DeleteUnit(Unit unit)
        {
            context.Unit.Remove(unit);
        }

        /// <summary>
        /// Converts the unit DAO to a DTO to send to the front end
        /// </summary>
        /// <param name="unit">Unit DAO object to be converted.</param>
        /// <returns></returns>
        private UnitDTO DAO2DTO(Unit unit)
        {
            UnitDTO unitDTO = new UnitDTO();
            unitDTO.UnitId = unit.UnitId;
            unitDTO.Code = unit.Code;
            unitDTO.Name = unit.Name;
            unitDTO.IsDeleted = unit.IsDeleted;

            foreach (Topic topic in unit.Topic)
            {
                if (!topic.IsDeleted)
                {
                    unitDTO.Topics.Add(
                        new TopicDTO()
                        {
                            Name = topic.Name,
                            IsDeleted = topic.IsDeleted,
                            TopicId = topic.TopicId,
                            UnitId = topic.UnitId
                        });
                }
            }

            return unitDTO;
        }

        /// <summary>
        /// Converts the unit DTO to a DAO to interact with the database
        /// </summary>
        /// <param name="unitDTO">Unit DTO object to be converted.</param>
        /// <returns></returns>
        private Unit DTO2DAO(UnitDTO unitDTO)
        {
            Unit unit = new Unit();
            unit.UnitId = unitDTO.UnitId;
            unit.Code = unitDTO.Code;
            unit.Name = unitDTO.Name;
            unit.IsDeleted = unitDTO.IsDeleted;

            return unit;
        }

        public void Save()
        {
            context.Dispose();
        }

        public IDbContextTransaction GetTransaction()
        {
            return context.Database.BeginTransaction();
        }

        public void Dispose()
        {
            context.Dispose();
        }
    }
}
