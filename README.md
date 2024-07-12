# Simplify Your Workflow: End of Manual Configs! 🎉 Run .NET Web API with SQL Server in Docker 🐳✨
# Introduction
 We’re taking it up a notch by connecting our Web API to SQL Server and running everything as a Docker image! 🚀 Whether you're a seasoned developer or just starting out, this tutorial is packed with valuable insights to streamline your development process. Let's dive in 
 and see why this setup is a game-changer for modern web development! 🌐

# Scenario
Imagine you’re working on a complex application that requires a robust database solution. SQLite is great for lightweight applications and development, but as your application grows, you’ll need a more powerful database like SQL Server. Running SQL Server and your Web API in Docker containers allows you to create a consistent and isolated environment, making development, testing, and deployment smoother and more efficient. Plus, it simplifies the setup process across different machines and environments. 📈

# Do you Know What Docker Is?
Imagine you're a developer working on a complex web application. Your application needs to run on multiple developer machines, each with slightly different configurations. You might be using Windows, while your colleague uses macOS, and another team member uses Linux. Setting up the same development environment on each machine can be a nightmare. You need to ensure that every machine has the correct versions of .NET, SQL Server, and other dependencies. Even small differences in configurations can lead to the dreaded "it works on my machine" problem.
This is where Docker comes to the rescue! Docker allows you to package your application and its dependencies into a container, which is a lightweight, standalone, and executable package. Containers ensure that your application runs consistently across different environments.
When a new developer joins the project, they can get started by simply running a couple of Docker commands, which will set up the entire environment for them. No more lengthy setup instructions or troubleshooting environment-specific issues!
By using Docker, you streamline your development process, reduce setup time, and ensure consistency across different stages of development and deployment. It’s a powerful tool that simplifies complex tasks and makes your development workflow much more efficient.

# What To Cover
In this tutorial, we'll cover:
1. Setting up the Docker environment 🐳
2. We'll pull the SQL Server image from Docker Hub and configure it to run alongside our .NET Web API.
3. Configuring the Web API 🛠️
4. You'll learn how to modify your connection strings and set up the necessary configurations to connect your Web API to the SQL Server database.
5. Creating and running Docker containers 🚀
6. We’ll guide you through building Docker images for both your Web API and SQL Server, and then running them as containers.
7. Testing the setup ✅
8. Finally, we'll test the setup to ensure that your Web API can communicate with the SQL Server database seamlessly.

# Lets Start By Creating Connection String
    {
      "Logging": {
        "LogLevel": {
          "Default": "Information",
          "Microsoft.AspNetCore": "Warning"
        }
      },
      "AllowedHosts": "*",
      "ConnectionStrings": {
        "DefaultConnection": "Server=sqlserver;Database=YoutubeProductDb;User Id=sa;Password=Netcode2024;TrustServerCertificate=true;"
      }
    }

# Create Model
     public class Product
     {
         public int Id { get; set; }
         public string? Name { get; set; }
         public string? Description { get; set; }
         public int Qauntity { get; set; }
     }

 # Create DB Context
     public class ProductDbContext : DbContext
        {
            public ProductDbContext (DbContextOptions<ProductDbContext> options)
                : base(options)
            {
            }
    
            public DbSet<Product> Product { get; set; } = default!;
        }

# Create Product Endpoints
    public static class ProductEndpoints
    {
        public static void MapProductEndpoints (this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/Product").WithTags(nameof(Product));
    
            group.MapGet("/", async (ProductDbContext db) =>
            {
                return await db.Product.ToListAsync();
            })
            .WithName("GetAllProducts")
            .WithOpenApi();
    
            group.MapGet("/{id}", async Task<Results<Ok<Product>, NotFound>> (int id, ProductDbContext db) =>
            {
                return await db.Product.AsNoTracking()
                    .FirstOrDefaultAsync(model => model.Id == id)
                    is Product model
                        ? TypedResults.Ok(model)
                        : TypedResults.NotFound();
            })
            .WithName("GetProductById")
            .WithOpenApi();
    
            group.MapPut("/{id}", async Task<Results<Ok, NotFound>> (int id, Product product, ProductDbContext db) =>
            {
                var affected = await db.Product
                    .Where(model => model.Id == id)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(m => m.Id, product.Id)
                        .SetProperty(m => m.Name, product.Name)
                        .SetProperty(m => m.Description, product.Description)
                        .SetProperty(m => m.Qauntity, product.Qauntity)
                        );
                return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
            })
            .WithName("UpdateProduct")
            .WithOpenApi();
    
            group.MapPost("/", async (Product product, ProductDbContext db) =>
            {
                db.Product.Add(product);
                await db.SaveChangesAsync();
                return TypedResults.Created($"/api/Product/{product.Id}",product);
            })
            .WithName("CreateProduct")
            .WithOpenApi();
    
            group.MapDelete("/{id}", async Task<Results<Ok, NotFound>> (int id, ProductDbContext db) =>
            {
                var affected = await db.Product
                    .Where(model => model.Id == id)
                    .ExecuteDeleteAsync();
                return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
            })
            .WithName("DeleteProduct")
            .WithOpenApi();
        }
    }

# Create Docker File
    # See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.
    
    # This stage is used when running from VS in fast mode (Default for Debug configuration)
    FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
    USER app
    WORKDIR /app
    EXPOSE 8080
    
    # This stage is used to build the service project
    FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
    ARG BUILD_CONFIGURATION=Release
    WORKDIR /src
    COPY ["DemoWebAPIWithSqlServerInDocker/DemoWebAPIWithSqlServerInDocker.csproj", "DemoWebAPIWithSqlServerInDocker/"]
    RUN dotnet restore "./DemoWebAPIWithSqlServerInDocker/DemoWebAPIWithSqlServerInDocker.csproj"
    COPY . .
    WORKDIR "/src/DemoWebAPIWithSqlServerInDocker"
    RUN dotnet build "./DemoWebAPIWithSqlServerInDocker.csproj" -c $BUILD_CONFIGURATION -o /app/build
    
    # This stage is used to publish the service project to be copied to the final stage
    FROM build AS publish
    ARG BUILD_CONFIGURATION=Release
    RUN dotnet publish "./DemoWebAPIWithSqlServerInDocker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
    
    # This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
    FROM base AS final
    WORKDIR /app
    COPY --from=publish /app/publish .
    ENTRYPOINT ["dotnet", "DemoWebAPIWithSqlServerInDocker.dll"]

# Create Docker-Compose File
    services:
      webapi:
       build:
         context: .
         dockerfile: Dockerfile
       image: my_api_with_sqlserver_v2
       ports:
        - "5002:80"
       environment:
        - ASPNETCORE_URLS=http://+:80;
        - ASPNETCORE_ENVIRONMENT=Development
        - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=YoutubeProductDb;User Id=sa;Password=Netcode2024;TrustServerCertificate=true;
       depends_on:
         - sqlserver
    
      sqlserver:
        image: mcr.microsoft.com/mssql/server:2022-latest
        environment:
          SA_PASSWORD: "Netcode2024"
          ACCEPT_EULA: "Y"
        ports:
          - "1433:1433"
        volumes:
          - sqlserverdata:/var/opt/mssql 
        healthcheck:
          test: ["CMD-SHELL", "sqlcmd -Q 'SELECT 1' -S sqlserver -U sa -P Netcode2024"]
          interval: 10s
          retries: 10
    networks:
      default:
        name: my_custom_network
    
    volumes:
        sqlserverdata:

  # Create Migration Service
       public class MigrationService
     {
         public static void InitializeMigration(IApplicationBuilder app)
    
         {
             using var serviceScope = app.ApplicationServices.CreateScope();
             serviceScope.ServiceProvider.GetService<ProductDbContext>()!.Database.Migrate();
         }
     }

  # Register Database, Migration Service, CORS and Endpoint
    builder.Services.AddDbContext<ProductDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAllOrigins", builder =>
        {
            builder.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
    });
    var app = builder.Build();
    MigrationService.InitializeMigration(app);
    
    // Configure the HTTP request pipeline.
        app.UseSwagger();
        app.UseSwaggerUI();
    //app.UseHttpsRedirection();
    app.UseCors("AllowAllOrigins");
    app.MapProductEndpoints();
    app.Run();
    
# Run This Command In Console 
    docker-compose up --build

# Conclusion
And that’s it, folks! in this tutorial, we have created fully functional .NET Web API connected to SQL Server, all running smoothly in Docker containers. This setup not only enhances your development workflow but also prepares your application for scalable, production-ready deployments. 🌟

![image](https://github.com/user-attachments/assets/683881bb-f959-43e2-8268-a6f5c7377aa6)
![image](https://github.com/user-attachments/assets/c0bf5466-d6cc-469e-8052-2b02c52b21b4)

# Here's a follow-up section to encourage engagement and support for Netcode-Hub:
🌟 Get in touch with Netcode-Hub! 📫
1. GitHub: [Explore Repositories](https://github.com/Netcode-Hub/Netcode-Hub) 🌐
2. Twitter: [Stay Updated](https://twitter.com/NetcodeHub) 🐦
3. Facebook: [Connect Here](https://web.facebook.com/NetcodeHub) 📘
4. LinkedIn: [Professional Network](https://www.linkedin.com/in/netcode-hub-90b188258/) 🔗
5. Email: Email: [business.netcodehub@gmail.com](mailto:business.netcodehub@gmail.com) 📧
   
# ☕️ If you've found value in Netcode-Hub's work, consider supporting the channel with a coffee!
1. Buy Me a Coffee: [Support Netcode-Hub](https://www.buymeacoffee.com/NetcodeHub) ☕️
