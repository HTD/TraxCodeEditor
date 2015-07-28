using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;

namespace Trax {

    /// <summary>
    /// Class used to import objects with FontStyle properties as Scintilla style schemes
    /// </summary>
    public sealed class StyleScheme {

        /// <summary>
        /// Class used to import objects with FontStyle properties as Scintilla style schemes
        /// </summary>
        public enum CommonStyles {
            /// <summary>
            /// Default font style
            /// </summary>
            Text,
            /// <summary>
            /// Number font style
            /// </summary>
            Number,
            /// <summary>
            /// Keyword font style
            /// </summary>
            Keyword,
            /// <summary>
            /// Keyword 1 font style
            /// </summary>
            Keyword2,
            /// <summary>
            /// Comment font style
            /// </summary>
            Comment
        }

        /// <summary>
        /// Settings instance or any object with FontStyle properties
        /// </summary>
        public object Source { get; set; }

        /// <summary>
        /// Properties of Source object
        /// </summary>
        private PropertyInfo[] Properties;
        /// <summary>
        /// Syntax properties of Source object
        /// </summary>
        private PropertyInfo[] SyntaxProperties;

        /// <summary>
        /// Scintilla code editor
        /// </summary>
        private CodeEditor Editor;

        /// <summary>
        /// Creates style scheme for editor from Settings instance or any object with FontStyle properties
        /// </summary>
        /// <param name="editor"></param>
        /// <param name="source"></param>
        public StyleScheme(CodeEditor editor, object source) {
            Editor = editor;
            Source = source;
            Properties = Source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var commonStyleNames = Enum.GetNames(typeof(CommonStyles));
            var styleProperties = Properties.Where(p => p.PropertyType == typeof(FontStyle));
            Properties = styleProperties != null ? styleProperties.ToArray() : null;
            if (Properties != null) {
                var syntaxProperties = Properties.Where(p => !commonStyleNames.Contains(p.Name));
                if (syntaxProperties != null) SyntaxProperties = syntaxProperties.ToArray();
            }
        }

        /// <summary>
        /// (Re)applies full style scheme to editor control
        /// </summary>
        public void Reset() {
            SetCommonStyles();
            ResetSyntax();
        }

        /// <summary>
        /// (Re)applies lexer related style scheme to editor control
        /// </summary>
        public void ResetSyntax() {
            if (Editor.Lexer != Lexer.Null && Editor.Lexer != Lexer.Container) {
                var lexerType = typeof(Style).GetNestedTypes().FirstOrDefault(t => t.Name == Editor.Lexer.ToString());
                if (lexerType != null) SetSyntaxStyles(lexerType);
            }
            else {
                if (Editor.ContainerLexer != null && Editor.ContainerLexer.SyntaxType != null)
                    SetSyntaxStyles(Editor.ContainerLexer.SyntaxType);
            }
        }

        /// <summary>
        /// Sets common styles from Settings instance (if set)
        /// </summary>
        private void SetCommonStyles() {
            PropertyInfo propertyInfo;
            object propertyValue = null;
            FontStyle style = FontStyle.Regular;
            foreach (CommonStyles commonStyle in Enum.GetValues(typeof(CommonStyles))) {
                propertyInfo = Properties.FirstOrDefault(p => p.Name == commonStyle.ToString());
                propertyValue = (propertyInfo != null) ? propertyInfo.GetValue(Source) : null;
                if (propertyValue != null && propertyValue is FontStyle) {
                    style = (FontStyle)propertyValue;
                    switch (commonStyle) {
                        case CommonStyles.Text:
                            Editor.Font = new Font(Editor.Font, style);
                            break;
                        case CommonStyles.Number:
                            SetEditorStyle(Style.Cpp.Number, style);
                            SetEditorStyle(Style.Python.Number, style);
                            SetEditorStyle(Style.Sql.Number, style);
                            break;
                        case CommonStyles.Keyword:
                            SetEditorStyle(Style.Cpp.Word, style);
                            SetEditorStyle(Style.Python.Word, style);
                            SetEditorStyle(Style.Sql.Word, style);
                            break;
                        case CommonStyles.Keyword2:
                            SetEditorStyle(Style.Cpp.Word2, style);
                            SetEditorStyle(Style.Python.Word2, style);
                            SetEditorStyle(Style.Sql.Word2, style);
                            break;
                        case CommonStyles.Comment:
                            SetEditorStyle(Style.Cpp.CommentLine, style);
                            SetEditorStyle(Style.Python.CommentLine, style);
                            SetEditorStyle(Style.Sql.CommentLine, style);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Sets editor styles with names matching specified syntax type
        /// </summary>
        /// <param name="syntax"></param>
        private void SetSyntaxStyles(Type syntax) {
            if (SyntaxProperties == null) return;
            var constants = syntax.GetFields();
            var syntaxStyles = new Dictionary<string, int>();
            foreach (var fieldInfo in constants) syntaxStyles.Add(fieldInfo.Name, (int)fieldInfo.GetRawConstantValue());
            foreach (var property in SyntaxProperties) {
                var name = property.Name;
                var value = property.GetValue(Source);
                if (syntaxStyles.ContainsKey(name) && value != null && value is FontStyle)
                    SetEditorStyle(syntaxStyles[name], (FontStyle)value);
            }
        }

        /// <summary>
        /// Sets editor style font style properties
        /// </summary>
        /// <param name="index"></param>
        /// <param name="style"></param>
        private void SetEditorStyle(int index, FontStyle style) {
            Editor.Styles[index].Bold = style.HasFlag(FontStyle.Bold);
            Editor.Styles[index].Italic = style.HasFlag(FontStyle.Italic);
            Editor.Styles[index].Underline = style.HasFlag(FontStyle.Underline);
        }

    }

}