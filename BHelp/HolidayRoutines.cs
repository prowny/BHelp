using System.Collections.Generic;
using BHelp.DataAccessLayer;
using System.Linq;
using System;
using BHelp.Models;

namespace BHelp
{
    public static class HolidayRoutines
    {
        private static readonly BHelpContext db = new BHelpContext();
        public static List<Holiday> GetHolidays(int year)
        {
            var Holidays = new List<Holiday>();
            var _holidayList = db.Holidays.ToList();

            foreach (var hol in _holidayList)
            {
                // Fixed date is sole option as of 11/02/2023
                if (hol.FixedDate.Year == year) // Fixed Date, no repeats
                {
                    
                        var _holiday = new Holiday()
                        {
                            FixedDate = hol.FixedDate,
                            Description = hol.Description
                        };
                        Holidays.Add(_holiday);
                }
            }

            return (Holidays);
        }

        public static bool IsHoliday(DateTime dt, IEnumerable<Holiday> holidays)
        {
            return holidays.Any(hol => dt == hol.FixedDate);
        }
    }
}