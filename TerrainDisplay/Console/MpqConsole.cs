using System;
using System.Collections.Generic;
using System.Text;
using XnaConsole;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
/// <summary>
/// Delegate Declaration
/// </summary>
public delegate void CommandDelegate();   // delegate declaration

namespace TerrainDisplay
{
    /// <summary>
    /// Console used to execute commands in game.
    /// </summary>
    public class MpqConsole : DrawableGameComponent
    {
        /// <summary>
        /// Structor for a console command.
        /// </summary>
        public struct ConsoleCommandStruct
        {
            /// <summary>
            /// List of the different commands that can be run
            /// 
            /// </summary>
            public enum CommandCode 
            { 
                /// <summary>
                /// Blank Command
                /// </summary>
                Nothing, 
                /// <summary>
                /// Load Command
                /// </summary>
                Load 
            };
            /// <summary>
            /// Code (command) that is run
            /// </summary>
            public CommandCode commandCode;
            /// <summary>
            /// Data passed to the command
            /// </summary>
            public string commandData;
            ConsoleCommandStruct(CommandCode command, string commandData)
            {
                this.commandCode = command;
                this.commandData = commandData;
            }
        }
       /// <summary>
       /// Not sure on this one
       /// </summary>
        public event CommandDelegate MyEvent;

        const string Prompt = ">>> ";
        const string PromptCont = "... ";

        private XnaConsoleComponent Console;
        private ConsoleCommandStruct command;
        private bool newCommand;

        #region Output stuff
        #endregion

        /// <summary>
        /// Creates a new MpqConsole
        /// </summary>
        public MpqConsole(Game game, SpriteFont font)
            : base(game)
        {

            Console = new XnaConsoleComponent(game, font);
            game.Components.Add(Console);
            Console.Prompt(Prompt, Execute);
        }
        /// <summary>
        /// Console Command
        /// </summary>
        public ConsoleCommandStruct Command
        {
            get
            {
                newCommand = false;
                return command; 
            }           
        }
        /// <summary>
        /// Not sure on this one
        /// </summary>
        public bool NewCommand
        {
            get
            {
                return newCommand;
            } 
        }
        /// <summary>
        /// Writes text to the console
        /// </summary>
        /// <param name="s">String to write</param>
        public void Write(string s)
        {
            Console.Write(s);
        }
        /// <summary>
        /// Writes a line to the console
        /// </summary>
        /// <param name="s">String to write</param>
        public void WriteLine(string s)
        {
            Console.WriteLine(s);
        }
        /// <summary>
        /// Check if the console is open.
        /// </summary>
        /// <returns>Boolean value representing if the console is open.</returns>
        public bool IsOpen()
        {
            return Console.IsOpen();
        }

        /// <summary>
        /// Executes python commands from the console.
        /// </summary>
        /// <param name="input"></param>
        /// <returns>Returns the execution results or error messages.</returns>
        public void Execute(string input)
        {
            try
            {
                //replace with loop that gets the names of the enums later on               
                const string commandCode = "Load ";
                if (input.StartsWith(commandCode, StringComparison.CurrentCultureIgnoreCase))
                {
                    command.commandCode = ConsoleCommandStruct.CommandCode.Load;
                    command.commandData = input.Substring(commandCode.Length);
                    newCommand = true;
                    if (MyEvent != null)
                        MyEvent();

                }
                Console.Prompt(Prompt, Execute);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
                Console.Prompt(Prompt, Execute);
            }

        }
        /// <summary>
        /// Opens the console.
        /// </summary>
        public void Open()
        {
            Console.Open();
        }
    }
}
