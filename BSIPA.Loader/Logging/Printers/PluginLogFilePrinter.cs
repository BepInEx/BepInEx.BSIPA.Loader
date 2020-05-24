using System;

namespace IPA.Logging.Printers
{
    /// <summary>
    /// Prints log messages to the file specified by the name.
    /// </summary>
    public class PluginLogFilePrinter : GZFilePrinter
    {
        /// <summary>
        /// Provides a filter for this specific printer.
        /// </summary>
        /// <value>the filter level for this printer</value>
        public override Logger.LogLevel Filter { get; set; } = Logger.LogLevel.All;

        /// <summary>
        /// Creates a new printer with the given name.
        /// </summary>
        /// <param name="name">the name of the logger</param>
        public PluginLogFilePrinter(string name)
        {

        }

        /// <summary>
        /// Prints an entry to the associated file.
        /// </summary>
        /// <param name="level">the <see cref="Logger.Level"/> of the message</param>
        /// <param name="time">the <see cref="DateTime"/> the message was recorded at</param>
        /// <param name="logName">the name of the log that sent the message</param>
        /// <param name="message">the message to print</param>
        public override void Print(Logger.Level level, DateTime time, string logName, string message)
        {

        }
    }
}