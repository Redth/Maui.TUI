using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.TUI.Sample;

class FormDemoPage : ContentPage
{
	readonly Label _statusLabel;

	public FormDemoPage()
	{
		Title = "Form";

		// Toolbar items
		ToolbarItems.Add(new ToolbarItem("Save", null, () =>
		{
			_statusLabel.Text = $"Saved: {_firstNameEntry.Text} {_lastNameEntry.Text}";
		}));
		ToolbarItems.Add(new ToolbarItem("Clear", null, () => ResetForm()));

		_statusLabel = new Label { Text = "Fill out the form and press Submit" };

		var submitButton = new Button { Text = "Submit" };
		submitButton.Clicked += (s, e) =>
		{
			_statusLabel.Text = $"Submitted: {_firstNameEntry.Text} {_lastNameEntry.Text}, {_emailEntry.Text}";
		};

		var resetButton = new Button { Text = "Reset" };
		resetButton.Clicked += (s, e) => ResetForm();

		Content = new ScrollView
		{
			Content = new VerticalStackLayout
			{
				Spacing = 1,
				Children =
				{
					new Label { Text = "=== User Registration Form ===" },
					_statusLabel,
					BuildPersonalInfoSection(),
					BuildContactSection(),
					BuildPreferencesSection(),
					BuildDateTimeSection(),
					BuildBioSection(),
					new HorizontalStackLayout
					{
						Spacing = 2,
						Children = { submitButton, resetButton }
					},
				}
			}
		};
	}

	// Personal info fields
	Entry _firstNameEntry = null!;
	Entry _lastNameEntry = null!;
	Entry _usernameEntry = null!;

	Grid BuildPersonalInfoSection()
	{
		_firstNameEntry = new Entry { Placeholder = "First name" };
		_lastNameEntry = new Entry { Placeholder = "Last name" };
		_usernameEntry = new Entry { Placeholder = "username" };

		// 3 rows x 2 columns: labels on left, inputs on right
		var grid = new Grid
		{
			ColumnSpacing = 2,
			RowSpacing = 1,
			ColumnDefinitions =
			{
				new ColumnDefinition(new GridLength(15)),
				new ColumnDefinition(GridLength.Star),
			},
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto), // section header (spans 2 cols)
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto),
			}
		};

		var header = new Label { Text = "--- Personal Info ---" };
		Grid.SetColumnSpan(header, 2);
		grid.Add(header, 0, 0);

		grid.Add(new Label { Text = "First Name:" }, 0, 1);
		grid.Add(_firstNameEntry, 1, 1);

		grid.Add(new Label { Text = "Last Name:" }, 0, 2);
		grid.Add(_lastNameEntry, 1, 2);

		grid.Add(new Label { Text = "Username:" }, 0, 3);
		grid.Add(_usernameEntry, 1, 3);

		return grid;
	}

	// Contact fields
	Entry _emailEntry = null!;
	Entry _phoneEntry = null!;
	Picker _countryPicker = null!;

	Grid BuildContactSection()
	{
		_emailEntry = new Entry { Placeholder = "email@example.com" };
		_phoneEntry = new Entry { Placeholder = "+1 555-0100" };
		_countryPicker = new Picker
		{
			Title = "Country",
			ItemsSource = new[] { "United States", "Canada", "United Kingdom", "Germany", "Japan", "Australia" },
		};

		var grid = new Grid
		{
			ColumnSpacing = 2,
			RowSpacing = 1,
			ColumnDefinitions =
			{
				new ColumnDefinition(new GridLength(15)),
				new ColumnDefinition(GridLength.Star),
			},
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto),
			}
		};

		var header = new Label { Text = "--- Contact ---" };
		Grid.SetColumnSpan(header, 2);
		grid.Add(header, 0, 0);

		grid.Add(new Label { Text = "Email:" }, 0, 1);
		grid.Add(_emailEntry, 1, 1);

		grid.Add(new Label { Text = "Phone:" }, 0, 2);
		grid.Add(_phoneEntry, 1, 2);

		grid.Add(new Label { Text = "Country:" }, 0, 3);
		grid.Add(_countryPicker, 1, 3);

		return grid;
	}

	// Preferences
	CheckBox _newsletterCheckBox = null!;
	Switch _darkModeSwitch = null!;
	Slider _fontSizeSlider = null!;
	Label _fontSizeLabel = null!;

	Grid BuildPreferencesSection()
	{
		_newsletterCheckBox = new CheckBox { IsChecked = true };
		_darkModeSwitch = new Switch { IsToggled = false };
		_fontSizeSlider = new Slider(8, 24, 14);
		_fontSizeLabel = new Label { Text = "14" };

		_fontSizeSlider.ValueChanged += (s, e) =>
		{
			_fontSizeLabel.Text = $"{(int)e.NewValue}";
		};

		// 4 rows x 3 columns: label | control | extra info
		var grid = new Grid
		{
			ColumnSpacing = 2,
			RowSpacing = 1,
			ColumnDefinitions =
			{
				new ColumnDefinition(new GridLength(15)),
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(new GridLength(6)),
			},
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto),
			}
		};

		var header = new Label { Text = "--- Preferences ---" };
		Grid.SetColumnSpan(header, 3);
		grid.Add(header, 0, 0);

		grid.Add(new Label { Text = "Newsletter:" }, 0, 1);
		grid.Add(_newsletterCheckBox, 1, 1);

		grid.Add(new Label { Text = "Dark Mode:" }, 0, 2);
		grid.Add(_darkModeSwitch, 1, 2);

		grid.Add(new Label { Text = "Font Size:" }, 0, 3);
		grid.Add(_fontSizeSlider, 1, 3);
		grid.Add(_fontSizeLabel, 2, 3);

		return grid;
	}

	// Date/Time/Stepper section
	DatePicker _datePicker = null!;
	TimePicker _timePicker = null!;
	Stepper _stepper = null!;
	Label _stepperValueLabel = null!;

	Grid BuildDateTimeSection()
	{
		_datePicker = new DatePicker { Date = DateTime.Today };
		_timePicker = new TimePicker { Time = DateTime.Now.TimeOfDay };
		_stepper = new Stepper { Minimum = 0, Maximum = 10, Value = 1, Increment = 1 };
		_stepperValueLabel = new Label { Text = "Guests: 1" };

		_stepper.ValueChanged += (s, e) =>
		{
			_stepperValueLabel.Text = $"Guests: {(int)e.NewValue}";
		};

		var grid = new Grid
		{
			ColumnSpacing = 2,
			RowSpacing = 1,
			ColumnDefinitions =
			{
				new ColumnDefinition(new GridLength(15)),
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(new GridLength(12)),
			},
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto),
			}
		};

		var header = new Label { Text = "--- Schedule ---" };
		Grid.SetColumnSpan(header, 3);
		grid.Add(header, 0, 0);

		grid.Add(new Label { Text = "Date:" }, 0, 1);
		grid.Add(_datePicker, 1, 1);

		grid.Add(new Label { Text = "Time:" }, 0, 2);
		grid.Add(_timePicker, 1, 2);

		grid.Add(new Label { Text = "Guests:" }, 0, 3);
		grid.Add(_stepper, 1, 3);
		grid.Add(_stepperValueLabel, 2, 3);

		return grid;
	}

	// Bio section — editor spans 2 columns
	Editor _bioEditor = null!;

	Grid BuildBioSection()
	{
		_bioEditor = new Editor { Placeholder = "Tell us about yourself...", HeightRequest = 4 };

		var grid = new Grid
		{
			ColumnSpacing = 2,
			RowSpacing = 1,
			ColumnDefinitions =
			{
				new ColumnDefinition(new GridLength(15)),
				new ColumnDefinition(GridLength.Star),
			},
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto),
			}
		};

		var header = new Label { Text = "--- Bio ---" };
		Grid.SetColumnSpan(header, 2);
		grid.Add(header, 0, 0);

		// Editor spans both columns for full width
		Grid.SetColumnSpan(_bioEditor, 2);
		grid.Add(_bioEditor, 0, 1);

		return grid;
	}

	void ResetForm()
	{
		_firstNameEntry.Text = string.Empty;
		_lastNameEntry.Text = string.Empty;
		_usernameEntry.Text = string.Empty;
		_emailEntry.Text = string.Empty;
		_phoneEntry.Text = string.Empty;
		_countryPicker.SelectedIndex = -1;
		_newsletterCheckBox.IsChecked = true;
		_darkModeSwitch.IsToggled = false;
		_fontSizeSlider.Value = 14;
		_datePicker.Date = DateTime.Today;
		_timePicker.Time = DateTime.Now.TimeOfDay;
		_stepper.Value = 1;
		_bioEditor.Text = string.Empty;
		_statusLabel.Text = "Form reset";
	}
}
