using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services
    .AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);

builder.Services.AddAuthorization();


builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();

var app = builder.Build();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseExceptionHandler("/error");

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/api/assessments/results", () => Results.Ok(new
{
    courseCode = "CS-101",
    studentId = "S-001",
    letterGrade = "A"
}))
.RequireAuthorization();

app.MapGet("/api/enrollments/worker-smoke", (EnrollmentWorker worker) =>
{
    worker.ProcessBatch();
    return Results.Ok("processed");
});

app.Map("/error", () => Results.Problem("An unexpected error occurred."));

app.MapPost("/api/enrollments/test", async (
    IEnrollmentService enrollmentService,
    string studentId,
    string courseCode) =>
{
    var record = await enrollmentService.EnrollAsync(studentId, courseCode);
    return Results.Ok(record);
});

app.MapGet("/api/enrollments/test/{id}", async (
    IEnrollmentService enrollmentService,
    string id) =>
{
    var record = await enrollmentService.GetByIdAsync(id);

    return record is null
        ? Results.NotFound()
        : Results.Ok(record);
});

app.MapDelete("/api/enrollments/test/{id}", async (
    IEnrollmentService enrollmentService,
    string id) =>
{
    var removed = await enrollmentService.DeleteAsync(id);

    return removed
        ? Results.Ok("deleted")
        : Results.NotFound();
});

app.Run();