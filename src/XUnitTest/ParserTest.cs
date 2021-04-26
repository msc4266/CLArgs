using System;
using System.Collections.Generic;
using System.Linq;
using MSPro.CLArgs;
using Xunit;
using Xunit.Abstractions;



namespace XUnitTest
{
    public class ParserTest
    {
        private readonly ITestOutputHelper _log;

        public ParserTest(ITestOutputHelper log)
        {
            _log = log;
        }

        [Fact]
        public void Print()
        {
            const string CMD_LINE = "DEPLOY /Package=\"Sprint 03\" --dst-env 01-DEV --DST-env 02-DEV /dn:\"Fixed API\" \"  Another target\"";
            string[] args = Win32.CommandLineToArgs(CMD_LINE);
            var clArgs = CommandLineParser.Parse(args);

            _log.WriteLine($"Verb-Count: {clArgs.Verbs.Count}");
            for (int i = 0; i < clArgs.Verbs.Count; i++)
            {
                _log.WriteLine($"Verb {i:d2}: #{clArgs.Verbs[i]}#");
            }
            _log.WriteLine($"Options-Count: {clArgs.Options.Count}");
            for (int i = 0; i < clArgs.Options.Count; i++)
            {
                _log.WriteLine($"Option {i:d2}: {clArgs.Options[i].Key}=#{clArgs.Options[i].Value}#");
            }
            _log.WriteLine($"Targets-Count: {clArgs.Targets.Count}");
            for (int i = 0; i < clArgs.Targets.Count; i++)
            {
                _log.WriteLine($"Targets {i:d2}: #{clArgs.Targets[i]}#");
            }
        }


        [Fact]
        public void Test1()
        {
            string[] args =
            {
                "DEPLOY",
                "EXEC",
                "/Package=",
                "2021-04.03_Sprint03\\iPack.jsonc",
                "/PackageDir=Packages",
                "/MyBool",
                "/DstEnv=01-DEV",
                "/dn=",
                "Fixed API parameter, removed trailing blank",
                "Target01",
                "Another target"
            };

            var clArgs = CommandLineParser.Parse(args);

            Assert.Equal(2, clArgs.Verbs.Count);
            var verbs = clArgs.Verbs.ToArray();
            Assert.Equal("DEPLOY", verbs[0]);
            Assert.Equal("EXEC", verbs[1]);
        }



      


        [Fact]
        public void Test2()
        {
            const string CMD_LINE =
                "DEPLOY EXEC /Package= \"2021-04.03_Sprint03\\iPack.jsonc\" /PackageDir=Packages /DstEnv=01-DEV /dn= \"Fixed API parameter, removed trailing blank\"" +
                " Target01 \"  Another target\" 'one more'";
            string[] args = Win32.CommandLineToArgs(CMD_LINE);

            var clArgs = CommandLineParser.Parse(args);

            Assert.Equal(2, clArgs.Verbs.Count);
            Assert.Equal(4, clArgs.Options.Count);
            Assert.Equal(4, clArgs.Targets.Count);

            var verbs = clArgs.Verbs.ToArray();
            Assert.Equal("DEPLOY", verbs[0]);
            Assert.Equal("EXEC", verbs[1]);

            Option packageOption = clArgs.Options.FirstOrDefault(o => o.Key == "Package");
            Assert.NotNull(packageOption);
            Assert.Equal(@"2021-04.03_Sprint03\iPack.jsonc", packageOption.Value);

            Option dnOption = clArgs.Options.FirstOrDefault(o => o.Key == "dn");
            Assert.NotNull(dnOption);
            Assert.Equal("Fixed API parameter, removed trailing blank", dnOption.Value);

            Assert.Equal("  Another target", clArgs.Targets[1]);
        }



        [Fact]
        public void Test3()
        {
            const string CMD_LINE = "Deploy Exec /Package='2021-04.03_Sprint03\\ipack.jsonc' /DstEnv=01-Dev ";
            string[] args = Win32.CommandLineToArgs(CMD_LINE);

            var clArgs = CommandLineParser.Parse(args);

            Assert.Equal(2, clArgs.Verbs.Count);
            Assert.Equal(2, clArgs.Options.Count);
            Assert.Empty(clArgs.Targets);

            var verbs = clArgs.Verbs.ToArray();
            Assert.Equal("Deploy", verbs[0]);
            Assert.Equal("Exec", verbs[1]);

            Option packageOption = clArgs.Options.FirstOrDefault(o => o.Key == "Package");
            Assert.NotNull(packageOption);
            Assert.Equal(@"'2021-04.03_Sprint03\ipack.jsonc'", packageOption.Value);

            Option dnOption = clArgs.Options.FirstOrDefault(o => o.Key == "DstEnv");
            Assert.NotNull(dnOption);
            Assert.Equal("01-Dev", dnOption.Value);
        }



        [Fact]
        public void Test4()
        {
            const string CMD_LINE = "Deploy Exec /Package \"2021-04.03 Sprint03\\ipack.jsonc\" /DstEnv 01-Dev --mixed-option is-set \"My Targets\"";
            string[] args = Win32.CommandLineToArgs(CMD_LINE);

            var clArgs = CommandLineParser.Parse(args);

            Assert.Equal(2, clArgs.Verbs.Count);
            Assert.Equal(3, clArgs.Options.Count);
            Assert.Single(clArgs.Targets);

            var verbs = clArgs.Verbs.ToArray();
            Assert.Equal("Deploy", verbs[0]);
            Assert.Equal("Exec", verbs[1]);

            Option packageOption = clArgs.Options.FirstOrDefault(o => o.Key == "Package");
            Assert.NotNull(packageOption);
            Assert.Equal(@"2021-04.03 Sprint03\ipack.jsonc", packageOption.Value);

            Option destEnv = clArgs.Options.FirstOrDefault(o => o.Key == "DstEnv");
            Assert.NotNull(destEnv);
            Assert.Equal("01-Dev", destEnv.Value);   
            
            Option mixedOption = clArgs.Options.FirstOrDefault(o => o.Key == "mixed-option");
            Assert.NotNull(mixedOption);
            Assert.Equal("is-set", mixedOption.Value);

            Assert.Equal("My Targets", clArgs.Targets[0]);
        }


        [Fact]
        public void Test5()
        {
            const string CMD_LINE = "Deploy /Package 2021 --flag --mixed-option is-set \"My Targets\"";
            string[] args = Win32.CommandLineToArgs(CMD_LINE);

            var clArgs = CommandLineParser.Parse(args);

            Assert.Single(clArgs.Verbs);
            Assert.Equal(3, clArgs.Options.Count);
            Assert.Single(clArgs.Targets);

            var verbs = clArgs.Verbs.ToArray();
            Assert.Equal("Deploy", verbs[0]);

            Option packageOption = clArgs.Options.FirstOrDefault(o => o.Key == "Package");
            Assert.NotNull(packageOption);
            Assert.Equal(@"2021", packageOption.Value);

            Option flagOption = clArgs.Options.FirstOrDefault(o => o.Key == "flag");
            Assert.NotNull(flagOption);
            Assert.True( bool.Parse(flagOption.Value));   
            
            Option mixedOption = clArgs.Options.FirstOrDefault(o => o.Key == "mixed-option");
            Assert.NotNull(mixedOption);
            Assert.Equal("is-set", mixedOption.Value);

            Assert.Equal("My Targets", clArgs.Targets[0]);
        }


        [Fact]
        public void Test6()
        {
            const string CMD_LINE = "Deploy /ExcludeFolder=\"HM Government of Gibraltar/Baseline Code11\" /Package=\"2021 gx\" @arguments.txt --flag --mixed-option is-set \"My Targets\"";
            string[] args = Win32.CommandLineToArgs(CMD_LINE);

            var clArgs = CommandLineParser.Parse(args);

            Assert.Single(clArgs.Verbs);
            Assert.Equal(10, clArgs.Options.Count);
            Assert.Single(clArgs.Targets);

            var verbs = clArgs.Verbs.ToArray();
            Assert.Equal("Deploy", verbs[0]);

            var excludeFolders = clArgs.Options.Where(option => option.Key == "ExcludeFolder").ToList();
            Assert.NotEmpty(excludeFolders);
            Assert.Equal(7, excludeFolders.Count);
            Assert.Equal(@"HM Government of Gibraltar/03", excludeFolders[2].Value);

            Option packageOption = clArgs.Options.FirstOrDefault(o => o.Key == "Package");
            Assert.NotNull(packageOption);
            Assert.Equal(@"2021 gx", packageOption.Value);

            Option flagOption = clArgs.Options.FirstOrDefault(o => o.Key == "flag");
            Assert.NotNull(flagOption);
            Assert.True( bool.Parse(flagOption.Value));   
            
            Option mixedOption = clArgs.Options.FirstOrDefault(o => o.Key == "mixed-option");
            Assert.NotNull(mixedOption);
            Assert.Equal("is-set", mixedOption.Value);

            Assert.Equal("My Targets", clArgs.Targets[0]);
        }
    }
}