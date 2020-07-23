﻿using System;
using System.IO;
using MSPro.CLArgs;



namespace Level2.ExtendedParameters
{
    internal partial class Program
    {
        private static void Main(string[] args)
        {
            // Use CommandResolver [=CurrentAssemblyResolver]
            // to find all classes with [Command] annotation
            Commander.Settings.AutoResolveCommands = true;
            Commander commander = new Commander(args);
            
            Console.WriteLine($"Command-Line: {commander.Arguments.CommandLine}");
            Console.WriteLine(">>> Start Main()");
            commander.ExecuteCommand();
            Console.WriteLine("<<< End Main()");
        }



        [Command("Default")]
        private class Command : CommandBase<CommandParameters>
        {
            public Command()
            {
                this.TypeConverters.Register(
                    typeof(FileInfo), (propertyName, optionValue) => new FileInfo(optionValue));
            }



            protected override void OnExecute(CommandParameters p)
            {
                Console.WriteLine($"UserName: {p.DBConnection.UserName}");
                Console.WriteLine($"DatabaseTableName: {p.DatabaseTableName}");
            }
        }
    }
}