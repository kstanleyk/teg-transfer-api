using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Parsing;
using Serilog.Sinks.PostgreSQL;
using System.Diagnostics;

namespace Core.Logger;

public static class Logger
{
    private static readonly ILogger PerfLogger;
    private static readonly ILogger UsageLogger;
    private static readonly ILogger ErrorLogger;
    private static readonly ILogger DiagnosticLogger;

    static Logger()
    {
        var appSettings = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var connectionString = appSettings.GetConnectionString("PostgresLogger");

        var columnOptions = GetPostgresColumnOptions();

        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext();

        PerfLogger = loggerConfig
            .WriteTo.PostgreSQL(connectionString, "perf_logs", columnOptions,
                restrictedToMinimumLevel: LogEventLevel.Information)
            .CreateLogger();

        UsageLogger = loggerConfig
            .WriteTo.PostgreSQL(connectionString, "usage_logs", columnOptions,
                restrictedToMinimumLevel: LogEventLevel.Information)
            .CreateLogger();

        ErrorLogger = loggerConfig
            .WriteTo.PostgreSQL(connectionString, "error_logs", columnOptions,
                restrictedToMinimumLevel: LogEventLevel.Error)
            .CreateLogger();

        DiagnosticLogger = loggerConfig
            .WriteTo.PostgreSQL(connectionString, "diagnostic_logs", columnOptions,
                restrictedToMinimumLevel: LogEventLevel.Information)
            .CreateLogger();

        Serilog.Debugging.SelfLog.Enable(msg => Debug.WriteLine(msg));
    }

    private static IDictionary<string, ColumnWriterBase> GetPostgresColumnOptions()
    {
        return new Dictionary<string, ColumnWriterBase>
        {
            {"timestamp", new TimestampColumnWriter()},
            {"product", new SinglePropertyColumnWriter("product")},
            {"layer", new SinglePropertyColumnWriter("layer")},
            {"location", new SinglePropertyColumnWriter("location")},
            {"message", new SinglePropertyColumnWriter("message")},
            {"hostname", new SinglePropertyColumnWriter("hostname")},
            {"userid", new SinglePropertyColumnWriter("userid")},
            {"username", new SinglePropertyColumnWriter("username")},
            {"exception", new SinglePropertyColumnWriter("exception")},
            {"elapsedmilliseconds", new SinglePropertyColumnWriter("elapsedmilliseconds", PropertyWriteMethod.ToString, NpgsqlTypes.NpgsqlDbType.Bigint)},
            {"correlationid", new SinglePropertyColumnWriter("correlationid")},
            {"customexception", new SinglePropertyColumnWriter("customexception")},
            {"additionalinfo", new SinglePropertyColumnWriter("additionalinfo", PropertyWriteMethod.ToString, NpgsqlTypes.NpgsqlDbType.Jsonb)},
            {"client", new SinglePropertyColumnWriter("client")},
            {"branchcode", new SinglePropertyColumnWriter("branchcode")}
        };
    }

    public static void WritePerf(LogDetail infoToLog)
    {
        WriteLog(PerfLogger, infoToLog);
    }

    public static void WriteUsage(LogDetail infoToLog)
    {
        WriteLog(UsageLogger, infoToLog);
    }

    public static void WriteError(LogDetail infoToLog)
    {
        var procName = FindProcName(infoToLog.Exception);
        infoToLog.Location = string.IsNullOrEmpty(procName) ? infoToLog.Location : procName;
        infoToLog.Message = GetMessageFromException(infoToLog.Exception);

        WriteLog(ErrorLogger, infoToLog);
    }

    public static void WriteDiagnostic(LogDetail infoToLog)
    {
        var writeDiagnostics = Convert.ToBoolean(Environment.GetEnvironmentVariable("EnableDiagnostics") ?? "false");
        if (!writeDiagnostics) return;

        WriteLog(DiagnosticLogger, infoToLog);
    }

    private static void WriteLog(ILogger logger, LogDetail detail)
    {
        var logEvent = new LogEvent(
            detail.Timestamp,
            LogEventLevel.Information,
            null,
            MessageTemplate,
            detail.ToLogEventProperties());

        logger.Write(logEvent);
    }

    private static MessageTemplate MessageTemplate { get; } =
        new MessageTemplate("Log event", new List<MessageTemplateToken>());

    private static List<LogEventProperty> ToLogEventProperties(this LogDetail detail)
    {
        return
        [
            new LogEventProperty("product", new ScalarValue(detail.Product)),
            new LogEventProperty("layer", new ScalarValue(detail.Layer)),
            new LogEventProperty("location", new ScalarValue(detail.Location)),
            new LogEventProperty("message", new ScalarValue(detail.Message)),
            new LogEventProperty("hostname", new ScalarValue(detail.Hostname)),
            new LogEventProperty("userid", new ScalarValue(detail.UserId)),
            new LogEventProperty("username", new ScalarValue(detail.UserName)),
            new LogEventProperty("exception", new ScalarValue(detail.Exception.ToBetterString())),
            new LogEventProperty("elapsedmilliseconds", new ScalarValue(detail.ElapsedMilliseconds)),
            new LogEventProperty("correlationid", new ScalarValue(detail.CorrelationId)),
            new LogEventProperty("customexception", new ScalarValue(detail.CustomException.ToString())),
            new LogEventProperty("additionalinfo", new ScalarValue(SerializeAdditionalInfo(detail.AdditionalInfo))),
            new LogEventProperty("client", new ScalarValue(detail.Client)),
            new LogEventProperty("branchcode", new ScalarValue(detail.BranchCode))
        ];
    }

    private static string SerializeAdditionalInfo(Dictionary<string, object>? additionalInfo)
    {
        return additionalInfo != null ?
            System.Text.Json.JsonSerializer.Serialize(additionalInfo) : "{}";
    }

    private static string GetMessageFromException(Exception ex)
    {
        return ex.InnerException != null ?
            GetMessageFromException(ex.InnerException) : ex.Message;
    }

    private static string? FindProcName(Exception ex)
    {
        // Simplified - remove SQL Server specific code
        if (ex.InnerException != null)
            return FindProcName(ex.InnerException);

        return null;
    }
}

//CREATE TABLE IF NOT EXISTS perf_logs(
//    timestamp TIMESTAMP,
//    product TEXT,
//    layer TEXT,
//    location TEXT,
//    message TEXT,
//    hostname TEXT,
//    userid TEXT,
//    username TEXT,
//    exception TEXT,
//    elapsedmilliseconds BIGINT,
//    correlationid TEXT,
//    customexception TEXT,
//    additionalinfo JSONB,
//    client TEXT,
//    branchcode TEXT
//);

//-- Create similar tables for: usage_logs, error_logs, diagnostic_logs