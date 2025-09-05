# API Pagination Query Tool

A modern Blazor application for querying paginated APIs with authentication and saving all data to files, available both as a web application and desktop application using .NET MAUI.

## 🚀 Features

- **Flexible Authentication**: Support for Bearer tokens or OAuth2 client credentials flow
- **Paginated API Querying**: Automatically retrieves all pages of data from paginated APIs
- **Real-time Progress**: Live progress updates during data retrieval
- **Data Export**: Automatically saves all retrieved data to JSON files
- **Connection Testing**: Test API connectivity before running full queries
- **Cross-Platform**: Available as web app and desktop application

## 📁 Project Structure

```
PaginationTool/
├── PaginationTool.Web/                # Blazor Web Application
│   ├── PaginationTool.Web.csproj
│   ├── Components/
│   ├── Properties/
│   ├── wwwroot/
│   └── Program.cs
├── PaginationTool.Maui/               # .NET MAUI Desktop Application
│   ├── PaginationTool.Maui.csproj
│   ├── Components/
│   ├── Platforms/
│   └── Resources/
├── PaginationTool.Shared/             # Shared Components Library
│   ├── PaginationTool.Shared.csproj
│   └── PaginationToolComponent.razor
└── PaginationTool.sln                 # Solution File
```

## 🏗️ Architecture

- **PaginationTool.Shared**: Contains the main `PaginationToolComponent.razor` and services for API querying, authentication, and data management
- **PaginationTool.Web**: Blazor Server application that hosts the shared component for web access
- **PaginationTool.Maui**: .NET MAUI Blazor Hybrid application that hosts the same component for desktop access

### Core Services

- **TokenService**: Handles Bearer token authentication and OAuth2 client credentials flow
- **HttpService**: Makes authenticated HTTP requests with proper headers
- **QueryService**: Manages paginated data retrieval and aggregation
- **JsonService**: Handles data serialization and file operations

## 🛠️ Getting Started

### Prerequisites

- .NET 9.0 SDK
- For MAUI development: .NET MAUI workload (`dotnet workload install maui`)

### Running the Web Application

```bash
# Navigate to the project root
cd PaginationTool

# Run the web application
dotnet run --project PaginationTool.Web\PaginationTool.Web.csproj
```

The web application will be available at `https://localhost:5001` or `http://localhost:5000`.

### Running the Desktop Application

```bash
# Navigate to the MAUI project
cd PaginationTool.Maui

# Run the desktop application
dotnet run -f net9.0-windows10.0.19041.0
```

### Building the Solution

```bash
# Build all projects
dotnet build PaginationTool.sln
```

## 🎯 Usage

### Authentication Options

Choose between two authentication methods:

1. **Bearer Token**: Use an existing token directly
   - Simply paste your Bearer token into the text area

2. **Client Credentials Flow**: Automatically obtain token using OAuth2
   - Provide Token Endpoint, Client ID, and Client Secret
   - The application will automatically request and cache tokens

### API Configuration

1. **API URL**: The base URL for your paginated API endpoint
2. **Tenant ID**: Value for the `x-raet-tenant-id` header
3. **Page Size**: Number of items to request per page (default: 100)

### Querying Data

1. **Test Connection**: Verify your configuration by testing the API connection
2. **QUERY**: Start the full data retrieval process
3. **Monitor Progress**: Watch real-time progress updates
4. **View Results**: See statistics and file location when complete

### API Requirements

Your API should:
- Return JSON with a `value` array containing data
- Include a `nextLink` property for pagination (optional)
- Support `take` query parameter for page size
- Support `nextToken` query parameter for next page

Example API response:
```json
{
  "value": [
    { "id": 1, "name": "Item 1" },
    { "id": 2, "name": "Item 2" }
  ],
  "nextLink": "/companies?nextToken=ABC123&take=2"
}
```

## 🎨 Features Highlights

- **Modern UI**: Clean, responsive design using Bootstrap 5
- **Real-time Progress**: Live updates during data retrieval
- **Error Handling**: Comprehensive error reporting and handling
- **File Management**: Automatic saving to Downloads/PaginationTool/ directory
- **Cross-Platform**: Same functionality in web and desktop environments

## 🔧 Technical Details

- **Framework**: .NET 9.0
- **UI Framework**: Blazor Server (Web) / Blazor Hybrid (MAUI)
- **Styling**: Bootstrap 5
- **Architecture**: Shared component library pattern
- **Platforms**: Web browsers, Windows desktop

## 📝 License

This project is created for demonstration purposes.
