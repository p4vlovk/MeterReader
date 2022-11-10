using MeterReader.Interceptors;
using MeterReader.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(8888, o => o.Protocols = HttpProtocols.Http1);
    options.ListenLocalhost(8889, o => o.Protocols = HttpProtocols.Http2);
    // options.ConfigureHttpsDefaults(o => o
    //     .ClientCertificateMode = ClientCertificateMode.AllowCertificate); // without this setting the server is not looking for certificates;
                                                                          // this tells Kestrel to accept the certificate and pass it down
                                                                          // the middleware pipeline
});

RegisterServices(builder);

var app = builder.Build();

SetupMiddleware(app);
SetupApi(app);

app.Run();

static void SetupApi(IEndpointRouteBuilder app)
{
    // Token Generation
    app.MapPost("/api/token", async Task<IResult>(CredentialModel model, JwtTokenValidationService tokenService) =>
    {
        var result = await tokenService.GenerateTokenModelAsync(model);

        return result.Success
            ? Results.Created("", new {token = result.Token, expiration = result.Expiration})
            : Results.BadRequest();
    }).AllowAnonymous();

    // REST API
    app.MapGet("/api/customers", async Task<IResult>(IReadingRepository repo) =>
    {
        var result = await repo.GetCustomersWithReadingsAsync();

        return Results.Ok(result);
    });

    app.MapGet("/api/customers/{id:int}", async Task<IResult>(int id, IReadingRepository repo) =>
    {
        var result = await repo.GetCustomerWithReadingsAsync(id);

        return Results.Ok(result);
    });
}

static void SetupMiddleware(WebApplication webApp)
{
    if (webApp.Environment.IsDevelopment())
    {
        webApp.UseMigrationsEndPoint();
    }
    else
    {
        webApp
            .UseExceptionHandler("/Error")
            .UseHsts()
            .UseHttpsRedirection();
    }

    webApp
        .UseStaticFiles()
        .UseRouting()
        .UseCors()
        .UseAuthentication()
        .UseAuthorization();

    webApp.MapRazorPages();
    webApp.MapGrpcService<MeterReadingService>();
}


static void RegisterServices(WebApplicationBuilder builder)
{
    builder
        .Services
        .AddDbContext<ReadingContext>(options => options
            .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
            .UseSnakeCaseNamingConvention())
        .AddDatabaseDeveloperPageExceptionFilter()
        .AddCors(cfg => cfg.AddPolicy("AllowAll", options => options
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()))
        .AddScoped<IReadingRepository, ReadingRepository>()
        .AddScoped<JwtTokenValidationService>()
        .AddAuthentication()
        .AddJwtBearer(options => options
            .TokenValidationParameters = new MeterReaderTokenValidationParameters(builder.Configuration));
        // .AddCertificate(options =>
        // {
        //     options.AllowedCertificateTypes = CertificateTypes.All;
        //     options.RevocationMode = X509RevocationMode.NoCheck; // NoCheck is used only for self-signed certificates in development
        //     options.Events = new CertificateAuthenticationEvents
        //     {
        //         OnCertificateValidated = context =>
        //         {
        //             if (context.ClientCertificate.Issuer == "CN=MeterRootCert")
        //             {
        //                 context.Success();
        //             }
        //             else
        //             {
        //                 context.Fail("Invalid certificate issuer");
        //             }
        //
        //             return Task.CompletedTask;
        //         }
        //     };
        // });

    builder
        .Services
        .AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
        .AddEntityFrameworkStores<ReadingContext>();

    builder.Services.AddRazorPages();
    builder.Services.AddGrpc(options =>
    {
        if (builder.Environment.IsDevelopment())
        {
            options.EnableDetailedErrors = true;
        }
        
        options.Interceptors.Add<ExceptionInterceptor>();
    });
}