using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trax {

    /// <summary>
    /// Interface for Scintilla Container Lexer
    /// </summary>
    public interface IContainerLexer {
        /// <summary>
        /// Type of the static class containing constants defining style indices
        /// The names of the constants are used as property names in color schemes
        /// </summary>
        Type SyntaxType { get; }
        /// <summary>
        /// Parses document range specified
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        void ApplyStyles(int start, int end);
    }

}