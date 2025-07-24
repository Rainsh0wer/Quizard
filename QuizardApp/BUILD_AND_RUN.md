# Quizard Application - Cursor AI Rebuild

## Overview
This is a complete rebuild of the Quizard WPF application using the **Cursor AI pattern** - a centralized flow controller approach where `MainCursorViewModel` acts as a state machine managing all navigation and business logic.

## Architecture

### Cursor AI Pattern
- **MainCursorViewModel**: Central flow controller that manages all state transitions
- **AppState Enum**: Defines all possible application states
- **State Machine Logic**: Validates transitions and manages navigation stack
- **Dependency Injection**: Services are injected into ViewModels for better testability

### Key Features Implemented

#### Student Features:
- ✅ **Take Quiz**: Complete quiz-taking experience with timer and progress tracking
- ✅ **View Results**: Personal quiz results with detailed question breakdowns
- ✅ **Join Class**: Search and join classes by code or name
- ✅ **Search Subjects**: Browse and filter subjects with related quizzes

#### Teacher Features:
- ✅ **Create Quiz**: Full quiz creation with multiple-choice questions
- ✅ **View Results**: Analytics for all student attempts on teacher's quizzes
- ✅ **View Classes**: Class management with student progress tracking
- ✅ **Manage Students**: Add/remove students from classes

#### Shared Features:
- ✅ **Modern Authentication**: Login/Register with validation
- ✅ **Dashboard Analytics**: Role-specific dashboards with statistics
- ✅ **Responsive UI**: Modern design with loading states and error handling

## Build Instructions

### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 or Visual Studio Code
- SQL Server (LocalDB is sufficient)

### Setup Database
1. Update connection string in `appsettings.json`
2. Run the SQL script `MainDBQuizard.sql` to create the database
3. The application will use Entity Framework Core for data access

### Build and Run
```bash
cd QuizardApp
dotnet restore
dotnet build
dotnet run
```

### Demo Users
For testing purposes, use the demo buttons on the login screen:
- **Demo Student**: Username: `student1`, Password: `password123`
- **Demo Teacher**: Username: `teacher1`, Password: `password123`

## Project Structure
```
QuizardApp/
├── Models/                 # Entity Framework models
├── ViewModels/            # MVVM ViewModels with Cursor AI pattern
│   ├── MainCursorViewModel.cs    # Central flow controller
│   ├── *DashboardViewModel.cs    # Role-specific dashboards
│   ├── *FeatureViewModel.cs      # Feature-specific ViewModels
├── Views/                 # WPF Views and Templates
│   ├── MainWindow.xaml           # Main application window
│   ├── ViewTemplates.xaml        # DataTemplates for ViewModels
│   ├── Converters.cs            # XAML converters
├── Services/              # Business logic services
│   ├── NavigationService.cs      # Navigation management
│   ├── CurrentUserService.cs     # User session management
└── App.xaml.cs           # Application startup
```

## Key Design Decisions

### 1. Cursor AI Pattern
Instead of traditional page-based navigation, we use a central `MainCursorViewModel` that:
- Manages all application state via `AppState` enum
- Validates state transitions for security
- Maintains navigation history with back functionality
- Provides centralized error handling and loading states

### 2. ViewModel-First Navigation
- Views are created dynamically based on ViewModels
- `ViewModelTemplateSelector` maps ViewModels to DataTemplates
- No direct View dependencies in ViewModels

### 3. Async/Await Pattern
- All database operations are asynchronous
- Loading states are managed centrally
- UI remains responsive during data operations

### 4. Command Pattern
- All user actions are commands with can-execute logic
- Commands are bound to UI elements declaratively
- Validation is handled at the ViewModel level

## Features Breakdown

### Student Workflow
1. **Login** → **Student Dashboard**
2. **Take Quiz**: Select quiz → Answer questions → Submit → View results
3. **Join Class**: Search classes → Join by code → Access class quizzes
4. **View Results**: Review past quiz attempts with detailed breakdowns

### Teacher Workflow
1. **Login** → **Teacher Dashboard**
2. **Create Quiz**: Design questions → Set options → Assign to classes
3. **Manage Classes**: Create classes → Generate join codes → Monitor progress
4. **View Analytics**: Student results → Class performance → Quiz statistics

## Technology Stack
- **Frontend**: WPF with MVVM pattern
- **Backend Logic**: C# with async/await
- **Database**: Entity Framework Core with SQL Server
- **Architecture**: Cursor AI pattern (centralized flow control)
- **UI Framework**: Modern WPF with custom styles and animations

## Next Steps for Enhancement
1. **Real-time Features**: SignalR for live quiz sessions
2. **Advanced Analytics**: Charts and graphs for performance tracking
3. **Mobile Support**: Xamarin or MAUI for cross-platform
4. **Offline Mode**: Local caching for quiz-taking without internet
5. **Advanced Question Types**: Fill-in-the-blank, drag-and-drop, etc.

## Contributing
This architecture makes it easy to add new features:
1. Add new `AppState` to the enum
2. Create corresponding ViewModel
3. Add DataTemplate in `ViewTemplates.xaml`
4. Update `MainCursorViewModel.CreateViewModelForState()`

The Cursor AI pattern ensures all navigation and state management remains centralized and testable.