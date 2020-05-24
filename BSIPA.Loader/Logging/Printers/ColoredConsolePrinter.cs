using System;

namespace IPA.Logging.Printers
{
    /// <summary>
    /// Prints a pretty message to the console.
    /// </summary>
    public class ColoredConsolePrinter : LogPrinter
    {
        /// <summary>
        /// A filter for this specific printer.
        /// </summary>
        /// <value>the filter to apply to this printer</value>
        public override Logger.LogLevel Filter { get; set; }
        /// <summary>
        /// The color to print messages as.
        /// </summary>
        /// <value>the color to print this message as</value>
        public ConsoleColor Color { get; set; }

        /// <summary>
        /// Prints an entry to the console window.
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