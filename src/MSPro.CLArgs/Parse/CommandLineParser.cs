﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;



namespace MSPro.CLArgs
{
    /// <summary>
    ///     Provides the functionality to parse a command-line
    /// </summary>
    /// <see cref="CommandLineParser" />
    public class CommandLineParser
    {
        /// <summary>
        ///     Shortcut and preferred way to use CommandLineParser.
        /// </summary>
        /// <remarks>
        ///     Same as <code>new CommandLineParser(settings).Run(args);</code>.
        /// </remarks>
        public static CommandLineArguments Parse(string[] args, Settings settings = null)
        {
            settings ??= new Settings();

            string commandLine = string.Join(" ", args);
            CommandLineArguments commandLineArguments = new(commandLine, settings.IgnoreCase);
            var sp = new CommandLineParser(settings.OptionsTags, settings.OptionValueTags);
            sp.Parse(commandLine, commandLineArguments);
            return commandLineArguments;
        }



        #region CommandLineParser

        private readonly char[] _optionsTags;
        private readonly char[] _optionValueTags;
        private string _argumentsString;
        private int _currentPos;



        /// <summary>
        ///     Create a new instance.
        /// </summary>
        /// <param name="optionsTags">
        ///     <see cref="Settings.OptionsTags" />
        /// </param>
        /// <param name="optionValueTags">
        ///     <see cref="Settings.OptionValueTags" />
        /// </param>
        private CommandLineParser(char[] optionsTags, char[] optionValueTags)
        {
            _optionsTags = optionsTags;
            _optionValueTags = optionValueTags;
        }



        /// <summary>
        ///     Parse a string containing arguments
        /// </summary>
        private void Parse(string argumentsString, CommandLineArguments commandLineArguments)
        {
            // When providing a Target there must have been at
            // least one Option to distinguish between verbs and options.
            bool optionRead = false;

            _currentPos = 0;
            _argumentsString = argumentsString;

            while (_currentPos < _argumentsString.Length)
            {
                char c = _argumentsString[_currentPos];
                if (_optionsTags.Any(tag => c == tag))
                {
                    optionRead = true;
                    commandLineArguments.AddOption(getOption());
                }
                else if (c == ' ')
                {
                    _currentPos++;
                }
                else if (c == '@')
                {
                    optionRead = true;
                    readFromFile(commandLineArguments);
                }
                else if (!optionRead && char.IsLetter(c))
                {
                    commandLineArguments.AddVerb(readString());
                }
                else if (optionRead && (char.IsLetter(c) || c == '\'' || c == '"'))
                {
                    commandLineArguments.AddTarget(readString());
                }
                else
                    throw new ApplicationException(
                        $"Unexpected character '{c}' in commandline, pos {_currentPos}.");
            }
        }



        private void readFromFile(CommandLineArguments commandLineArguments)
        {
            _currentPos++;
            string fileName = getFileName();
            string filePath = Helper.FindFile( fileName, 
                Environment.CurrentDirectory,
                Helper.BinDir,null, 
                true);

            var args = readArgs(filePath);
            CommandLineParser sp = new(_optionsTags, _optionValueTags);
            sp.Parse(string.Join(" ", args), commandLineArguments);
        }



        private string getFileName()
        {
            char firstChar = _argumentsString[_currentPos];
            return firstChar == '"' || firstChar == '\''
                ? readStringQuoted()
                : readUntil(Path.GetInvalidPathChars().Concat(Path.GetInvalidFileNameChars()).ToArray());
        }



        private IEnumerable<string> readArgs(string fileName)
        {
            string[] lines = File.ReadAllLines(fileName);
            return lines.Select(line => line.Trim())
                .Where(trimmed => !string.IsNullOrWhiteSpace(trimmed) && !trimmed.StartsWith("//") &&
                                  !trimmed.StartsWith("#"));
        }



        private Option getOption()
        {
            // Name starts at first char that is not an optionsNameIdent
            skipChars(_optionsTags);
            Option optionTag = new(getOptionName());

            // 2021-04-11: getValue
            skipChars(_optionValueTags);
            // Value starts, if not the next option already
            if (_argumentsString.Length > _currentPos && _argumentsString[_currentPos] != ' ')
            {
                // an option value was provided
                optionTag.Value = readString();
            }
            else optionTag.Value = true.ToString();

            return optionTag;
        }

        
        private string getOptionName()
        {
            int startPos = _currentPos;
            while (_currentPos < _argumentsString.Length && isOptionNameChar(_argumentsString[_currentPos]))
                _currentPos++;
            return _argumentsString.Substring(startPos, _currentPos - startPos);
        }



        bool isOptionNameChar(char c) => 
            (char.IsLetterOrDigit(c) || c=='_')
            && !_optionValueTags.Contains(c) 
            && !_optionsTags.Contains(c);



        private string readUntil(char[] breaker)
        {
            int startPos = _currentPos;
            while (_currentPos < _argumentsString.Length && !breaker.Contains(_argumentsString[_currentPos]))
                _currentPos++;
            return _argumentsString.Substring(startPos, _currentPos - startPos);
        }



        private void skipChars(char[] ignoreChars)
        {
            while (_currentPos < _argumentsString.Length
                   && ignoreChars.Any(sc => sc == _argumentsString[_currentPos]))
                _currentPos++;
        }



        private string readString()
        {
            skipChars(new[] {' '});
            char firstChar = _argumentsString[_currentPos];
            return firstChar == '"' || firstChar == '\''
                ? readStringQuoted()
                : readUntil(new[] {' '});
        }



        private string readStringQuoted()
        {
            char stringToken = _argumentsString[_currentPos]; // string token " or '
            _currentPos++; // skip token

            // don't use Substring but collect all characters to support escaping
            List<char> chars = new();

            // Iterate until the second token is found
            while (_argumentsString[_currentPos] != stringToken)
            {
                /*
                // msc, 2020-07-15: Escaping removed, kept collecting chars
                // skip escape char, collect next char
                if (arguments[_currentPos] == '\\')
                {
                    if (++_currentPos >= arguments.Length)
                        throw new ApplicationException("Unexpected end of string after escape character");
                }
                */

                chars.Add(_argumentsString[_currentPos]);
                if (++_currentPos >= _argumentsString.Length)
                    throw new ApplicationException($"Unexpected end of string, missing closing {stringToken}.");
            }

            _currentPos++; // skip last char (second string token)
            return string.Join("", chars);
        }

        #endregion
    }
}