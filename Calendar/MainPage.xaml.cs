﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Calendar
{
	public partial class MainPage : ContentPage
	{

		private DateTime now;
		private int currYear;
		private int currMonth;
		private int currDay;

		private const string dbPath = "CalendarEvents.json";
		private List<Event> events;

		public MainPage()
		{
			InitializeComponent();
			events = LoadEvents();
			now = DateTime.Now;
			currYear = now.Year;
			currMonth = now.Month;
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
			TodayButtonVisible();

			// find previous month days
			var prevMonth = firstDay.AddMonths(-1);
			var prevDaysInMonth = DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);
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
					var label = new Label { Text = "" + n };

					if (n == now.Day && currMonth == now.Month && currYear == now.Year)
						label.SetDynamicResource(Label.StyleProperty, "today");
					else if (now.Day % 7 == r && currMonth == now.Month && currYear == now.Year)
						label.SetDynamicResource(Label.StyleProperty, "todayWeek");
					else if (!inMonth)
						label.SetDynamicResource(Label.StyleProperty, "otherMonth");
					else
						label.SetDynamicResource(Label.StyleProperty, "date");
					square.Children.Add(label);

					if (inMonth)
					{
						var dt = new DateTime(currYear, currMonth, n);
						var tapGestureRecognizer = new TapGestureRecognizer();
						tapGestureRecognizer.Tapped += (s, e) =>
						{
							ToggleWindow(dt.ToString("dddd, MMMM d"), hasEvent(dt));
							if (hasEvent(dt))
							{
								eventDetails.Text = getEvent(dt).info;
								deleteButton.IsVisible = true;
							}
							else
							{
								eventDetails.Text = "";
								deleteButton.IsVisible = false;
							}
							currDay = dt.Day;
						};
						square.GestureRecognizers.Add(tapGestureRecognizer);

						// count + display events
						
						if (hasEvent(dt))
						{
							var circle = new BoxView() { HeightRequest = 5, WidthRequest = 5 };
							AbsoluteLayout.SetLayoutBounds(circle, new Rectangle(0.5, 0.9, 0.1, 0.1));
							AbsoluteLayout.SetLayoutFlags(circle, AbsoluteLayoutFlags.All);
							circle.SetDynamicResource(BoxView.StyleProperty, "event");
							square.Children.Add(circle);
						}
					}

					n++;
					
					if ((n > prevDaysInMonth && !inMonth) || (n > daysInMonth && inMonth))
					{
						n = 1;
						inMonth = !inMonth;
					}

					grid.Children.Add(square);
				}
			}
		}

		private bool hasEvent(DateTime dt)
		{
			for (var i = 0; i < events.Count; i++)
				if (events[i].Equals(dt))
					return true;
			return false;
		}

		private Event getEvent(DateTime dt)
		{
			for (var i = 0; i < events.Count; i++)
				if (events[i].Equals(dt))
					return events[i];
			return null;
		}

		private void PrevMonth(object sender, EventArgs e)
		{
			currMonth--;
			if (currMonth < 1)
			{
				currMonth = 12;
				currYear--;
			}
			ToggleWindow();
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
			ToggleWindow();
			GenerateGrid();
		}

		private void JumpToday(object sender, EventArgs e)
		{
			currMonth = now.Month;
			currYear = now.Year;
			GenerateGrid();
		}

		private void TodayButtonVisible()
		{
			// remove today button for present month
			if (now.Month == currMonth && now.Year == currYear)
				todayButton.IsVisible = false;
			else
				todayButton.IsVisible = true;
		}

		private void SwipePrevMonth(object sender, SwipedEventArgs e)
		{
			PrevMonth(null, null);
		}

		private void SwipeNextMonth(object sender, SwipedEventArgs e)
		{
			NextMonth(null, null);
		}

		private void ToggleWindow(String date = null, bool exist = false)
		{
			if (date == null)
			{
				eventWindow.IsVisible = false;
				TodayButtonVisible();
			}
			else
			{
				eventWindow.IsVisible = true;
				todayButton.IsVisible = false;
				if (exist)
					eventDate.Text = "Event - " + date;
				else
					eventDate.Text = "New Event - " + date;
			}
		}

		private void CloseEventWindow(object sender, EventArgs e)
		{
			ToggleWindow();
		}

		public static string DatabasePath
		{
			get
			{
				var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
				return Path.Combine(basePath, dbPath);
			}
		}

		private List<Event> LoadEvents()
		{
			string text;
			try
			{
				text = File.ReadAllText(DatabasePath);
			}
			catch // file doesn't exist yet
			{
				return new List<Event>();
			}
			return JsonSerializer.Deserialize<List<Event>>(text);
		}

		private void SaveEvents()
		{
			var json = JsonSerializer.Serialize(events);
			File.WriteAllText(DatabasePath, json);
		}

		private void SaveEvent(object sender, EventArgs e)
		{
			var dt = new DateTime(currYear, currMonth, currDay);
			if (eventDetails.Text == null)
			{
				DeleteEvent(null, null);
				return;
			}
			else if (hasEvent(dt)) // existing event, update
			{
				DeleteEvent(null, null);
				var d = new Event(currYear, currMonth, currDay, eventDetails.Text);
				events.Add(d);
			}
			else // new event
			{
				var d = new Event(currYear, currMonth, currDay, eventDetails.Text);
				events.Add(d);
			}
			SaveEvents();
			ToggleWindow();
			GenerateGrid();
		}

		private void DeleteEvent(object sender, EventArgs e)
		{
			for (var i = events.Count - 1; i > 0; i--)
			{
				var dt = new DateTime(currYear, currMonth, currDay);
				if (events[i].Equals(dt))
					events.RemoveAt(i);
			}
			SaveEvents();
			ToggleWindow();
			GenerateGrid();
		}
	}
}
