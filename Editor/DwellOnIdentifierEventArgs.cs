using System;
using System.Drawing;

namespace Trax.Editor {

    /// <summary>
    /// EventArgs for DwellOnIdentifier event
    /// </summary>
    public class DwellOnIdentifierEventArgs : EventArgs {
        /// <summary>
        /// Identifier on which the mouse cursor dwelled
        /// </summary>
        public string Identifier { get; private set; }
        /// <summary>
        /// Character range of the identifier
        /// </summary>
        public CharacterRange IdentifierRange { get; private set; }
        /// <summary>
        /// Visible text range
        /// </summary>
        public CharacterRange VisibleRange { get; private set; }
        /// <summary>
        /// Visible text context
        /// </summary>
        public string VisibleText { get; private set; }
        /// <summary>
        /// If set the text will be displayed as calltip
        /// </summary>
        public string CallTipText { get; set; }
        /// <summary>
        /// Creates DwellOnIdentifierEventArgs instance with read-only values set
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="identifierRange"></param>
        /// <param name="visibleRange"></param>
        /// <param name="visibleText"></param>
        public DwellOnIdentifierEventArgs(string identifier, CharacterRange identifierRange, CharacterRange visibleRange, string visibleText) {
            Identifier = identifier;
            IdentifierRange = identifierRange;
            VisibleRange = visibleRange;
            VisibleText = visibleText;
        }

    }

}