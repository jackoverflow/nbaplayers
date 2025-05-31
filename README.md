# NBA Players Management App

Welcome to the NBA Players Management App! This application allows you to manage a list of NBA players, including their personal details and status. Follow the instructions below to get started.

## Features

- Create a spreadsheet with player information.
- Request a list of NBA players with specific fields.
- Convert the spreadsheet to a PDF format.
- Upload the PDF to the application for processing.
- Automatically scrape player information and insert it into the database.
- Display the player data in a user-friendly table.

## Getting Started

### Prerequisites

Make sure you have the following installed on your machine:

- [.NET SDK](https://dotnet.microsoft.com/download) (version 9.0)
- A code editor (e.g., Visual Studio, Visual Studio Code)
- [PostgreSQL](https://www.enterprisedb.com/downloads/postgres-postgresql-downloads) (for database management)

### Creating the Spreadsheet

1. Create a new spreadsheet with the following column headers:
   - **Firstname**
   - **Lastname**
   - **DateOfBirth**
   - **Team**
   - **Retired**
   - **Injured**

2. Fill in the rows with the relevant data for each NBA player.

### Requesting Player Data

You can ask the AI for a list of NBA players by specifying the fields you need. The AI will provide you with the requested information.

### Converting to PDF

Once you have completed the spreadsheet, convert it to PDF format using your preferred method or software.

### Running the Application

To run the application, open your terminal and navigate to the project directory. Then, issue the following command:

```bash
dotnet watch run
```

### Uploading the PDF

After the application is running, upload the PDF file you created. The application will scrape the player information from the PDF and insert it into the database.

### Viewing the Data

Once the data has been processed, it will be displayed in a table format within the application.

## Enjoy!

Thank you for using the NBA Players Management App! If you have any questions or feedback, feel free to reach out.
