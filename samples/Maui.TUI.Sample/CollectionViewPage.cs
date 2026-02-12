using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace Maui.TUI.Sample;

// Model for grouped items
class Contact
{
	public string Name { get; set; } = "";
	public string Role { get; set; } = "";
	public override string ToString() => $"{Name} ({Role})";
}

class ContactGroup : ObservableCollection<Contact>
{
	public string Department { get; }
	public ContactGroup(string department, IEnumerable<Contact> contacts) : base(contacts)
	{
		Department = department;
	}
	public override string ToString() => Department;
}

class CollectionViewPage : ContentPage
{
	readonly ObservableCollection<string> _simpleItems;
	readonly ObservableCollection<ContactGroup> _groupedItems;
	readonly CollectionView _collectionView;
	readonly Label _statusLabel;
	int _itemCount;

	public CollectionViewPage()
	{
		Title = "CollectionView";

		_simpleItems = new ObservableCollection<string>(
			Enumerable.Range(1, 20).Select(i => $"Item {i}")
		);
		_itemCount = 20;

		_groupedItems = new ObservableCollection<ContactGroup>
		{
			new("Engineering", new[]
			{
				new Contact { Name = "Alice Chen", Role = "Lead" },
				new Contact { Name = "Bob Smith", Role = "Senior Dev" },
				new Contact { Name = "Carol Wu", Role = "Dev" },
				new Contact { Name = "Dan Park", Role = "Dev" },
			}),
			new("Design", new[]
			{
				new Contact { Name = "Eve Johnson", Role = "Lead" },
				new Contact { Name = "Frank Li", Role = "UX" },
				new Contact { Name = "Grace Kim", Role = "Visual" },
			}),
			new("Product", new[]
			{
				new Contact { Name = "Hank Brown", Role = "PM" },
				new Contact { Name = "Iris Patel", Role = "PM" },
			}),
			new("QA", new[]
			{
				new Contact { Name = "Jack Davis", Role = "Lead" },
				new Contact { Name = "Karen Lee", Role = "SDET" },
				new Contact { Name = "Leo Nguyen", Role = "Manual QA" },
			}),
		};

		_statusLabel = new Label { Text = "Switch between list modes" };

		_collectionView = new CollectionView
		{
			SelectionMode = SelectionMode.Single,
			VerticalOptions = LayoutOptions.Fill,
		};
		_collectionView.SelectionChanged += (s, e) =>
		{
			if (_collectionView.SelectionMode == SelectionMode.Multiple)
			{
				var count = _collectionView.SelectedItems?.Count ?? 0;
				_statusLabel.Text = $"Selected {count} item(s)";
			}
			else if (_collectionView.SelectedItem is string str)
				_statusLabel.Text = $"Selected: {str}";
			else if (_collectionView.SelectedItem is Contact contact)
				_statusLabel.Text = $"Selected: {contact.Name} - {contact.Role}";
		};

		// Start with simple list
		ShowSimpleList();

		// --- Controls ---
		var addButton = new Button { Text = "Add Item" };
		addButton.Clicked += (s, e) =>
		{
			_itemCount++;
			_simpleItems.Add($"Item {_itemCount}");
			_statusLabel.Text = $"Added Item {_itemCount} ({_simpleItems.Count} total)";
		};

		var removeButton = new Button { Text = "Remove Last" };
		removeButton.Clicked += (s, e) =>
		{
			if (_simpleItems.Count > 0)
			{
				var removed = _simpleItems[^1];
				_simpleItems.RemoveAt(_simpleItems.Count - 1);
				_statusLabel.Text = $"Removed {removed} ({_simpleItems.Count} total)";
			}
		};

		var simpleBtn = new Button { Text = "Simple" };
		var groupedBtn = new Button { Text = "Grouped" };
		var templatedBtn = new Button { Text = "Templated" };
		var multiBtn = new Button { Text = "Multi-Select" };

		simpleBtn.Clicked += (s, e) => ShowSimpleList();
		groupedBtn.Clicked += (s, e) => ShowGroupedList();
		templatedBtn.Clicked += (s, e) => ShowTemplatedList();
		multiBtn.Clicked += (s, e) => ShowMultiSelectList();

		var grid = new Grid
		{
			RowSpacing = 1,
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star),
			}
		};

		grid.Add(new HorizontalStackLayout
		{
			Spacing = 1,
			Children = { simpleBtn, groupedBtn, templatedBtn, multiBtn }
		}, 0, 0);

		grid.Add(new HorizontalStackLayout
		{
			Spacing = 1,
			Children = { addButton, removeButton }
		}, 0, 1);

		grid.Add(_statusLabel, 0, 2);
		grid.Add(_collectionView, 0, 3);

		Content = grid;
	}

	void ShowSimpleList()
	{
		_collectionView.SelectionMode = SelectionMode.Single;
		_collectionView.ItemTemplate = null;
		_collectionView.GroupHeaderTemplate = null;
		_collectionView.IsGrouped = false;
		_collectionView.ItemsSource = _simpleItems;
		_statusLabel.Text = "Showing simple list";
	}

	void ShowGroupedList()
	{
		_collectionView.SelectionMode = SelectionMode.Single;
		_collectionView.ItemTemplate = null;
		_collectionView.GroupHeaderTemplate = new DataTemplate(() =>
		{
			var label = new Label();
			label.SetBinding(Label.TextProperty, new Binding("Department"));
			return label;
		});
		_collectionView.IsGrouped = true;
		_collectionView.ItemsSource = _groupedItems;
		_statusLabel.Text = "Showing grouped list";
	}

	void ShowTemplatedList()
	{
		_collectionView.SelectionMode = SelectionMode.Single;
		_collectionView.ItemTemplate = new DataTemplate(() =>
		{
			var grid = new Grid
			{
				ColumnSpacing = 2,
				ColumnDefinitions =
				{
					new ColumnDefinition(new GridLength(20)),
					new ColumnDefinition(GridLength.Star),
				}
			};
			var nameLabel = new Label();
			nameLabel.SetBinding(Label.TextProperty, new Binding("Name"));
			var roleLabel = new Label();
			roleLabel.SetBinding(Label.TextProperty, new Binding("Role"));
			grid.Add(nameLabel, 0, 0);
			grid.Add(roleLabel, 1, 0);
			return grid;
		});
		_collectionView.GroupHeaderTemplate = new DataTemplate(() =>
		{
			var label = new Label();
			label.SetBinding(Label.TextProperty, new Binding("Department"));
			return label;
		});
		_collectionView.IsGrouped = true;
		_collectionView.ItemsSource = _groupedItems;
		_statusLabel.Text = "Showing templated list (Name | Role columns)";
	}

	void ShowMultiSelectList()
	{
		_collectionView.ItemTemplate = null;
		_collectionView.GroupHeaderTemplate = null;
		_collectionView.IsGrouped = false;
		_collectionView.SelectionMode = SelectionMode.Multiple;
		_collectionView.ItemsSource = _simpleItems;
		_statusLabel.Text = "Multi-select: use Space to toggle items";
	}
}
