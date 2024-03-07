using Umbraco.Cms.Infrastructure.Runtime.RuntimeModeValidators;

var builder = WebApplication.CreateBuilder(args);
var umbracoBuilder = builder.CreateUmbracoBuilder();

umbracoBuilder
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers();
umbracoBuilder.RuntimeModeValidators()
    .Remove<UmbracoApplicationUrlValidator>()
    .Remove<UseHttpsValidator>();
umbracoBuilder.Build();

var app = builder.Build();

await app.BootUmbracoAsync();


app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        u.UseInstallerEndpoints();
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

await app.RunAsync();
