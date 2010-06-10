using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace XnaConsole
{
    //binds keys to characters or strings
    struct KeyBinding
    {
        public Keys Key;
        public string UnmodifiedString;
        public string ShiftString;
        public string AltString;
        public string ShiftAltString;

        public KeyBinding(Keys key, string unmodifiedString, string shiftString, string altString, string shiftAltString)
        {
            this.Key = key;
            this.UnmodifiedString = unmodifiedString;
            this.ShiftString = shiftString;
            this.AltString = altString;
            this.ShiftAltString = shiftAltString;
        }
    }

    //defines standard character mappings
    class KeyboardHelper
    {
        static public KeyBinding[] ItalianBindings = new KeyBinding[]
        {
            new KeyBinding( Keys.OemPipe, "\\", "|", "", ""),
            new KeyBinding( Keys.OemBackslash, "<", ">", "", ""),
            new KeyBinding( Keys.OemOpenBrackets, "\"", "?", "", ""),
            new KeyBinding( Keys.OemCloseBrackets, "ì", "^", "", ""),
            new KeyBinding( Keys.OemSemicolon, "è", "é", "[", "{"),
            new KeyBinding( Keys.OemPlus, "+", "*", "]", "}"),
            new KeyBinding( Keys.OemTilde, "ò", "ç", "@", ""),
            new KeyBinding( Keys.OemQuotes, "à", "°", "#", ""),
            new KeyBinding( Keys.OemQuestion, "ù", "§", "", ""),
            new KeyBinding( Keys.OemComma, ", ", ";", "", ""),
            new KeyBinding( Keys.OemPeriod, ".", ":", "", ""),
            new KeyBinding( Keys.OemMinus, "-", "_", "", ""),
            new KeyBinding( Keys.Space, " ", "", "", ""),            
            new KeyBinding( Keys.Tab, "\t", "", "", ""),
            new KeyBinding( Keys.D1, "1", "!", "", ""),
            new KeyBinding( Keys.D2, "2", "@", "", ""),
            new KeyBinding( Keys.D3, "3", "#", "", ""),
            new KeyBinding( Keys.D4, "4", "$", "", ""),
            new KeyBinding( Keys.D5, "5", "%", "", ""),
            new KeyBinding( Keys.D6, "6", "^", "", ""),
            new KeyBinding( Keys.D7, "7", "&", "", ""),
            new KeyBinding( Keys.D8, "8", "*", "", ""),
            new KeyBinding( Keys.D9, "9", "(", "", ""),
            new KeyBinding( Keys.D0, "0", ")", "", ""),
            new KeyBinding( Keys.NumPad1, "1", "!", "", ""),
            new KeyBinding( Keys.NumPad2, "2", "\"", "", ""),
            new KeyBinding( Keys.NumPad3, "3", "£", "", ""),
            new KeyBinding( Keys.NumPad4, "4", "$", "", ""),
            new KeyBinding( Keys.NumPad5, "5", "%", "€", ""),
            new KeyBinding( Keys.NumPad6, "6", "&", "", ""),
            new KeyBinding( Keys.NumPad7, "7", "/", "", ""),
            new KeyBinding( Keys.NumPad8, "8", "(", "", ""),
            new KeyBinding( Keys.NumPad9, "9", ")", "", ""),
            new KeyBinding( Keys.NumPad0, "0", "=", "", ""),
            new KeyBinding( Keys.A, "a", "A", "", ""),
            new KeyBinding( Keys.B, "b", "B", "", ""),
            new KeyBinding( Keys.C, "c", "C", "", ""),
            new KeyBinding( Keys.D, "d", "D", "", ""),
            new KeyBinding( Keys.E, "e", "E", "€", ""),
            new KeyBinding( Keys.F, "f", "F", "", ""),
            new KeyBinding( Keys.G, "g", "G", "", ""),
            new KeyBinding( Keys.H, "h", "H", "", ""),
            new KeyBinding( Keys.I, "i", "I", "", ""),
            new KeyBinding( Keys.J, "j", "J", "", ""),
            new KeyBinding( Keys.K, "k", "K", "", ""),
            new KeyBinding( Keys.L, "l", "L", "", ""),
            new KeyBinding( Keys.M, "m", "M", "", ""),
            new KeyBinding( Keys.N, "n", "N", "", ""),
            new KeyBinding( Keys.O, "o", "O", "", ""),
            new KeyBinding( Keys.P, "p", "P", "", ""),
            new KeyBinding( Keys.Q, "q", "Q", "", ""),
            new KeyBinding( Keys.R, "r", "R", "", ""),
            new KeyBinding( Keys.S, "s", "S", "", ""),
            new KeyBinding( Keys.T, "t", "T", "", ""),
            new KeyBinding( Keys.U, "u", "U", "", ""),
            new KeyBinding( Keys.V, "v", "V", "", ""),
            new KeyBinding( Keys.W, "w", "W", "", ""),
            new KeyBinding( Keys.X, "x", "X", "", ""),
            new KeyBinding( Keys.Y, "y", "Y", "", ""),
            new KeyBinding( Keys.Z, "z", "Z", "", "")
        };

        static public KeyBinding[] SwedishBindings = new KeyBinding[]
        {
            new KeyBinding( Keys.OemPipe, "\\", "|", "", ""),
            new KeyBinding( Keys.OemBackslash, "\\", "|", "", ""),
            new KeyBinding( Keys.OemOpenBrackets, "[", "{", "", ""),
            new KeyBinding( Keys.OemCloseBrackets, "]", "}", "", ""),
            new KeyBinding( Keys.OemSemicolon, ";", ":", "", ""),
            new KeyBinding( Keys.OemPlus, "=", "+", "", ""),
            new KeyBinding( Keys.OemTilde, "§", "½", "", ""),
            new KeyBinding( Keys.OemQuotes, "\"", "\"", "", ""),
            new KeyBinding( Keys.OemQuestion, "/", "?", "", ""),
            new KeyBinding( Keys.OemComma, ", ", "<", "", ""),
            new KeyBinding( Keys.OemPeriod, ".", ">", "", ""),
            new KeyBinding( Keys.OemMinus, "-", "_", "", ""),
            new KeyBinding( Keys.Space, " ", "", "", ""),
            new KeyBinding( Keys.Tab, "\t", "", "", ""),
            new KeyBinding( Keys.D1, "1", "!", "", ""),
            new KeyBinding( Keys.D2, "2", "@", "", ""),
            new KeyBinding( Keys.D3, "3", "#", "", ""),
            new KeyBinding( Keys.D4, "4", "$", "", ""),
            new KeyBinding( Keys.D5, "5", "%", "", ""),
            new KeyBinding( Keys.D6, "6", "^", "", ""),
            new KeyBinding( Keys.D7, "7", "&", "", ""),
            new KeyBinding( Keys.D8, "8", "*", "", ""),
            new KeyBinding( Keys.D9, "9", "(", "", ""),
            new KeyBinding( Keys.D0, "0", ")", "", ""),
            new KeyBinding( Keys.NumPad1, "1", "!", "", ""),
            new KeyBinding( Keys.NumPad2, "2", "@", "", ""),
            new KeyBinding( Keys.NumPad3, "3", "#", "", ""),
            new KeyBinding( Keys.NumPad4, "4", "$", "", ""),
            new KeyBinding( Keys.NumPad5, "5", "%", "", ""),
            new KeyBinding( Keys.NumPad6, "6", "^", "", ""),
            new KeyBinding( Keys.NumPad7, "7", "&", "", ""),
            new KeyBinding( Keys.NumPad8, "8", "*", "", ""),
            new KeyBinding( Keys.NumPad9, "9", "(", "", ""),
            new KeyBinding( Keys.NumPad0, "0", ")", "", ""),
            new KeyBinding( Keys.A, "a", "A", "", ""),
            new KeyBinding( Keys.B, "b", "B", "", ""),
            new KeyBinding( Keys.C, "c", "C", "", ""),
            new KeyBinding( Keys.D, "d", "D", "", ""),
            new KeyBinding( Keys.E, "e", "E", "", ""),
            new KeyBinding( Keys.F, "f", "F", "", ""),
            new KeyBinding( Keys.G, "g", "G", "", ""),
            new KeyBinding( Keys.H, "h", "H", "", ""),
            new KeyBinding( Keys.I, "i", "I", "", ""),
            new KeyBinding( Keys.J, "j", "J", "", ""),
            new KeyBinding( Keys.K, "k", "K", "", ""),
            new KeyBinding( Keys.L, "l", "L", "", ""),
            new KeyBinding( Keys.M, "m", "M", "", ""),
            new KeyBinding( Keys.N, "n", "N", "", ""),
            new KeyBinding( Keys.O, "o", "O", "", ""),
            new KeyBinding( Keys.P, "p", "P", "", ""),
            new KeyBinding( Keys.Q, "q", "Q", "", ""),
            new KeyBinding( Keys.R, "r", "R", "", ""),
            new KeyBinding( Keys.S, "s", "S", "", ""),
            new KeyBinding( Keys.T, "t", "T", "", ""),
            new KeyBinding( Keys.U, "u", "U", "", ""),
            new KeyBinding( Keys.V, "v", "V", "", ""),
            new KeyBinding( Keys.W, "w", "W", "", ""),
            new KeyBinding( Keys.X, "x", "X", "", ""),
            new KeyBinding( Keys.Y, "y", "Y", "", ""),
            new KeyBinding( Keys.Z, "z", "Z", "", "")
        };

        static public KeyBinding[] AmericanBindings = new KeyBinding[]
        {
            new KeyBinding( Keys.OemPipe, "\\", "|", "", ""),
            new KeyBinding( Keys.OemBackslash, "\\", "|", "", ""),
            new KeyBinding( Keys.OemOpenBrackets, "[", "{", "", ""),
            new KeyBinding( Keys.OemCloseBrackets, "]", "}", "", ""),
            new KeyBinding( Keys.OemSemicolon, ";", ":", "", ""),
            new KeyBinding( Keys.OemPlus, "=", "+", "", ""),
            new KeyBinding( Keys.OemTilde, "`", "~", "", ""),
            new KeyBinding( Keys.OemQuotes, "\"", "\"", "", ""),
            new KeyBinding( Keys.OemQuestion, "/", "?", "", ""),
            new KeyBinding( Keys.OemComma, ", ", "<", "", ""),
            new KeyBinding( Keys.OemPeriod, ".", ">", "", ""),
            new KeyBinding( Keys.OemMinus, "-", "_", "", ""),
            new KeyBinding( Keys.Space, " ", "", "", ""),
            new KeyBinding( Keys.Tab, "\t", "", "", ""),
            new KeyBinding( Keys.D1, "1", "!", "", ""),
            new KeyBinding( Keys.D2, "2", "@", "", ""),
            new KeyBinding( Keys.D3, "3", "#", "", ""),
            new KeyBinding( Keys.D4, "4", "$", "", ""),
            new KeyBinding( Keys.D5, "5", "%", "", ""),
            new KeyBinding( Keys.D6, "6", "^", "", ""),
            new KeyBinding( Keys.D7, "7", "&", "", ""),
            new KeyBinding( Keys.D8, "8", "*", "", ""),
            new KeyBinding( Keys.D9, "9", "(", "", ""),
            new KeyBinding( Keys.D0, "0", ")", "", ""),
            new KeyBinding( Keys.NumPad1, "1", "!", "", ""),
            new KeyBinding( Keys.NumPad2, "2", "@", "", ""),
            new KeyBinding( Keys.NumPad3, "3", "#", "", ""),
            new KeyBinding( Keys.NumPad4, "4", "$", "", ""),
            new KeyBinding( Keys.NumPad5, "5", "%", "", ""),
            new KeyBinding( Keys.NumPad6, "6", "^", "", ""),
            new KeyBinding( Keys.NumPad7, "7", "&", "", ""),
            new KeyBinding( Keys.NumPad8, "8", "*", "", ""),
            new KeyBinding( Keys.NumPad9, "9", "(", "", ""),
            new KeyBinding( Keys.NumPad0, "0", ")", "", ""),
            new KeyBinding( Keys.A, "a", "A", "", ""),
            new KeyBinding( Keys.B, "b", "B", "", ""),
            new KeyBinding( Keys.C, "c", "C", "", ""),
            new KeyBinding( Keys.D, "d", "D", "", ""),
            new KeyBinding( Keys.E, "e", "E", "", ""),
            new KeyBinding( Keys.F, "f", "F", "", ""),
            new KeyBinding( Keys.G, "g", "G", "", ""),
            new KeyBinding( Keys.H, "h", "H", "", ""),
            new KeyBinding( Keys.I, "i", "I", "", ""),
            new KeyBinding( Keys.J, "j", "J", "", ""),
            new KeyBinding( Keys.K, "k", "K", "", ""),
            new KeyBinding( Keys.L, "l", "L", "", ""),
            new KeyBinding( Keys.M, "m", "M", "", ""),
            new KeyBinding( Keys.N, "n", "N", "", ""),
            new KeyBinding( Keys.O, "o", "O", "", ""),
            new KeyBinding( Keys.P, "p", "P", "", ""),
            new KeyBinding( Keys.Q, "q", "Q", "", ""),
            new KeyBinding( Keys.R, "r", "R", "", ""),
            new KeyBinding( Keys.S, "s", "S", "", ""),
            new KeyBinding( Keys.T, "t", "T", "", ""),
            new KeyBinding( Keys.U, "u", "U", "", ""),
            new KeyBinding( Keys.V, "v", "V", "", ""),
            new KeyBinding( Keys.W, "w", "W", "", ""),
            new KeyBinding( Keys.X, "x", "X", "", ""),
            new KeyBinding( Keys.Y, "y", "Y", "", ""),
            new KeyBinding( Keys.Z, "z", "Z", "", "")
        };
    }
}
