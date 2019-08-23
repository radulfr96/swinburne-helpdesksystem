﻿using System;

namespace Helpdesk.Website
{
    internal class JobSchedule
    {
        public Type Type { get; }
        public string CronExpression { get; set; }

        public JobSchedule(Type type, string cronExpression)
        {
            Type = type;
            CronExpression = cronExpression;
        }
    }
}