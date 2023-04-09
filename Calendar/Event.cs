using System;
using System.Collections.Generic;
using System.Text;

namespace Calendar
{
	class Event
	{
		public int year { get; set; }
		public int month { get; set; }
		public int day { get; set; }
		public String info { get; set; }

		public Event(int year, int month, int day, String info)
		{
			this.month = month;
			this.year = year;
			this.day = day;
			this.info = info;
		}

		public bool Equals(DateTime dt)
		{
			return dt.Year == year && dt.Month == month && dt.Day == day;
		}
	}
}
