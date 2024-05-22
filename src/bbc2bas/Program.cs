namespace Vurdalakov.AgonTools
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Text;

    internal class Program
    {
        public static Int32 Main(String[] args)
        {
            if (args.Length != 1)
            {
                var assembly = Assembly.GetCallingAssembly();
                Console.WriteLine($"{(assembly.GetCustomAttribute(typeof(AssemblyTitleAttribute)) as AssemblyTitleAttribute).Title} v{assembly.GetName().Version} | {(assembly.GetCustomAttribute(typeof(AssemblyCopyrightAttribute)) as AssemblyCopyrightAttribute).Copyright} | https://github.com/vurdalakov/agontools");
                Console.WriteLine((assembly.GetCustomAttribute(typeof(AssemblyDescriptionAttribute)) as AssemblyDescriptionAttribute).Description);
                Console.WriteLine("Usage: bbc2bas <file-name.bbc>");
                return 1;
            }

            var fileName = args[0];

            if (!File.Exists(fileName))
            {
                Error(2, $"File not found: {fileName}");
            }

            var stringBuilder = new StringBuilder();

            try
            {
                using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (var binaryReader = new BinaryReader(fileStream))
                    {
                        while (true)
                        {
                            var length = binaryReader.ReadByte();
                            if (length == 0)
                            {
                                break;
                            }

                            var line = binaryReader.ReadUInt16();
                            stringBuilder.Append($"{line} ");

                            for (var i = 0; i < length - 4; i++)
                            {
                                var b = binaryReader.ReadByte();

                                if (b < 0x80)
                                {
                                    stringBuilder.Append((Char)b);
                                }
                                else
                                {
                                    stringBuilder.Append(Keywords[b - 0x80]);
                                }
                            }

                            var separator = binaryReader.ReadByte();
                            if (separator != 0x0D)
                            {
                                Error(3, $"Invalid separator at {fileStream.Position:N0}");
                            }

                            stringBuilder.AppendLine();
                        }

                        var footer = binaryReader.ReadUInt16();
                        if (footer != 0xFFFF)
                        {
                            Error(4, $"Invalid footer at {fileStream.Position:N0}");
                        }

                        if (fileStream.Position != fileStream.Length)
                        {
                            Error(5, $"Extra bytes after {fileStream.Position:N0}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Error(6, $"Error converting file: {ex.Message}");
            }

            Console.WriteLine(stringBuilder);
            return 0;
        }

        private static void Error(Int32 errorCode, String errorMessage)
        {
            Console.WriteLine($"Error: {errorMessage}");
            Environment.Exit(errorCode);
        }

        private static readonly String[] Keywords =
        {
            /* 0x80 */ "AND", "DIV", "EOR", "MOD", "OR", "ERROR", "LINE", "OFF", "STEP", "SPC", "TAB(", "ELSE", "THEN", "", "OPENIN", "PTR",
            /* 0x90 */ "PAGE", "TIME", "LOMEM", "HIMEM", "ABS", "ACS", "ADVAL", "ASC", "ASN", "ATN", "BGET", "COS", "COUNT", "DEG", "ERL", "ERR",
            /* 0xA0 */ "EVAL", "EXP", "EXT", "FALSE", "FN", "GET", "INKEY", "INSTR(", "INT", "LEN", "LN", "LOG", "NOT", "OPENUP", "OPENOUT", "PI",
            /* 0xB0 */ "POINT(", "POS", "RAD", "RND", "SGN", "SIN", "SQR", "TAN", "TO", "TRUE", "USR", "VAL", "VPOS", "CHR$", "GET$", "INKEY$",
            /* 0xC0 */ "LEFT$(", "MID$(", "RIGHT$(", "STR$", "STRING$(", "EOF", "AUTO", "DELETE", "LOAD", "LIST", "NEW", "OLD", "RENUMBER", "SAVE", "PUT", "PTR",
            /* 0xD0 */ "PAGE", "TIME", "LOMEM", "HIMEM", "SOUND", "BPUT", "CALL", "CHAIN", "CLEAR", "CLOSE", "CLG", "CLS", "DATA", "DEF", "DIM", "DRAW",
            /* 0xE0 */ "END", "ENDPROC", "ENVELOPE", "FOR", "GOSUB", "GOTO", "GCOL", "IF", "INPUT", "LET", "LOCAL", "MODE", "MOVE", "NEXT", "ON", "VDU",
            /* 0xF0 */ "PLOT", "PRINT", "PROC", "READ", "REM", "REPEAT", "REPORT", "RESTORE", "RETURN", "RUN", "STOP", "COLOUR", "TRACE", "UNTIL", "WIDTH", "OSCLI",
        };
    }
}