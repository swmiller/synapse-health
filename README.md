# SignalBooster - DME Data Extraction Tool

## Overview
SignalBooster is a utility that processes physician notes to extract information about patient durable medical equipment (DME) needs such as CPAPs, oxygen tanks, and wheelchairs. The application parses the notes, extracts structured data, and sends it to an external API endpoint.

## Development Environment

### Tools and IDE
- **IDE**: Visual Studio Code
- **Framework**: .NET 9.0
- **Version Control**: Git with GitHub
- **Testing Framework**: xUnit with Moq for mocking

### AI Development Tools
- **GitHub Copilot**: Used for code completion, documentation generation, and test case development
- **GitHub Copilot Chat/Agent**: Used for code review and refactoring suggestions

## Project Structure
The solution follows a clean architecture approach with clear separation of concerns:

```
solution/
├── SignalBooster/                # Main application
│   ├── Program.cs                # Application entry point
│   ├── Models/                   # Data models
│   ├── Services/                 # Business logic services
│   ├── Interfaces/               # Service interfaces
│   └── SignalBoosterEnums.cs     # Enumeration types
├── SignalBooster.Tests/          # Unit tests
```

## Key Features
- Extracts structured DME data from physician notes
- Supports CPAP, oxygen tank, and wheelchair device types
- Produces standardized JSON output
- Posts data to an external API endpoint
- Comprehensive error handling and logging
- Configurable file paths and API endpoints via appsettings.json

## Running the Application

### Prerequisites
- .NET 9.0 SDK or later

### Building and Running
1. Clone the repository
2. Navigate to the solution directory
3. Build the solution:
   ```
   dotnet build
   ```
4. Run the application:
   ```
   dotnet run --project SignalBooster
   ```

### Running Tests
Execute the test suite with:
```
dotnet test
```

## Assumptions and Limitations

### Assumptions
- Physician notes follow a semi-structured format with key fields like patient name, DOB, diagnosis, etc.
- The external API endpoint accepts JSON formatted data
- The endpoint URL is fixed at "https://alert-api.com/DrExtract" (configurable in appsettings.json)
- Notes are primarily in English and follow standard medical terminology

### Current Limitations
- Limited device type support (only CPAP, oxygen tank, and wheelchair)
- Basic text parsing without advanced NLP capabilities
- Synchronous file operations for simplicity
- No database persistence (stateless operation)

### Future Improvements
- Integrate with LLM models (OpenAI/Azure OpenAI) for more accurate extraction
- Support additional input formats (JSON, XML, structured reports)
- Expand device type coverage
- Implement validation rules for extracted data
- Add asynchronous batch processing for multiple notes
- Improve extraction accuracy with machine learning
- Add more comprehensive unit and integration tests

## Testing Strategy
The application includes unit tests covering:
- Physician note parsing logic
- Device type detection
- Data extraction
- JSON output format verification
- API client functionality with mocked HTTP responses
- End-to-end workflow validation

## Configuration
Application settings can be modified in appsettings.json:
```json
{
  "PhysicianNote": {
    "FilePath": "physician_note.txt"
  },
  "ApiSettings": {
    "BaseUrl": "https://alert-api.com"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```
