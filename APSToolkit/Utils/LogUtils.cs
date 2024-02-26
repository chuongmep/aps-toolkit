// Copyright (c) chuongmep.com. All rights reserved

using RestSharp;
using Serilog;
using Serilog.Events;

namespace APSToolkit.Utils;

public static class LogUtils
{
    private static string DirPath = Path.Combine(Path.GetTempPath(), "ForgeToolkit");
    private static string LogName = "ForgeToolkit"+DateTime.Now.ToString("yyyyMMdd")+".log";
    private static  string LogPath
    {
        get
        {
            if (!Directory.Exists(DirPath))
            {
                Directory.CreateDirectory(DirPath);
            }
            return Path.Combine(DirPath, LogName);
        }
    }
    public static void Info(string message)
    {
        // set log path
        using var log = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(LogPath,LogEventLevel.Information)
            .CreateLogger();
        log.Information(message);
    }
    public static void Error(string message)
    {
        // set log path
        using var log = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(LogPath,LogEventLevel.Information)
            .CreateLogger();
        log.Error(message);
    }
    /// <summary>
    /// Write report to console from report url
    /// </summary>
    /// <param name="reportUrl"></param>
    public static string? WriteConsoleWorkItemReport(string reportUrl)
    {
        var client = new RestClient(reportUrl);
        var request = new RestRequest() {Method = Method.Get};
        var response = client.Execute(request);
        if (response.IsSuccessful)
        {
            string? report = response.Content;
            Console.WriteLine(report);
            return report;
        }
        Console.WriteLine("Error: " + response.ErrorMessage);
        return string.Empty;
    }
}