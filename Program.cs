var builder = WebApplication.CreateBuilder(args);
builder.Services.AddReverseProxy()
	.AddTransforms(builderContext =>
					{
						builderContext.CopyRequestHeaders = true;
						builderContext.AddOriginalHost(useOriginal: true);
						builderContext.UseDefaultForwarders = true;
					})
	.LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
builder.Services.AddLettuceEncrypt();

builder.WebHost.UseKestrel(k =>
{
	var appServices = k.ApplicationServices;
	k.ConfigureHttpsDefaults(h =>
	{
		h.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
		h.UseLettuceEncrypt(appServices);
	});
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.MapReverseProxy();
app.Run();