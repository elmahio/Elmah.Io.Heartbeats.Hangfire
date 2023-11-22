using Elmah.Io.Heartbeats.Hangfire;
using Elmah.Io.Heartbeats.Hangfire.AspNetCore80;
using Hangfire;
using Hangfire.MemoryStorage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHangfire(config => config
    // Remove the comment on the following line to install the elmah.io heartbeat filter as a global filter:
    //.UseFilter(new ElmahIoHeartbeatAttribute(builder.Configuration["ElmahIo:ApiKey"], builder.Configuration["ElmahIo:LogId"], builder.Configuration["ElmahIo:HeartbeatId"]))
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseMemoryStorage());
builder.Services.AddHangfireServer();

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

var recurringJobManager = app.Services.GetService<IRecurringJobManager>();
recurringJobManager.AddOrUpdate("Run every minute", () => Jobs.Test(), Cron.Minutely);

app.Run();
