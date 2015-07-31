using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ScintillaNET;
using System.IO;

namespace Test1 {

    /// <summary>
    /// This test illustrates the bug when loading text with ILoader
    /// </summary>
    public partial class Form1 : Form {

        /// <summary>
        /// Left editor, the one which will have text assigned to Text property
        /// </summary>
        Scintilla Editor1;
        
        /// <summary>
        /// Right editor, the one which will have text loaded with ILoader
        /// </summary>
        Scintilla Editor2;

        public Form1() {
            InitializeComponent();
        }

        /// <summary>
        /// Configures 2 ScintillaNET editors to use simple word highlighting
        /// Left editor loads text through assinging to Text property
        /// Right editor loads text with ILoader
        /// Both have assigned the simplest possible highligther to highlight one word on each UI update
        /// Then, we try to add and remove 5 characters to 3rd line of each editor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e) {
            splitContainer1.Panel1.Controls.Add(Editor1 = GetConfiguredEditor("Editor1"));
            splitContainer1.Panel2.Controls.Add(Editor2 = GetConfiguredEditor("Editor2"));
            LoadText(Editor1, 0);
            LoadText(Editor2, 1);
            Editor1.UpdateUI += Editor_UpdateUI;
            Editor2.UpdateUI += Editor_UpdateUI;
            EditTest(Editor1);
            EditTest(Editor2);
        }

        /// <summary>
        /// Configures ScintillaNET Scintilla control for simple text highlighting
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Scintilla GetConfiguredEditor(string name = null) {
            var scintilla = new Scintilla();
            scintilla.Lexer = Lexer.Null;
            scintilla.Styles[1].ForeColor = Color.Red;
            scintilla.Name = name;
            scintilla.Dock = DockStyle.Fill;
            return scintilla;
        }

        /// <summary>
        /// Loads a sample text using one of following methods:
        /// 0: assign a string to Text property
        /// 1: use Scintilla.CreateLoader to load character array
        /// </summary>
        /// <param name="editor"></param>
        /// <param name="mode"></param>
        void LoadText(Scintilla editor, int mode) {
            const string path = @"..\..\TestString.txt";
            var encoding = Encoding.ASCII;
            switch (mode) {
                case 0:
                    editor.Document = Document.Empty;
                    editor.EndUndoAction();
                    var text = File.ReadAllText(path, encoding);
                    editor.Text = text;
                    editor.EmptyUndoBuffer();
                    break;
                case 1:
                    var loader = editor.CreateLoader(1024 * 1024);
                    var bytes = File.ReadAllBytes(path);
                    var chars = encoding.GetChars(bytes);
                    loader.AddData(chars, chars.Length);
                    var document = loader.ConvertToDocument();
                    editor.Document = document;
                    editor.ReleaseDocument(document);
                    //editor.UndoCollection = true; // see issue #94
                    break;
            }
        }

        /// <summary>
        /// On each UI update a word should be highlighted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Editor_UpdateUI(object sender, UpdateUIEventArgs e) {
            var scintilla = sender as Scintilla;
            HighlightWord(scintilla);
        }

        /// <summary>
        /// Highlights each occurance of a word (default "FOX")
        /// </summary>
        /// <param name="editor"></param>
        /// <param name="word"></param>
        void HighlightWord(Scintilla editor, string word = "FOX") {
            var text = editor.Text;
            var indexes = new List<int>();
            for (int i = 0; ; i += word.Length) {
                i = text.IndexOf(word, i);
                if (i < 0) break;
                if (i >= 0) indexes.Add(i);
            }
            indexes.ForEach(i => {
                editor.StartStyling(i);
                editor.SetStyling(word.Length, 1);
            });
        }

        /// <summary>
        /// Adds 5 characters and removes 5 characters
        /// </summary>
        /// <param name="editor"></param>
        void EditTest(Scintilla editor) {
            var p = editor.Text.IndexOf("\nT");
            editor.InsertText(p + 1, "TEST ");
            editor.DeleteRange(p + 1, 5);
        }

    }

}