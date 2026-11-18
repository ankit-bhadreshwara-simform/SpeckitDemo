using SpeckitDemo.Api.Pipeline;

namespace SpeckitDemo.Api.Hosting;

public static class RequestPipeline
{
    public static WebApplication UseSpeckitDemoPipeline(this WebApplication app)
    {
        app.UseMiddleware<UnhandledErrorMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "SpeckitDemo Access v1"));
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        return app;
    }
}
