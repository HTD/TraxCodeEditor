﻿using ScintillaNET;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Trax {

    public partial class MainForm : Form {

        string SamplesDir = @"..\..\Samples";
        int Mode = 0; // Text
        Encoding ScnEncoding = Encoding.GetEncoding(1250);

        /// <summary>
        /// Initializes the editor and loads a color scheme
        /// </summary>
        public MainForm() {
            InitializeComponent();
            Ed.ContainerLexer = new ScnLexer(Ed);
            Ed.Preset = Presets.Zenburn;
            Ed.Lexer = Lexer.Null;
            Ed.FoldingStyle = FoldingStyles.CurvyTrees;
            Ed.FontQuality = FontQuality.LcdOptimized;
            Ed.Technology = Technology.DirectWrite;
            ListSamples();
        }

        /// <summary>
        /// Creates test menu
        /// </summary>
        private void ListSamples() {
            var samples = new ToolStripMenuItem("&Samples");
            var mode = new ToolStripMenuItem("&Mode");
            MainMenuStrip = new MenuStrip();
            MainMenuStrip.Items.Add(samples);
            MainMenuStrip.Items.Add(mode);
            foreach (string filename in Directory.EnumerateFiles(SamplesDir))
                samples.DropDownItems.Add(
                    new ToolStripMenuItem(
                        filename.Replace(SamplesDir + "\\", ""),
                        null,
                        SelectSample
                    )
                );
            mode.DropDownItems.Add(new ToolStripMenuItem("Text", null, SwitchMode) { Tag = 0, Checked = true });
            mode.DropDownItems.Add(new ToolStripMenuItem("Loader", null, SwitchMode) { Tag = 1 });
            Controls.Add(MainMenuStrip);
        }

        /// <summary>
        /// Handles mode switching
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SwitchMode(object sender, EventArgs e) {
            var item = sender as ToolStripMenuItem;
            Mode = (int)item.Tag;
            var toolStrip = item.GetCurrentParent();
            foreach (ToolStripMenuItem i in toolStrip.Items) i.Checked = !i.Checked;
        }

        /// <summary>
        /// Selects a sample based on file extension
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectSample(object sender, EventArgs e) {
            var item = sender as ToolStripMenuItem;
            var filename = item.Text;
            if (filename.EndsWith(".js")) JavaScriptTest(filename);
            else ScnTest(filename);
        }

        /// <summary>
        /// Loads a file as custom format
        /// </summary>
        /// <param name="filename"></param>
        private async void ScnTest(string filename) {
            Ed.Lexer = Lexer.Null;
            if (Mode == 0) Ed.LoadFile(String.Format("{0}\\{1}", SamplesDir, filename), ScnEncoding);
            else await Ed.LoadFileAsync(String.Format("{0}\\{1}", SamplesDir, filename), ScnEncoding);
            Ed.ContainerLexerMode = ContainerLexerModes.Visible;
            Ed.Lexer = Lexer.Container;
        }

        /// <summary>
        /// Loads a file as JavaScript
        /// </summary>
        /// <param name="filename"></param>
        private async void JavaScriptTest(string filename) {
            if (Mode == 0) Ed.LoadFile(String.Format("{0}\\{1}", SamplesDir, filename));
            else await Ed.LoadFileAsync(String.Format("{0}\\{1}", SamplesDir, filename));
            Ed.Lexer = Lexer.Cpp;
            Ed.Keywords = KeywordSets.ECMAScript;
            Ed.ColorScheme.ResetSyntax();
            Ed.StyleScheme.Reset();
            Ed.IndentationGuides = IndentView.LookBoth;
            Ed.ShowFoldMargin = true;
        }

        /// <summary>
        /// Quick test of new DwellOnIdentifier event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Ed_DwellOnIdentifier(object sender, DwellOnIdentifierEventArgs e) {
            e.CallTipText = String.Format(
                "Identifier: {0}\r\nIndex: {1}\r\nLength: {2}",
                e.Identifier, e.IdentifierRange.First, e.IdentifierRange.Length
            );
            var s = sender as CodeEditor;
       }
    }

}
