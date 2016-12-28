using ScintillaNET;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Trax.Editor;
using Trax.Editor.Lexers;

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
            Ed.SetColorScheme(ColorSchemes.TraxFull.Default);
            Ed.Lexer = Lexer.Null;
            Ed.FoldingStyle = FoldingStyles.CurvyTrees;
            Ed.FontQuality = FontQuality.LcdOptimized;
            Ed.Technology = Technology.DirectWrite;
            Ed.DwellOnIdentifier += Ed_DwellOnIdentifier;
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
            MainMenuStrip.Items.Add(Ed.PresetsMenu);

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

            //var schemes = new ToolStripMenuItem("S&chemes");
            //foreach (var preset in Enum.GetNames(typeof(Presets))) {
            //    schemes.DropDownItems.Add(
            //        new ToolStripMenuItem(
            //            preset,
            //            null,
            //            SelectScheme
            //        )
            //    );
            //}
            

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
            var ext = Path.GetExtension(filename);
            if (ext == ".js" || ext == ".css" || ext == ".html" || ext == ".htm" || ext == ".cs1") WebDesignTest(filename);
            else ScnTest(filename);
        }

        private void SelectScheme(object sender, EventArgs e) {
            var item = sender as ToolStripMenuItem;
            var scheme = (Presets)Enum.Parse(typeof(Presets), item.Text);
            Ed.Preset = scheme;
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
        private async void WebDesignTest(string filename) {
            var ext = Path.GetExtension(filename);
            if (Mode == 0) Ed.LoadFile(String.Format("{0}\\{1}", SamplesDir, filename));
            else await Ed.LoadFileAsync(String.Format("{0}\\{1}", SamplesDir, filename));
            switch (ext) {
                case ".js":
                    Ed.Lexer = Lexer.Cpp;
                    Ed.Keywords = KeywordSets.ECMAScript;
                    break;
                case ".css":
                    Ed.Lexer = Lexer.Css;
                    //Ed.Keywords = KeywordSets.ECMAScript;
                    break;
                case ".html":
                case ".htm":
                    Ed.Lexer = Lexer.Html;
                    //Ed.Keywords = KeywordSets.ECMAScript;
                    break;
                case ".cs1":
                    Ed.Lexer = Lexer.Cpp;
                    break;
            }
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
