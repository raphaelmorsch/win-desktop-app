# Contact Manager

A native Windows desktop CRUD application for managing contacts. Built with **C#**, **WPF**, and **SQLite**.

## Features

- **Create** new contacts with name, email, phone, company, and notes
- **Read** and browse all contacts in a searchable list
- **Update** existing contacts by selecting them from the list
- **Delete** contacts with confirmation
- Local SQLite database (data persists between sessions)
- Clean MVVM architecture with data binding

## Tech Stack

| Layer | Technology |
|-------|------------|
| UI | WPF (.NET 10) |
| Pattern | MVVM (CommunityToolkit.Mvvm) |
| Database | SQLite (Microsoft.Data.Sqlite) |

## Requirements

- Windows 10 or later (Server 2022 works)
- No separate .NET install needed when using the **GitHub Actions artifact** (self-contained build)
- [.NET 10 SDK](https://dotnet.microsoft.com/download) only if building from source

## Run the App

```bash
dotnet restore
dotnet run
```

## Build a Release

```bash
dotnet publish -c Release -r win-x64 --self-contained true
```

The executable will be in `bin/Release/net10.0-windows/win-x64/publish/`.

## Deploy via GitHub Actions (Windows VM)

Every push to `main` builds a **self-contained** Windows x64 package (includes the .NET Desktop Runtime — no install required on the VM).

1. Open the repo on GitHub → **Actions** → latest **Build Windows App** run
2. Download the **ContactManager-win-x64** artifact (zip)
3. On your Windows VM, extract the zip
4. Run `ContactManager.exe`

You can also trigger a build manually: **Actions** → **Build Windows App** → **Run workflow**.

To build from source on the VM instead:

```powershell
git clone https://github.com/raphaelmorsch/win-desktop-app.git
cd win-desktop-app
dotnet run
```

## Data Storage

Contacts are stored in:

```
%LOCALAPPDATA%\ContactManager\contacts.db
```

## Project Structure

```
ContactManager/
├── Models/          # Contact entity
├── Data/            # SQLite repository (CRUD operations)
├── ViewModels/      # MVVM view models and commands
├── MainWindow.xaml  # Main UI
└── App.xaml         # Global styles and resources
```

## Usage

1. Click **New** to create a contact
2. Fill in the form (first and last name are required)
3. Click **Save** to persist the contact
4. Select a contact from the list to edit it
5. Use the search box to filter contacts
6. Click **Delete** to remove the selected contact
