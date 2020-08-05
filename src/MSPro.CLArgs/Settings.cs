﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;



namespace MSPro.CLArgs
{
    public delegate void DisplayHelp(List<CommandDescriptor> commandDescriptors);

    [PublicAPI]
    public class Settings
    {
        /// <summary>
        ///     Get or set a list of characters that mark the end of an option's name.
        /// </summary>
        public char[] OptionValueTags = {' ', ':', '='};

        public TraceLevel TraceLevel { get; set; } = TraceLevel.Off;
        public Action<string> Trace { get; set; } = Console.WriteLine;


        public ValueConverters ValueConverters { get; } = new ValueConverters(); 

        public bool IgnoreCase { get; set; }

        /// <summary>
        ///     Get or set if unknown option tags provided in the command-line should be ignored.
        /// </summary>
        /// <remarks>
        ///     If set to <c>true</c> unknown options are ignored.<br />
        ///     Otherwise an error is added to the <see cref="ErrorDetailList">error collection</see>.
        /// </remarks>
        public bool IgnoreUnknownOptions { get; set; }

        /// <summary>
        ///     Automatically resolve commands using <see cref="CommandResolver" />
        /// </summary>
        public bool AutoResolveCommands { get; set; } = true;

        /// <summary>
        ///     Get or set an object to resolve all known commands (and verbs).
        /// </summary>
        /// <remarks>
        /// The default resolver is
        ///    <see cref="AssemblyCommandResolver"/>(<b>Assembly.GetEntryAssembly()</b>),
        /// to find all classes with [Command] annotation in the <c>EntryAssembly</c>.
        /// </remarks>
        public ICommandResolver CommandResolver { get; set; } =
            new AssemblyCommandResolver(Assembly.GetEntryAssembly());

        /// <summary>
        ///     Get or set tags which identify an option.
        /// </summary>
        /// <remarks>
        ///     A command-line argument that starts with any of these character
        ///     is considered to be an <c>Option</c>.
        /// </remarks>
        public char[] OptionsTags { get; set; } = {'-', '/'};


        public DisplayHelp DisplayHelp { get; set; } = commandDescriptors =>
        // Default Implementation
        {
            Console.WriteLine( $"{commandDescriptors.Count} Commands available.");
            foreach (CommandDescriptor commandDescriptor in commandDescriptors)
            {
                Console.WriteLine( $"{commandDescriptor.Verb}\t\t{commandDescriptor.Description}");
            }
        };
    }
}