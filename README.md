# ToDoListApi

- A simple to do list api project, built with
  - .NET 7
  - EntityFramework
  - MSSQL Server 2022
and hosted in AWS ECS Cluster

- Design Pattern used
  - Mediator (provided by IMediatR)

- Authentication
  - The API is protected by JWT Token

- Request Validation
    - All requests made to the API Endpoints validated automatically using FluentValidator AutoValidation middleware

- Database
    - The ORM used is EntityFramework
    - The database is scripted with the help of `dotnet-ef` tool

## Steps to Run at Local

- Prerequisite
  1. Download the folder to local
  2. Your machine should have installed .NET SDK, .NET Tool `dotnet-ef` and MSSQL Server

- Setup the Database
  1. From the downloaded root folder, navigate to `ToDoListApi`
  2. Open file `appsettings.Development.json`, modify the value of `ConnectionStrings.DefaultConnection` value to point to your local MSSQL Server 
  3. Open a terminal from the folder
  4. Run command `dotnet ef database update`

- Run the Application
  1. From the downloaded root folder, navigate to `ToDoListApi`
  2. Open a terminal
  3. Run command `dotnet run`
  4. You should see the swagger at `http://localhost:5188/swagger/index.html`

- API List
![API](api.png "API")

## Steps to Run Unit Test

1. From the downloaded root folder, navigate to `ToDoListApiTest`
2. Open a terminal
3. Run command `dotnet test`

## Deployment
Deployment are performed with the following tools and setup. Item #2 - # 5 are all hosted in AWS Cloud
1. Github
2. Jenkins
3. Amazon ECR
4. AWS ECS
5. AWS EC2

![Architecture](arch.png "Architecture")


