using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using ScintillaNET;

namespace Trax {

    /// <summary>
    /// Class used to import objects with color properties as Scintilla color schemes
    /// </summary>
    public sealed class ColorScheme {

        /// <summary>
        /// Common colors enumeration (no syntax)
        /// </summary>
        public enum CommonColors {
            Background,
            Text,
            SelectionBack,
            SelectionFore,
            CaretLineBack,
            AditionalSelectionFore,
            AditionalSelectionBack,
            Caret,
            LineNumberBack,
            LineNumberFore,
            CallTipBack,
            CallTipFore,
            IndentGuide
        }

        /// <summary>
        /// Settings instance or any object with Color properties
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
        /// Creates color scheme for editor from Settings instance or any object with Color properties
        /// </summary>
        /// <param name="editor"></param>
        /// <param name="source"></param>
        public ColorScheme(CodeEditor editor, object source) {
            Editor = editor;
            Source = source;
            Properties = Source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var commonColorNames = Enum.GetNames(typeof(CommonColors));
            var colorProperties = Properties.Where(p => p.PropertyType == typeof(Color));
            Properties = colorProperties != null ? colorProperties.ToArray() : null;
            if (Properties != null) {
                var syntaxProperties = Properties.Where(p => !commonColorNames.Contains(p.Name));
                if (syntaxProperties != null) SyntaxProperties = syntaxProperties.ToArray();
            }
            Reset();
        }

        /// <summary>
        /// (Re)applies full color scheme to editor control
        /// </summary>
        public void Reset() {
            SetCommonColors();
            ResetSyntax();
        }

        /// <summary>
        /// (Re)applies lexer related color scheme to editor control
        /// </summary>
        public void ResetSyntax() {
            if (Editor.Lexer != Lexer.Null && Editor.Lexer != Lexer.Container) {
                var lexerType = typeof(Style).GetNestedTypes().FirstOrDefault(t => t.Name == Editor.Lexer.ToString());
                if (lexerType != null) SetSyntaxColors(lexerType);
            } else {
                if (Editor.ContainerLexer != null && Editor.ContainerLexer.SyntaxType != null)
                    SetSyntaxColors(Editor.ContainerLexer.SyntaxType);
            }
        }

        /// <summary>
        /// Sets common colors from Settings instance (if set)
        /// </summary>
        private void SetCommonColors() {
            PropertyInfo propertyInfo;
            object propertyValue = null;
            Color color = Color.Transparent;
            foreach (CommonColors commonColor in Enum.GetValues(typeof(CommonColors))) {
                propertyInfo = Properties.FirstOrDefault(p => p.Name == commonColor.ToString());
                propertyValue = (propertyInfo != null) ? propertyInfo.GetValue(Source) : null;
                if (propertyValue != null && propertyValue is Color) {
                    color = (Color)propertyValue;
                    switch (commonColor) {
                        case CommonColors.Background:
                            Editor.BackColor = color;
                            break;
                        case CommonColors.Text:
                            Editor.ForeColor = color;
                            break;
                        case CommonColors.SelectionBack:
                            Editor.SetSelectionBackColor(true, color);
                            break;
                        case CommonColors.SelectionFore:
                            Editor.SetSelectionForeColor(true, color);
                            break;
                        case CommonColors.AditionalSelectionBack:
                            Editor.SetAdditionalSelBack(color);
                            break;
                        case CommonColors.AditionalSelectionFore:
                            Editor.SetAdditionalSelFore(color);
                            break;
                        case CommonColors.CaretLineBack:
                            Editor.CaretLineBackColor = color;
                            break;
                        case CommonColors.Caret:
                            Editor.CaretForeColor = color;
                            break;
                        case CommonColors.LineNumberBack:
                            Editor.Styles[Style.LineNumber].BackColor = color;
                            break;
                        case CommonColors.LineNumberFore:
                            Editor.Styles[Style.LineNumber].ForeColor = color;
                            break;
                        case CommonColors.CallTipBack:
                            Editor.CallTipBackColor = color;
                            break;
                        case CommonColors.CallTipFore:
                            Editor.CallTipForeColor = color;
                            break;
                        case CommonColors.IndentGuide:
                            Editor.Styles[Style.IndentGuide].ForeColor = color;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Sets editor colors with name matching specified syntax type
        /// </summary>
        /// <param name="syntax"></param>
        private void SetSyntaxColors(Type syntax) {
            if (SyntaxProperties == null) return;
            var constants = syntax.GetFields();
            var syntaxStyles = new System.Collections.Generic.Dictionary<string, int>();
            foreach (var fieldInfo in constants) syntaxStyles.Add(fieldInfo.Name, (int)fieldInfo.GetRawConstantValue());
            foreach (var property in SyntaxProperties) {
                var name = property.Name;
                var bare = name;
                var backIndex = name.IndexOf("Back");
                var foreIndex = name.IndexOf("Fore");
                if (backIndex > 0) bare = name.Substring(0, backIndex);
                if (foreIndex > 0) bare = name.Substring(0, foreIndex);
                var value = property.GetValue(Source);
                if (syntaxStyles.ContainsKey(bare) && value != null && value is Color) {
                    if (backIndex > 0) Editor.Styles[syntaxStyles[bare]].BackColor = (Color)value;
                    else Editor.Styles[syntaxStyles[bare]].ForeColor = (Color)value;
                }
            }
        }

    }

}