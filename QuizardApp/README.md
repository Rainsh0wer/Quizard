# Quizard - Quiz Learning Platform

A comprehensive quiz learning platform similar to Quizlet, built with C# WPF using MVVM pattern.

## Features

### 👨‍🏫 Teacher Features
- **Dashboard**: Overview of teaching activities with real-time statistics
- **Quiz Management**: Create and manage quizzes with multiple-choice questions
- **Subject Management**: Organize quizzes by subjects
- **Classroom Management**: Create classrooms and manage student enrollment
- **Results & Analytics**: View detailed student performance data

### 👨‍🎓 Student Features
- **Dashboard**: Personal learning overview with progress tracking
- **Quiz Taking**: Interactive quiz interface with immediate feedback
- **Results Tracking**: View quiz history and performance analytics
- **Subject Browser**: Explore quizzes by subject categories
- **Saved Quizzes**: Bookmark favorite quizzes for later

## Technology Stack

- **Framework**: .NET 8.0 WPF
- **Pattern**: MVVM (Model-View-ViewModel)
- **Database**: SQL Server with Entity Framework Core
- **UI**: Modern WPF with custom styling

## Database Setup

1. **Create Database**: Use the provided SQL script to create the Quizard database
2. **Connection String**: Update `appsettings.json` with your SQL Server connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=Quizard;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

## Installation & Setup

### Prerequisites
- Visual Studio 2022
- .NET 8.0 SDK
- SQL Server (LocalDB or full instance)

### Steps
1. **Clone/Download** the project
2. **Open** `QuizardApp.sln` in Visual Studio 2022
3. **Restore** NuGet packages
4. **Update** connection string in `appsettings.json`
5. **Run** the database script to create tables
6. **Build** and run the application

## Sample Data

To test the application with sample data:

1. Open `App.xaml.cs`
2. Uncomment the line: `SampleDataCreator.CreateSampleData();`
3. Run the application

This will create:
- Teacher account: `teacher1` / `password123`
- Student account: `student1` / `password123`
- Sample subjects, quizzes, and questions

## Project Structure

```
QuizardApp/
├── Models/                 # Entity Framework models
├── ViewModels/            # MVVM ViewModels
├── Views/                 # User Controls for content areas
├── Services/              # Navigation and user services
├── TestData/              # Sample data creation
├── *.xaml                 # Main pages (Login, Register, Dashboards)
└── appsettings.json       # Configuration
```

## Key Components

### Services
- **AppNavigationService**: Handles page navigation
- **CurrentUserService**: Manages user session state

### ViewModels
- **BaseViewModel**: Base class with INotifyPropertyChanged
- **LoginViewModel**: Authentication logic
- **TeacherDashboardViewModel**: Teacher interface management
- **StudentDashboardViewModel**: Student interface management

### Views
- **TeacherDashboardView**: Teacher statistics and overview
- **StudentDashboardView**: Student progress and activity
- **Various Management Views**: Subject, Quiz, Classroom management

## Usage

### For Teachers
1. **Login** with teacher credentials
2. **Create Subjects** to organize your quizzes
3. **Create Quizzes** with multiple-choice questions
4. **Manage Classrooms** and enroll students
5. **View Results** to track student performance

### For Students
1. **Login** with student credentials
2. **Browse Available Quizzes** by subject
3. **Take Quizzes** and get immediate feedback
4. **View Your Results** and track progress
5. **Save Favorite Quizzes** for quick access

## Development Notes

### MVVM Pattern
- **Models**: Entity classes representing database tables
- **Views**: XAML files for UI layout
- **ViewModels**: Business logic and data binding

### Navigation
- Uses custom `AppNavigationService` for page transitions
- Sidebar navigation with role-based menu items
- Automatic routing based on user role

### Styling
- Modern flat design with card-based layouts
- Consistent color scheme (Blue for teachers, Green for students)
- Responsive design with proper grid layouts

## Future Enhancements

- [ ] Quiz Timer functionality
- [ ] Question categories and tags
- [ ] Export quiz results to PDF/Excel
- [ ] Quiz sharing and collaboration
- [ ] Mobile-responsive design
- [ ] Advanced analytics and reporting
- [ ] Question import/export
- [ ] Multi-language support

## Contributing

1. Fork the project
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is for educational purposes. Feel free to use and modify as needed.

---

**Happy Learning with Quizard! 📚✨**