///*************************************************************************
// *
// *   file		: MainForm.cs
// *   copyright		: (C) The WCell Team
// *   email		: info@wcell.org
// *   last changed	: $LastChangedDate: 2008-05-17 11:48:06 +0200 (lø, 17 maj 2008) $
// *   last author	: $LastChangedBy: domiii $
// *   revision		: $Rev: 357 $
// *
// *   This program is free software; you can redistribute it and/or modify
// *   it under the terms of the GNU General Public License as published by
// *   the Free Software Foundation; either version 2 of the License, or
// *   (at your option) any later version.
// *
// *************************************************************************/

//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Globalization;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Windows.Forms;

//namespace WCell.PacketAnalyzer
//{
//    public partial class MainForm : Form
//    {
//        public MainForm()
//        {
//            InitializeComponent();
//        }

//        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
//        {
//            Application.Exit();
//        }

//        private void btnAnalyze_Click(object sender, EventArgs e)
//        {
//            Analyze(txtRawInput.Text);
//        }

//        private int m_opcodeLine = 0;
//        private int m_packetLength = 0;
//        private int m_opcode = 0;
//        private byte[] m_data;

//        private ObjectTypeIds m_objectType = ObjectTypeIds.Object;

//        private void Analyze(string rawData)
//        {
//            List<string> dataArr = new List<string>(rawData.Split('\n'));
//            int removed = dataArr.RemoveAll(IsBadLine);

//            FindOpcodeLine(dataArr);
//            FillLabels(dataArr[m_opcodeLine]);

//            dataArr.Remove(dataArr[m_opcodeLine]);

//            StripBadChars(dataArr);

//            for (int i = 0; i < dataArr.Count; i++)
//            {
//                int first = dataArr[i].IndexOf('|') + 1;
//                int second = dataArr[i].IndexOf('|', first) - 1;
//                dataArr[i] = dataArr[i].Remove(second);
//                dataArr[i] = dataArr[i].Substring(first, second - first);
//                dataArr[i].TrimEnd(' ');
//            }

//            StringBuilder sb = new StringBuilder();
//            foreach (string s in dataArr)
//            {
//                sb.AppendLine(s);
//            }
//            txtAnalyzed.Text = sb.ToString();

//            m_data = GetBytes(sb.ToString());
//            if (m_data == null)
//            {
//                Debugger.Break();
//            }
//        }

//        public byte[] GetBytes(string data)
//        {
//            data = data.Replace(" ", "");
//            data = data.Replace("\r", "");
//            data = data.Replace("\n", "");
//            byte[] dump = new byte[data.Length/2]; // each byte has 2 width in this line
//            int pos = 0;
//            string bytedata;
//            for (int i = 0; i < dump.Length; i++)
//            {
//                bytedata = data.Substring(pos, 2);
//                pos += 2;
//                dump[i] = Byte.Parse(bytedata, NumberStyles.HexNumber);
//            }
//            return dump;
//        }

//        private void FindOpcodeLine(List<string> data)
//        {
//            for (int i = 0; i < data.Count; i++)
//            {
//                if (data[i].StartsWith("{"))
//                {
//                    m_opcodeLine = i;
//                    return;
//                }
//            }
//        }

//        private void FillLabels(string line)
//        {
//            //Packet: (0x0096) SMSG_MESSAGECHAT PacketSize = 111
//            Match m = Regex.Match(line, @"(0x.{4})");
//            if (m.Success)
//            {
//                string text = m.Value + " / " +
//                              (RealmServerOpCode) Int32.Parse(m.Value.Substring(2), NumberStyles.HexNumber);
//                lblOpcodeValue.Text = text;
//            }

//            m = Regex.Match(line, "= [0-9]+");
//            if (m.Success)
//            {
//                lblLengthValue.Text = m.Value.Substring(2);
//            }
//        }

//        private void tabPage2_Click(object sender, EventArgs e)
//        {
//        }

//        private void compressedA9ToolStripMenuItem_Click(object sender, EventArgs e)
//        {
//            UpdateParser.Parse(txtOutput, m_data, true);
//        }

//        private void a9ToolStripMenuItem_Click(object sender, EventArgs e)
//        {
//            UpdateParser.Parse(txtOutput, m_data, false);
//        }


//        private bool IsBadLine(string s)
//        {
//            if (s.Length == 0)
//                return true;
//            if (s.Length == 1 && s[0] == '\n')
//                return true;
//            if (s.Length == 1 && s[0] == '\r')
//                return true;
//            if (s.Contains("|00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F |0123456789ABCDEF|"))
//                return true;
//            if (s.StartsWith("|-"))
//                return true;
//            if (s.StartsWith("--"))
//                return true;

//            return String.IsNullOrEmpty(s);
//        }

//        private void StripBadChars(List<string> list)
//        {
//            foreach (string s in list)
//            {
//                s.Replace("\n", "");
//                s.Replace("\r", "");
//            }
//        }
//    }
//}