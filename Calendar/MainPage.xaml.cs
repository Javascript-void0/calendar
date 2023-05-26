using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Essentials;

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
		private List<StackLayout> windows = new List<StackLayout>();

		private const int windowHeight = 565;
		private const uint animationTime = (uint)200;

		public MainPage()
		{
			InitializeComponent();
			events = LoadEvents();
			now = DateTime.Now;
			currYear = now.Year;
			currMonth = now.Month;
			GenerateGrid();

			windows.Add(settingsWindow);
			windows.Add(eventWindow);
			windows.Add(listWindow);

			foreach (StackLayout s in windows)
			{
				s.TranslationY = windowHeight;
				AbsoluteLayout.SetLayoutBounds(s, new Rectangle(0, 1, 1, windowHeight));
			}
			 
			settingsIcon.Source = ImageSource.FromResource("Calendar.Images.menu.png");
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

		private void GenerateGrid(string filterEvent = null)
		{
			ClearGrid();

			DateTime firstDay = new DateTime(currYear, currMonth, 1);
			monthYear.Text = "" + firstDay.ToString("MMMM") + " " + currYear;
			var dayOfWeek = (int)firstDay.DayOfWeek;
			var daysInMonth = DateTime.DaysInMonth(currYear, currMonth);

			// find previous month days
			var prevMonth = firstDay.AddMonths(-1);
			var prevDaysInMonth = DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);
			var inMonth = false;

			var n = prevDaysInMonth - dayOfWeek + 1; // start?
			var numEvents = 0;
			var monthEvents = new Dictionary<string, int>();
			for (var r = 0; r < 6; r++)
			{
				for (var c = 0; c < 7; c++)
				{
					var square = new AbsoluteLayout();
					square.SetValue(Grid.RowProperty, r + 1);
					square.SetValue(Grid.ColumnProperty, c);

					var gridIndex = r * 7 + c;
					var label = new Label { Text = "" + n };

					if (n == now.Day && currMonth == now.Month && currYear == now.Year && inMonth)
						label.SetDynamicResource(Label.StyleProperty, "today");
					// highlight current week
					else if (n >= now.Day - (int)now.DayOfWeek && n < now.Day - (int)now.DayOfWeek + 7 && inMonth && currMonth == now.Month && currYear == now.Year)
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
							if (findEvent(dt) != null)
							{
								eventDetails.Text = getEvent(dt).info;
								deleteButton.IsVisible = true;
							}
							else
							{
								eventDetails.Text = "";
								deleteButton.IsVisible = false;
							}
							ToggleEventWindow(dt.ToString("dddd, MMMM d"), findEvent(dt) != null);
							currDay = dt.Day;
						};
						square.GestureRecognizers.Add(tapGestureRecognizer);

						// count + display events

						var ev = findEvent(dt);
						if (ev != null)
						{
							// split different events by ', '
							ev = ev.Replace(", ", ",");
							var splitMultiple = ev.Split(',');

							numEvents += splitMultiple.Length;

							// draw circle if fits filter
							if (filterEvent == null || splitMultiple.Contains(filterEvent))
							{
								var circle = new BoxView() { HeightRequest = 5, WidthRequest = 5 };
								AbsoluteLayout.SetLayoutBounds(circle, new Rectangle(0.5, 0.9, 0.1, 0.1));
								AbsoluteLayout.SetLayoutFlags(circle, AbsoluteLayoutFlags.All);
								circle.SetDynamicResource(BoxView.StyleProperty, "event");
								square.Children.Add(circle);
							}

							// count keys
							foreach (var e in splitMultiple)
							{
								if (monthEvents.ContainsKey(e))
									monthEvents[e] += 1;
								else
									monthEvents.Add(e, 1);
							}

							if (dt.Day == now.Day && dt.Month == now.Month && dt.Year == now.Year)
								todayEvent.Text = ev;
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

			listEvents.Children.Clear();
			Button b = new Button() { Text = "All Events (" + numEvents + ")" };
			b.SetDynamicResource(Label.StyleProperty, "filterButton");
			b.Clicked += (s, e) =>
			{
				GenerateGrid();
				CloseWindows();
			};
			listEvents.Children.Add(b);

			foreach (KeyValuePair<string, int> x in monthEvents)
			{
				b = new Button() { Text = x.Key + " (" + x.Value + ")", BindingContext=x.Key };
				b.SetDynamicResource(Label.StyleProperty, "filterButton");
				b.Clicked += (s, e) =>
				{
					var filter = ((Button)s).BindingContext.ToString();
					GenerateGrid(filter);
					CloseWindows();
				};
				listEvents.Children.Add(b);
			}

		}

		private string findEvent(DateTime dt)
		{
			for (var i = 0; i < events.Count; i++)
				if (events[i].Equals(dt))
					return events[i].info;
			return null;
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
			if (currMonth == now.Month && currYear == now.Year)
				return;
			currMonth = now.Month;
			currYear = now.Year;
			GenerateGrid();
		}

		private async void ToggleEventWindow(String date = null, bool exist = false)
		{
			if (date == null)
			{
				// hide keyboard
				eventDetails.IsEnabled = false;
				eventDetails.IsEnabled = true;
				// animation
				await eventWindow.TranslateTo(0, windowHeight, animationTime, Easing.CubicOut);

				// hide
				eventWindow.IsVisible = false;
				eventWindow.InputTransparent = true;
				dim.IsVisible = false;
				dim.InputTransparent = true;
			}
			else
			{
				HapticFeedback.Perform(HapticFeedbackType.Click);
				// unhide
				eventWindow.IsVisible = true;
				eventWindow.InputTransparent = false;
				dim.IsVisible = true;
				dim.InputTransparent = false;

				if (exist)
					eventDate.Text = "Event - " + date;
				else
					eventDate.Text = "New Event - " + date;

				// animation
				eventDetails.Focus();
				Console.WriteLine(eventDetails.Text);
				if (eventDetails.Text != null)
					eventDetails.CursorPosition = eventDetails.Text.Length;
				await eventWindow.TranslateTo(0, 0, animationTime, Easing.CubicOut);
			}
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
				DeleteEvent();
				return;
			}
			else if (findEvent(dt) != null) // existing event, update
			{
				DeleteEvent();
				var d = new Event(currYear, currMonth, currDay, eventDetails.Text);
				events.Add(d);
			}
			else // new event
			{
				var d = new Event(currYear, currMonth, currDay, eventDetails.Text);
				events.Add(d);
			}
			SaveEvents();
			ToggleEventWindow();
			GenerateGrid();
		}

		private void DeleteEvent(object sender = null, EventArgs e = null)
		{
			for (var i = events.Count - 1; i > 0; i--)
			{
				var dt = new DateTime(currYear, currMonth, currDay);
				if (events[i].Equals(dt))
					events.RemoveAt(i);
			}
			SaveEvents();
			ToggleEventWindow();
			GenerateGrid();
		}

		private void CloseWindows(object sender = null, EventArgs e = null)
		{
			if (settingsWindow.IsVisible)
				ToggleSettingsWindow();
			if (eventWindow.IsVisible)
				ToggleEventWindow();
			if (listWindow.IsVisible)
				ToggleListWindow();
		}

		private void DragStarting(object sender, DragStartingEventArgs e)
		{
			try
			{
				HapticFeedback.Perform(HapticFeedbackType.LongPress);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
			ToggleListWindow();
			 
		}

		private async void ToggleListWindow()
		{
			if (listWindow.IsVisible)
			{
				// hide
				await listWindow.TranslateTo(0, windowHeight, animationTime, Easing.CubicOut);
				listWindow.IsVisible = false;
				listWindow.InputTransparent = true;
				dim.IsVisible = false;
				dim.InputTransparent = true;
			}
			else
			{
				// show
				listWindow.IsVisible = true;
				listWindow.InputTransparent = false;
				dim.IsVisible = true;
				dim.InputTransparent = false;
				await listWindow.TranslateTo(0, 0, animationTime, Easing.CubicOut);
			}
		}

		private async void ToggleSettingsWindow(object sender = null, EventArgs e = null)
		{
			// pressed button
			if (sender != null)
				HapticFeedback.Perform(HapticFeedbackType.Click);

			if (settingsWindow.IsVisible)
			{
				// hide
				await settingsWindow.TranslateTo(0, windowHeight, animationTime, Easing.CubicOut);
				settingsWindow.IsVisible = false;
				settingsWindow.InputTransparent = true;
				dim.IsVisible = false;
				dim.InputTransparent = true;
			}
			else
			{
				// show
				settingsWindow.IsVisible = true;
				settingsWindow.InputTransparent = false;
				dim.IsVisible = true;
				dim.InputTransparent = false;
				await settingsWindow.TranslateTo(0, 0, animationTime, Easing.CubicOut);
			}
		}

		private void SwipeDown(object sender, SwipedEventArgs e)
		{
			CloseWindows();
		}

		protected override bool OnBackButtonPressed()
		{
			// close window with back button
			if (settingsWindow.IsVisible)
			{
				ToggleSettingsWindow();
				return true;
			}
			if (eventWindow.IsVisible)
			{
				ToggleEventWindow();
				return true;
			}
			if (listWindow.IsVisible)
			{
				ToggleListWindow();
				return true;
			}
			// none open, default behavior
			return false;
		}
	}
}
