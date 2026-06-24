using System.IO;
using ContactManager.Models;
using Microsoft.Data.Sqlite;

namespace ContactManager.Data;

public class ContactRepository
{
    private readonly string _connectionString;

    public ContactRepository()
    {
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ContactManager",
            "contacts.db");

        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
        _connectionString = $"Data Source={dbPath}";
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS Contacts (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                FirstName TEXT NOT NULL,
                LastName TEXT NOT NULL,
                Email TEXT NOT NULL,
                Phone TEXT NOT NULL,
                Company TEXT NOT NULL,
                Notes TEXT NOT NULL,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL
            );
            """;
        command.ExecuteNonQuery();
    }

    public List<Contact> GetAll(string? search = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        if (string.IsNullOrWhiteSpace(search))
        {
            command.CommandText = "SELECT * FROM Contacts ORDER BY LastName, FirstName";
        }
        else
        {
            command.CommandText = """
                SELECT * FROM Contacts
                WHERE FirstName LIKE $search
                   OR LastName LIKE $search
                   OR Email LIKE $search
                   OR Company LIKE $search
                ORDER BY LastName, FirstName
                """;
            command.Parameters.AddWithValue("$search", $"%{search.Trim()}%");
        }

        var contacts = new List<Contact>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            contacts.Add(ReadContact(reader));
        }

        return contacts;
    }

    public Contact? GetById(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Contacts WHERE Id = $id";
        command.Parameters.AddWithValue("$id", id);

        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadContact(reader) : null;
    }

    public int Create(Contact contact)
    {
        var now = DateTime.UtcNow;
        contact.CreatedAt = now;
        contact.UpdatedAt = now;

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Contacts (FirstName, LastName, Email, Phone, Company, Notes, CreatedAt, UpdatedAt)
            VALUES ($firstName, $lastName, $email, $phone, $company, $notes, $createdAt, $updatedAt);
            SELECT last_insert_rowid();
            """;
        AddContactParameters(command, contact);

        return Convert.ToInt32(command.ExecuteScalar());
    }

    public void Update(Contact contact)
    {
        contact.UpdatedAt = DateTime.UtcNow;

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE Contacts
            SET FirstName = $firstName,
                LastName = $lastName,
                Email = $email,
                Phone = $phone,
                Company = $company,
                Notes = $notes,
                UpdatedAt = $updatedAt
            WHERE Id = $id
            """;
        AddContactParameters(command, contact);
        command.Parameters.AddWithValue("$id", contact.Id);
        command.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Contacts WHERE Id = $id";
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }

    private static void AddContactParameters(SqliteCommand command, Contact contact)
    {
        command.Parameters.AddWithValue("$firstName", contact.FirstName);
        command.Parameters.AddWithValue("$lastName", contact.LastName);
        command.Parameters.AddWithValue("$email", contact.Email);
        command.Parameters.AddWithValue("$phone", contact.Phone);
        command.Parameters.AddWithValue("$company", contact.Company);
        command.Parameters.AddWithValue("$notes", contact.Notes);
        command.Parameters.AddWithValue("$createdAt", contact.CreatedAt.ToString("O"));
        command.Parameters.AddWithValue("$updatedAt", contact.UpdatedAt.ToString("O"));
    }

    private static Contact ReadContact(SqliteDataReader reader)
    {
        return new Contact
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
            LastName = reader.GetString(reader.GetOrdinal("LastName")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            Phone = reader.GetString(reader.GetOrdinal("Phone")),
            Company = reader.GetString(reader.GetOrdinal("Company")),
            Notes = reader.GetString(reader.GetOrdinal("Notes")),
            CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("CreatedAt"))),
            UpdatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("UpdatedAt")))
        };
    }
}
