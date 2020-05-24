using BepInEx;
using BepInEx.Logging;
using IPA.Config;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.IO;

namespace IPA.Logging
{
    /// <summary>
    /// The default (and standard) <see cref="Logger"/> implementation.
    /// </summary>
    /// <remarks>
    /// <see cref="StandardLogger"/> uses a multi-threaded approach to logging. All actual I/O is done on another thread,
    /// where all messaged are guaranteed to be logged in the order they appeared. It is up to the printers to format them.
    ///
    /// This logger supports child loggers. Use <see cref="LoggerExtensions.GetChildLogger"/> to safely get a child.
    /// The modification of printers on a parent are reflected down the chain.
    /// </remarks>
    public class StandardLogger : Logger
    {
        /// <summary>
        /// The <see cref="TextWriter"/> for writing directly to the console window, or stdout if no window open.
        /// </summary>
        /// <value>a <see cref="TextWriter"/> for the current primary text output</value>
        public static TextWriter ConsoleWriter { get; internal set; } = ConsoleManager.ConsoleStream ?? ConsoleManager.StandardOutStream;

        private readonly string logName;

        protected ManualLogSource BepInExLogger { get; set; }

        /// <summary>
        /// All levels defined by this filter will be sent to loggers. All others will be ignored.
        /// </summary>
        /// <value>the global filter level</value>
        public static LogLevel PrintFilter { get; internal set; } = LogLevel.All;

        private readonly List<LogPrinter> printers = new List<LogPrinter>();
        private readonly StandardLogger parent;

        private readonly Dictionary<string, StandardLogger> children = new Dictionary<string, StandardLogger>();

        /// <summary>
        /// Configures internal debug settings based on the config passed in.
        /// </summary>
        internal static void Configure()
        {
            PrintFilter = SelfConfig.Debug_.ShowDebug_ ? LogLevel.All : LogLevel.InfoUp;
        }

        private StandardLogger(StandardLogger parent, string subName)
        {
            logName = $"{parent.logName}/{subName}";
            BepInExLogger = BepInEx.Logging.Logger.CreateLogSource(logName);
            this.parent = parent;
        }

        internal StandardLogger(string name)
        {
            logName = name;
            BepInExLogger = BepInEx.Logging.Logger.CreateLogSource(name);
        }

        /// <summary>
        /// Gets a child printer with the given name, either constructing a new one or using one that was already made.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>a child <see cref="StandardLogger"/> with the given sub-name</returns>
        internal StandardLogger GetChild(string name)
        {
            if (!children.TryGetValue(name, out var child))
            {
                child = new StandardLogger(this, name);
                children.Add(name, child);
            }

            return child;
        }

        /// <summary>
        /// Adds a log printer to the logger.
        /// </summary>
        /// <param name="printer">the printer to add</param>
        public void AddPrinter(LogPrinter printer)
        {
            printers.Add(printer);
        }

        /// <summary>
        /// Logs a specific message at a given level.
        /// </summary>
        /// <param name="level">the message level</param>
        /// <param name="message">the message to log</param>
        public override void Log(Level level, string message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            var time = Utils.CurrentTime();
            foreach (var printer in printers)
            {
                try
                {
                    if (((byte)level & (byte)printer.Filter) != 0)
                    {
                        printer.LastUse = time;
                        printer.Print(level, time, logName, message);
                    }
                }
                catch (Exception e)
                {
                    // do something sane in the face of an error
                    BepInExLogger.LogError($"printer errored: {e}");
                }
            }

            BepInExLogger.Log(ConvertToBepInExLogLevel(level), message);
        }

        internal static BepInEx.Logging.LogLevel ConvertToBepInExLogLevel(Level ipaLogLevel)
		{
			switch (ipaLogLevel)
			{
                default:
				case Level.None:
                    return BepInEx.Logging.LogLevel.None;
				case Level.Trace:
                    return BepInEx.Logging.LogLevel.Debug;
				case Level.Debug:
                    return BepInEx.Logging.LogLevel.Debug;
				case Level.Info:
                    return BepInEx.Logging.LogLevel.Info;
				case Level.Notice:
                    return BepInEx.Logging.LogLevel.Message;
				case Level.Warning:
                    return BepInEx.Logging.LogLevel.Warning;
				case Level.Error:
                    return BepInEx.Logging.LogLevel.Error;
                case Level.Critical:
                    return BepInEx.Logging.LogLevel.Fatal;
            }

		}
	}

	/// <summary>
	/// A class providing extensions for various loggers.
	/// </summary>
	public static class LoggerExtensions
    {
        /// <summary>
        /// Gets a child logger, if supported. Currently the only defined and supported logger is <see cref="StandardLogger"/>, and most plugins will only ever receive this anyway.
        /// </summary>
        /// <param name="logger">the parent <see cref="Logger"/></param>
        /// <param name="name">the name of the child</param>
        /// <returns>the child logger</returns>
        public static Logger GetChildLogger(this Logger logger, string name)
        {
            if (logger is StandardLogger standardLogger)
                return standardLogger.GetChild(name);

            throw new InvalidOperationException();
        }
    }
}