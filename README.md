# City Info Project

This project is a web application built using .NET Core 8 and C# 12, following the MVC (Model-View-Controller) architecture. The application provides information about various cities, including details such as current temperature, population, and other relevant data.

## Technologies Used

- **.NET Core 8**
- **C# 12**
- **MVC Architecture**
- **Entity Framework Core** (for database access)
- **ASP.NET Core** (for web application framework)
- **SQL Server** (database)

## Project Structure

The project follows the MVC architecture, which is divided into three main components:

1. **Models**: Represent the data and business logic of the application.
2. **Views**: Handle the display of data and user interface.
3. **Controllers**: Manage the flow of the application and handle user input.

## Getting Started

### Prerequisites

- .NET Core 8 SDK
- Visual Studio 2022 or later
- SQL Server

### Installation

1. Clone the repository:
git clone https://github.com/maksimovicnikola/city-info.git

2. Navigate to the project folder: 
cd city-info

3. Restore the dependencies:
dotnet restore

4. Update the database connection string in `appsettings.json`:
"ConnectionStrings": { "DefaultConnection": "Server=your_server;Database=CityInfoDb;Trusted_Connection=True;MultipleActiveResultSets=true" }

5. Apply the database migrations:
dotnet ef database update

6. Run the application:
dotnet run


### Usage

- Navigate to `https://localhost:7095` in your web browser to access the application.
- Use the navigation menu to explore different cities and their information.

## Contributing

Contributions are welcome! Please fork the repository and create a pull request with your changes.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for more details.

## Contact

For any questions or suggestions, please contact (mailto:nikola.maksimovic.jobs@gmail.com).
