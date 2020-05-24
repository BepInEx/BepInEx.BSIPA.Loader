﻿using System;
using System.IO;

namespace IPA.Logging.Printers
{
    /// <summary>
    /// A printer for all messages to a unified log location.
    /// </summary>
    public class GlobalLogFilePrinter : GZFilePrinter
    {
        /// <summary>
        /// Provides a filter for this specific printer.
        /// </summary>
        /// <value>the filter level for this printer</value>
        public override Logger.LogLevel Filter { get; set; } = Logger.LogLevel.All;

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