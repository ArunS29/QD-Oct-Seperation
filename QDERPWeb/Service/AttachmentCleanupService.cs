using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class AttachmentCleanupService : BackgroundService
{
    private readonly string _attachmentsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "attachments");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (Directory.Exists(_attachmentsFolder))
                {
                    var files = Directory.GetFiles(_attachmentsFolder);
                    foreach (var file in files)
                    {
                        var creationTime = File.GetCreationTimeUtc(file);
                        if ((DateTime.UtcNow - creationTime).TotalHours > 10)
                        {
                            File.Delete(file);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log exception (optional)
                Console.WriteLine("Error during attachment cleanup: " + ex.Message);
            }

            // Wait 10 hours before next check
            await Task.Delay(TimeSpan.FromHours(10), stoppingToken);
        }
    }
}
