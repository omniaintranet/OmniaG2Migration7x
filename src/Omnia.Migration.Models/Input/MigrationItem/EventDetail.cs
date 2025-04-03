using System;
using System.Collections.Generic;

namespace Omnia.Migration.Models.Input.MigrationItem
{
    public class EventDetail
    {
        public Guid Id { get; set; }
        public int PageId { get; set; }
        public int? PageCollectionId { get; set; }
        public string Title { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public int MaxParticipants { get; set; }
        public string RegistrationStartDate { get; set; }
        public string RegistrationEndDate { get; set; }
        public string CancellationEndDate { get; set; }
        public bool? IsColleague { get; set; }
        public bool? ReservationOnly { get; set; }
        public bool? IsOnlineMeeting { get; set; }
        public string OnlineMeetingUrl { get; set; }
        public string OutlookEventId { get; set; }
    }

    public class EventParticipant
    {
        public Guid EventId { get; set; }
        public string LoginName { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Comment { get; set; }
        public ParticipantType ParticipantType { get; set; }
        public int Capacity { get; set; }
        public string StatusResponse { get; set; }
        public string StatusTime { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedAt { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedAt { get; set; }
        public string OutlookEventId { get; set; }
    }

    public enum ParticipantType
    {
        Official = 0,
        Standby = 1
    }

    public class EventClone
    {
        public int PageId { get; set; }
        public string Title { get; set; }
        public int TimeZone { get; set; }
        public string TimeZoneName { get; set; }
        public ScheduleSetting ScheduleSettings { get; set; }
    }

    public class ScheduleSetting
    {
        public RecurrenceTypeEnum Type { get; set; }
        public DailyRecurrenceTypeEnum DailyRecurrenceType { get; set; }
        public int DayFrequency { get; set; }
        public int WeekFrequency { get; set; }
        public List<string> DaysOfWeek { get; set; }
        public MonthlyRecurrenceTypeEnum MonthlyRecurrenceType { get; set; }
        public int Day { get; set; }
        public int MonthFrequency { get; set; }
        public int MonthFrequencyWithDay { get; set; }
        public WeekOfMonthEnum WeekOfMonth { get; set; }
        public string DayOfMonth { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public ScheduleEndTypeEnum ScheduleEndType { get; set; }
    }

    public enum RecurrenceTypeEnum
    {
        Daily = 0,
        Weekly = 1,
        Monthly = 2,
        Never = 3
    }

    public enum DailyRecurrenceTypeEnum
    {
        DayFrequency = 0,
        Weekday = 1
    }

    public enum MonthlyRecurrenceTypeEnum
    {
        SpecificDay = 0,
        WeekOfMonth = 1
    }

    public enum ScheduleEndTypeEnum
    {
        NoEndDate = 0,
        EndByDate = 1
    }

    public enum WeekOfMonthEnum
    {
        First = 0,
        Second = 1,
        Third = 2,
        Fourth = 3,
        Last = 4
    }
}
