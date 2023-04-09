using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Calendar
{
	public partial class MainPage : ContentPage
	{

		private DateTime now;
		private int currMonth;
		private int currYear;

		public MainPage()
		{
			InitializeComponent();
			now = DateTime.Now;
			currMonth = now.Month;
			currYear = now.Year;
			GenerateGrid();
		}

		private void ClearGrid()
		{
			for (var i = grid.Children.Count - 1; i >= 0; i--)
			{
				if (grid.Children[i].GetType() == typeof(AbsoluteLayout))
				{
					grid.Children.RemoveAt(i);
				}
			}
		}

		private void GenerateGrid()
		{
			ClearGrid();

			DateTime firstDay = new DateTime(currYear, currMonth, 1);
			monthYear.Text = "" + firstDay.ToString("MMMM") + " " + currYear;
			var dayOfWeek = (int)firstDay.DayOfWeek;
			var daysInMonth = DateTime.DaysInMonth(currYear, currMonth);
			Console.WriteLine(daysInMonth);

			// find previous month days
			var prevMonth = firstDay.AddMonths(-1);
			var prevDaysInMonth = DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);
			Console.WriteLine(prevDaysInMonth);
			var inMonth = false;

			var n = prevDaysInMonth - dayOfWeek + 1; // start?
			for (var r = 0; r < 6; r++)
			{
				for (var c = 0; c < 7; c++)
				{
					var square = new AbsoluteLayout();
					square.SetValue(Grid.RowProperty, r + 1);
					square.SetValue(Grid.ColumnProperty, c);

					var gridIndex = r * 7 + c;
					var label = new Label();
					label.Text = "" + n;
					
					if (n == now.Day && currMonth == now.Month && currYear == now.Year)
						label.SetDynamicResource(Label.StyleProperty, "today");
					else if (now.Day % 7 == r && currMonth == now.Month && currYear == now.Year)
						label.SetDynamicResource(Label.StyleProperty, "todayWeek");
					else if (!inMonth)
						label.SetDynamicResource(Label.StyleProperty, "otherMonth");
					else
						label.SetDynamicResource(Label.StyleProperty, "date");
					n++;
					square.Children.Add(label);
					
					if ((n > prevDaysInMonth && !inMonth) || (n > daysInMonth && inMonth))
					{
						n = 1;
						inMonth = !inMonth;
					}

					grid.Children.Add(square);
				}
			}
		}

		private void PrevMonth(object sender, EventArgs e)
		{
			currMonth--;
			if (currMonth < 1)
			{
				currMonth = 12;
				currYear--;
			}
			GenerateGrid();
		}

		private void NextMonth(object sender, EventArgs e)
		{
			currMonth++;
			if (currMonth > 12)
			{
				currMonth = 1;
				currYear++;
			}
			GenerateGrid();
		}

		private void JumpToday(object sender, EventArgs e)
		{
			currMonth = now.Month;
			currYear = now.Year;
			GenerateGrid();
		}
	}
}
