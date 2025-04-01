# DinamikCvSitesi - Dynamic CV/Resume Website

This project is a dynamic ASP.NET Core MVC application designed as a personal CV/Resume website. Users can manage their personal information, experiences, education, skills, and certificates through the site.

## Security Enhancements

The following security measures have been implemented in the project:

### Security Configurations in Program.cs

- **Cookie Security**: HttpOnly, SameSite, and Secure policies configured based on the environment.
- **Session Management**: Secure session management with a 30-minute timeout.
- **XSS Protection**: X-XSS-Protection headers added.
- **Content Type Sniffing Protection**: X-Content-Type-Options nosniff headers added.
- **Clickjacking Protection**: X-Frame-Options headers added.
- **Antiforgery Token Protection**: All forms protected against CSRF attacks.
- **SSL/HTTPS Configuration**: 
  - HTTP allowed in development environment
  - HTTPS enforced in production environment with HSTS enabled.
- **Content Security Policy**: Security policies defined for scripts, styles, fonts, and image sources in production environment.

### Authentication and Authorization

- **Cookie-Based Authentication**: Authentication with secure cookie settings.
- **Role-Based Authorization**: Special access controls for the Admin role.
- **Password Security**: Password hashing with SHA-256.

### Other Security Measures

- **File Upload Security**: Security controls for profile image uploads.
- **Login/Logout Operations**: Secure session start and end processes.
- **Validator Controls**: Server-side validation for all forms.
- **Error Management**: Secure error handling and logging.

## Admin Panel Login Information

You can use the following user credentials to access the admin panel:

**Default Admin:**
- Username: admin
- Password: Admin123!

**Test User:**
- Username: test
- Password: test123

## Project Features

- **Homepage**: Personal summary information and skills.
- **Resume/CV Page**: Display of education, experience, skills, and certificate information.
- **About Me Page**: Detailed personal information.
- **Contact Page**: Contact information.
- **Admin Panel**:
  - Edit profile information
  - Manage education information
  - Manage experience information
  - Manage skills
  - Manage certificates

## Technical Information

- **Framework**: ASP.NET Core 8.0 MVC
- **Database**: SQLite
- **ORM**: Entity Framework Core
- **Frontend**: Bootstrap 5, CSS, JavaScript
- **Deployment**: Can run on any web server

## Installation

1. Clone the repository
2. Install packages with the `dotnet restore` command
3. Run the application with the `dotnet run` command
4. Go to `http://localhost:5000` in your browser

## Development Notes

When working in the development environment:
- SSL-requiring security measures are automatically disabled
- All security measures become active when transitioning to the production environment 