using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ContactManager.Data;
using ContactManager.Models;

namespace ContactManager.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ContactRepository _repository = new();

    [ObservableProperty]
    private ObservableCollection<Contact> _contacts = [];

    [ObservableProperty]
    private Contact? _selectedContact;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _firstName = string.Empty;

    [ObservableProperty]
    private string _lastName = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private string _company = string.Empty;

    [ObservableProperty]
    private string _notes = string.Empty;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private bool _isEditing;

    public string FormTitle => IsEditing ? "Edit Contact" : "New Contact";

    public MainViewModel()
    {
        LoadContacts();
    }

    partial void OnIsEditingChanged(bool value) => OnPropertyChanged(nameof(FormTitle));

    partial void OnSelectedContactChanged(Contact? value)
    {
        if (value is null)
        {
            return;
        }

        FirstName = value.FirstName;
        LastName = value.LastName;
        Email = value.Email;
        Phone = value.Phone;
        Company = value.Company;
        Notes = value.Notes;
        IsEditing = true;
        StatusMessage = $"Editing contact: {value.FullName}";
    }

    [RelayCommand]
    private void LoadContacts()
    {
        var items = _repository.GetAll(string.IsNullOrWhiteSpace(SearchText) ? null : SearchText);
        Contacts = new ObservableCollection<Contact>(items);
        StatusMessage = $"{Contacts.Count} contact(s) loaded";
    }

    [RelayCommand]
    private void Search()
    {
        LoadContacts();
    }

    [RelayCommand]
    private void NewContact()
    {
        SelectedContact = null;
        ClearForm();
        IsEditing = false;
        StatusMessage = "Creating new contact";
    }

    [RelayCommand]
    private void SaveContact()
    {
        if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
        {
            MessageBox.Show("First name and last name are required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!string.IsNullOrWhiteSpace(Email) && !Email.Contains('@'))
        {
            MessageBox.Show("Please enter a valid email address.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            if (IsEditing && SelectedContact is not null)
            {
                var contact = new Contact
                {
                    Id = SelectedContact.Id,
                    FirstName = FirstName.Trim(),
                    LastName = LastName.Trim(),
                    Email = Email.Trim(),
                    Phone = Phone.Trim(),
                    Company = Company.Trim(),
                    Notes = Notes.Trim(),
                    CreatedAt = SelectedContact.CreatedAt
                };

                _repository.Update(contact);
                StatusMessage = $"Updated: {contact.FullName}";
            }
            else
            {
                var contact = new Contact
                {
                    FirstName = FirstName.Trim(),
                    LastName = LastName.Trim(),
                    Email = Email.Trim(),
                    Phone = Phone.Trim(),
                    Company = Company.Trim(),
                    Notes = Notes.Trim()
                };

                var id = _repository.Create(contact);
                contact.Id = id;
                StatusMessage = $"Created: {contact.FullName}";
            }

            LoadContacts();
            ClearForm();
            IsEditing = false;
            SelectedContact = null;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save contact: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void DeleteContact()
    {
        if (SelectedContact is null)
        {
            MessageBox.Show("Select a contact to delete.", "Delete", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show(
            $"Delete {SelectedContact.FullName}?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            _repository.Delete(SelectedContact.Id);
            StatusMessage = "Contact deleted";
            NewContact();
            LoadContacts();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to delete contact: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ClearForm()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
        Email = string.Empty;
        Phone = string.Empty;
        Company = string.Empty;
        Notes = string.Empty;
    }

    [RelayCommand]
    private void CancelEdit()
    {
        if (SelectedContact is not null)
        {
            OnSelectedContactChanged(SelectedContact);
            return;
        }

        ClearForm();
        IsEditing = false;
        StatusMessage = "Edit cancelled";
    }
}
