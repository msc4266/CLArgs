﻿using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;



namespace MSPro.CLArgs
{
    /// <summary>
    ///     All arguments as they were provided in the command-line.
    /// </summary>
    /// <remarks>
    ///     An <c>Argument</c> is either a <see cref="Verbs" />
    ///     or an <see cref="Option" />.
    /// </remarks>
    [PublicAPI]
    public class CommandLineArguments
    {
        private readonly List<Option> _allCommandLineOptions;
        private readonly StringComparison _comparer;


        internal CommandLineArguments(string commandLine, bool ignoreCase = false)
        {
            this.CommandLine = commandLine;

            _comparer = ignoreCase 
                ? StringComparison.InvariantCultureIgnoreCase
                : StringComparison.InvariantCulture;

            IEqualityComparer<string> c = ignoreCase
                ? StringComparer.InvariantCultureIgnoreCase
                : StringComparer.InvariantCulture;
            this.Verbs = new HashSet<string>(c);
            this.Targets = new HashSet<string>(c);
            _allCommandLineOptions = new List<Option>();
        }



        /// <summary>
        ///     The full command-line as it was parsed.
        /// </summary>
        public string CommandLine { get; }


        /// <summary>
        ///     The list of verbs in the sequence order
        ///     as they were provided in the command-line.
        /// </summary>
        public HashSet<string> Verbs { get; }

        /// <summary>
        ///     The list of Targets in the sequence order
        ///     as they were provided in the command-line.
        /// </summary>
        public HashSet<string> Targets { get; }



        /// <summary>
        ///     All Verbs as a '.' concatenated list - namespaced verbs.
        /// </summary>
        /// <returns>
        ///     All <see cref="Verbs" /> concatenated by '.', for example, 'HelloWorld.Germany'.<br />
        ///     <c>null</c> in case, no verb was provided in the command-line.
        /// </returns>
        public string VerbPath => this.Verbs.Count == 0 ? null : string.Join(".", this.Verbs);

        /// <summary>
        ///     A key-value list of all options provided in the command-line.
        /// </summary>
        /// <remarks>
        ///     All option values are <c>strings</c> in the first instance.
        ///     Conversion may happen later.
        /// </remarks>
        public IEnumerable<Option> Options => _allCommandLineOptions;

        public bool OptionTagProvided(string optionTag) => _allCommandLineOptions.Any(o => o.Key.Equals(optionTag, _comparer));

        public void AddVerb(string verb) => this.Verbs.Add(verb);
        public void AddTarget(string target) => this.Targets.Add(target);



        /// <inheritdoc cref="AddOption(MSPro.CLArgs.Option)" />
        public void AddOption(Option option) => _allCommandLineOptions.Add( option);



        /// <summary>
        ///     Manually add or update an option.
        /// </summary>
        /// <remarks>Options are unique by their <see cref="Option.Key" /></remarks>
        public void AddOption(string tag, string value) => AddOption(new Option(tag, value));
    }
}