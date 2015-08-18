﻿using System;
using System.Collections.Generic;
using System.Text;

namespace VitaDefiler.Modules
{
    class General : IModule
    {
        private static readonly string HelpText =
@"
Commands:
alloc type length               Allocates space for a variable. Type is 
                                'data' or 'code'.
compile file.c file             Compiles file.c to produce file.
echo [text]                     Prints text to Vita/console screen
exec addr [arg0] ... [arg3]     Executes code located at address with args.
                                Return value will be stored into a variable.
free addr                       Frees memory allocated at addr.
usbread addr length [file]      Uses USB to dump address. Optional file to 
                                capture output to. Optional length to read.
read addr [length] [file]       Uses network to dump address. Optional file 
                                to capture output to. If no length is 
                                specified, it will be size of variable.
write addr [length] file        Writes binary data from a file to addr.
                                If no length is specified, length will be 
                                file size. Code memory write inferred if 
                                'addr' is a pointer variable from alloc code.
writecode addr [length] file    Same as 'write' but force variable to point to 
                                code.
pull srcfile [dstfile]          Pulls a file from the device. If dstfile 
                                is not specified, name will be same as src.
push srcfile [dstfile]          Pushes a file to the device. If dstfile 
                                is not specified, name will be same as src.
set addr [value|name]           Writes a 32-bit little-endian integer at addr.
                                Value can be a variable.
get addr name                   Reads a 32-bit little-endian pointer from addr.
                                Stores data to '%name'
local addr name                 Creates a local variable accessible with '%name'
                                and value of pointer addr casted to an integer.
vars                            Print list of variables
script file                     Runs all commands from file.

Paramaters:
addr    Can be either an integer address (ex: 0x81000000) or a variable of form
        $x (for code/data pointers) or %x (for data variables). Can also optionally 
        include an offset in the form of $x+num or $x-num (ex: $2+0x100, $0-256, 
        0x81000000+0x100). %# is a special variable indicating the last return.
name    Refers to the name of a data variable (e.g. %name)
length  Can be a hex number (ex: 0x1000), a decimal number (ex: 256), or a data type 
        including int, uint, char, short, float, etc. int/uint can also be 
        qualified with size, for example: int32 or uint16.
file    Filename relative to current working directory or absolute path.
";

        public bool Run(Device dev, string cmd, string[] args)
        {
            switch (cmd)
            {
                case "help":
                    Help();
                    return true;
                case "echo":
                case "print":
                    Echo(dev, string.Join(" ", args));
                    return true;
                case "vars":
                    PrintVars(dev);
                    return true;
                case "local":
                    if (args.Length >= 2)
                    {
                        dev.CreateLocal(args[1], args[0].ToVariable(dev).Data);
                        return true;
                    }
                    break;
            }
            return false;
        }

        public void Help()
        {
            Console.Error.Write(HelpText);
        }

        public void Echo(Device dev, string text)
        {
            byte[] resp;
            dev.Network.RunCommand(Command.Echo, Encoding.ASCII.GetBytes(text), out resp);
            Console.WriteLine(Encoding.ASCII.GetString(resp));
        }

        public void PrintVars(Device dev)
        {
            var dataVars = dev.Vars;
            for (int i = 0; i < dataVars.Count; i++)
            {
                if (dataVars[i].Data == 0)
                {
                    continue;
                }
                Console.WriteLine("${0}: 0x{1:X}, size: 0x{2:X}, code: {3}", i, dataVars[i].Data, dataVars[i].Size, dataVars[i].IsCode);
            }
            foreach (KeyValuePair<string, uint> entry in dev.Locals)
            {
                Console.WriteLine("%{0}: 0x{1:X}", entry.Key, entry.Value);
            }
        }
    }
}
