# CMCS - Part 2 (Full functionality)

This package includes:
- ASP.NET Core MVC app with Identity (roles: Lecturer, ProgrammeCoordinator, AcademicManager)
- EF Core ApplicationDbContext with Claims DbSet
- Pages:
  - Lecturer: /Lecturer/Submit, /Lecturer/Track
  - Coordinator: /Coordinator/PreApprove
  - Manager: /Manager/Approve
- Account register/login with role selection
- Seeded sample users (see below)

## Sample users (seeded on startup)
- lecturer@test.com / Lecturer1!
- coordinator@test.com / Coordinator1!
- manager@test.com / Manager1!

## Setup
1. Open the solution in Visual Studio.
2. Ensure packages are installed:
   - Microsoft.EntityFrameworkCore
   - Microsoft.EntityFrameworkCore.SqlServer
   - Microsoft.AspNetCore.Identity.EntityFrameworkCore
   - Microsoft.EntityFrameworkCore.Tools
3. Update connection string in appsettings.json if needed.
4. Run migrations:
   - dotnet ef migrations add Initial --startup-project CMCS
   - dotnet ef database update --startup-project CMCS
5. Run the app. On first run roles and sample users will be created automatically.

