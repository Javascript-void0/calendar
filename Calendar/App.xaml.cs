using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Calendar
{
	public partial class App : Application
	{
		public App()
		{
			InitializeComponent();

			MainPage = new MainPage();
		}

		protected override void OnStart()
		{
		}

		protected override void OnSleep()
		{
		}

		protected override void OnResume()
		{
		}

		private const String Option1Key = "Option1";
		private const String Option2Key = "Option2";
		private const String Option3Key = "Option3";

		public bool Option1
		{
			get
			{
				if (Properties.ContainsKey(Option1Key))
					return (bool)Properties[Option1Key];
				return false;
			}
			set
			{
				Properties[Option1Key] = value;
			}
		}

		public bool Option2
		{
			get
			{
				if (Properties.ContainsKey(Option2Key))
					return (bool)Properties[Option2Key];
				return false;
			}
			set
			{
				Properties[Option2Key] = value;
			}
		}

		public bool Option3
		{
			get
			{
				if (Properties.ContainsKey(Option3Key))
					return (bool)Properties[Option3Key];
				return false;
			}
			set
			{
				Properties[Option3Key] = value;
			}
		}

	}
}
