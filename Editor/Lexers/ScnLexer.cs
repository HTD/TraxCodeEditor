using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Trax.Editor.Lexers {

    /// <summary>
    /// Lexer for MaSzyna Railway Vehicle Simulator scenery files
    /// </summary>
    public class ScnLexer : IContainerLexer {

        /// <summary>
        /// Scintilla editor control
        /// </summary>
        private readonly Scintilla Scintilla;

        /// <summary>
        /// Keyword definitions
        /// </summary>
        private static string[] _Keywords = new[] {
            "node end include config endconfig trainset dynamic enddynamic endtrainset time endtime sky endsky atmo endatmo light endlight movelight joinduplicatedevents movelight doubleambient event0 event1 event2 eventall0 eventall1 eventall2 event endevent multiple memcell endmemcell addvalues copyvalues updatevalues condition memcompare propability velocity isolated trackfree trackoccupied track endtrack traction endtraction triangles endtri sound endsound",
            "nobody headdriver reardriver passengers normal road river cross flat vis cu",
            "none * yes no",
            "FirstInit SetVelocity ShuntVelocity PassengerStopPoint FoulingPoint SetProximityVelocity OutsideStation Wait_for_orders Prepare_engine Change_direction Obey_train Warning_signal CabSignal Jump_to_order Jump_to_first_order Timetable Emergency_brake",
            "material endmaterial ambient diffuse specular"
        };

        /// <summary>
        /// Keywords unpacked
        /// </summary>
        public static string[][] Keywords;

        /// <summary>
        /// Unpacks keywords on first static class access
        /// </summary>
        static ScnLexer() {
            Keywords = new string[_Keywords.Length][];
            int i = 0;
            foreach (var set in _Keywords) Keywords[i++] = set.Split(' ');
        }

        /// <summary>
        /// IContainerLexer implementation: returns style number definition class type
        /// </summary>
        public Type SyntaxType { get { return typeof(ScnType); } }

        private struct State {
            public const int Unknown = 0;
            public const int Comment = 1;
            public const int Command = 2;
            public const int Content = 3;
            public const int Identifier = 4;
        }

        /// <summary>
        /// Style number definitions
        /// </summary>
        public struct ScnType {
            public const int Comment = 0;
            public const int Command = 1;
            public const int Content = 2;
            public const int Identifier = 3;
            public const int Separator = 4;
            public const int Number = 5;
            public const int Keyword = 6;
            public const int Keyword2 = 7;
            public const int Keyword3 = 8;
            public const int Keyword4 = 9;
            public const int Keyword5 = 10;
            public const int Switch = 11;
            public const int Red = 12;
            public const int Green = 13;
            public const int Blue = 14;
            public const int White = 15;
            public const int Orange = 16;
            public const int Time = 17;
            public const int Path = 18;
        }

        /// <summary>
        /// Creates a lexer not bound to a control
        /// </summary>
        public ScnLexer() { }

        /// <summary>
        /// Creates a lexer bound to scintilla editor control
        /// </summary>
        /// <param name="scintilla"></param>
        public ScnLexer(Scintilla scintilla) { Scintilla = scintilla; }

        /// <summary>
        /// Performs lexical analysis on input string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public LexemCollection Lex(string input) {
            var length = input.Length;
            var output = new ScnLexemCollection(input);
            int state = State.Unknown;
            char c;
            for (int i = 0, p = 0; i <= length; i++) {
                c = i < length ? input[i] : '\n';
                reprocess:
                switch (state) {
                    case State.Unknown:
                        if (c != ' ' && c != ';' && c != ':' && c != '\t' && c != '\r' && c != '\n') { // not separator
                            p = i;
                            state = State.Identifier;
                            goto reprocess;
                        }
                        break;
                    case State.Comment:
                        if (c == '\r' || c == '\n') { // line end
                            if (i - p > 0) output.Add(ScnType.Comment, p, i - p);
                            p = i + 1;
                            state = State.Unknown;
                            goto reprocess;
                        }
                        if (c == '$') { // command start
                            if (i - p > 0) output.Add(ScnType.Comment, p, i - p);
                            p = i;
                            state = State.Command;
                            goto reprocess;
                        }
                        break;
                    case State.Command:
                        if (c == ' ' || c == ';' || c == ':' || c == '\t' || c == '\r' || c == '\n') { // separator
                            if (i - p > 0) output.Add(ScnType.Command, p, i - p);
                            p = i + 1;
                            state = (c == '\r' || c == '\n') ? State.Unknown : State.Content;
                        }
                        break;
                    case State.Content:
                        if (c == '\r' || c == '\n') { // line end
                            output.Add(ScnType.Content, p, i - p);
                            p = i;
                            state = State.Unknown;
                            goto reprocess;
                        }
                        break;
                    case State.Identifier:
                        if (c == ' ' || c == ';' || c == ':' || c == '\t' || c == '\r' || c == '\n') // is separator
                            if (i < 1 || c != ':' || !Char.IsDigit(input[i - 1])) { // ':' is a not a part of time identifier
                                output.Add(ScnType.Identifier, p, i - p);
                                if (c == ';' || c == ':') output.Add(ScnType.Separator, i, 1);
                                p = i;
                                state = State.Unknown;
                                goto reprocess;
                            }
                        if (i > 0 && c == '/' && input[i - 1] == '/') {
                            if (i - p - 1 > 0) output.Add(ScnType.Identifier, p, i - p - 1);
                            p = i - 1;
                            state = State.Comment;
                            goto reprocess;
                        }
                        break;
                }
            }
            return output;
        }

        public void ApplyStyles(int start, int end) {
            if (end == start) return;
            var text = Scintilla.GetTextRange(start, end - start + 1);
            var lexems = Lex(text);
            var count = lexems.Count;
            int offset, lastOffset = 0;
            Lexem l = default(Lexem), p;
            for (int i = 0; i < count; i++) {
                l = lexems[i];
                if (i < 1) {
                    lastOffset = start + l.Start;
                    Scintilla.StartStyling(lastOffset);
                } else {
                    p = lexems[i - 1];
                    if (l.Type != p.Type) {
                        offset = start + l.Start;
                        Scintilla.SetStyling(offset - lastOffset, p.Type);
                        lastOffset = offset;
                    }
                }
            }
            Scintilla.SetStyling(end - lastOffset, l.Type);
        }

    }

    public struct Lexem {
        public int Type;
        public int Start;
        public int Length;
    }

    public class LexemCollection : List<Lexem> {

        public LexemCollection() : base(10 * 1024 * 1024) { }

        public LexemCollection(int capacity) : base(capacity) { }

        public string Debug(string input, Type typeInfo = null) {
            Dictionary<int, string> names = null;
            if (typeInfo != null) {
                names = new Dictionary<int, string>();
                var fields = typeInfo.GetFields();
                foreach (FieldInfo field in fields) names.Add((int)field.GetRawConstantValue(), field.Name);
            }
            var b = new StringBuilder();
            ForEach(l => b.AppendLine(String.Format("{0}: \"{1}\"", names != null ? names[l.Type] : l.Type.ToString(), input.Substring(l.Start, l.Length))));
            return b.ToString();
        }
    }

    public class ScnLexemCollection : LexemCollection {

        static Regex RxTime = new Regex(@"^\d?\d:\d\d$", RegexOptions.Compiled);

        static Regex RxPath = new Regex(@"[/\.]", RegexOptions.Compiled);

        public readonly string Buffer;
        
        public ScnLexemCollection(string buffer) : base(buffer.Length) { Buffer = buffer; }

        public void Add(int type, int start, int length) {
            if (Buffer != null && type == ScnLexer.ScnType.Identifier) {
                double n;
                var i = Buffer.Substring(start, length);
                if (Double.TryParse(i, NumberStyles.Float, CultureInfo.InvariantCulture, out n)) type = ScnLexer.ScnType.Number;
                else if (ScnLexer.Keywords[0].Contains(i)) type = ScnLexer.ScnType.Keyword;
                else if (ScnLexer.Keywords[1].Contains(i)) type = ScnLexer.ScnType.Keyword2;
                else if (ScnLexer.Keywords[2].Contains(i)) type = ScnLexer.ScnType.Keyword3;
                else if (ScnLexer.Keywords[3].Contains(i)) type = ScnLexer.ScnType.Keyword4;
                else if (ScnLexer.Keywords[4].Contains(i)) type = ScnLexer.ScnType.Keyword5;
                else if (i.EndsWith("-") || i.EndsWith("+") || 
                    ((length > 2 && Char.IsDigit(i[length - 3]) &&
                    (i.EndsWith("ac") || i.EndsWith("ad") || i.EndsWith("bc") || i.EndsWith("bd"))))) type = ScnLexer.ScnType.Switch;
                else if (i.EndsWith("_s1")) type = ScnLexer.ScnType.Red;
                else if (i.EndsWith("_sem_info") || i.EndsWith("_speedinfo") || i.EndsWith("_stopinfo")) type = ScnLexer.ScnType.Keyword4;
                else if (i.EndsWith("_s2") || i.EndsWith("_s3") || i.EndsWith("_s6") || i.EndsWith("_s7") || 
                    i.EndsWith("_s10") || i.EndsWith("_s11") || i.EndsWith("_os2") || i.EndsWith("_os3")) type = ScnLexer.ScnType.Green;
                else if (i.EndsWith("_s4") || i.EndsWith("_s5") || i.EndsWith("_s8") || i.EndsWith("_s9") ||
                    i.EndsWith("_s12") || i.EndsWith("_s13") || i.EndsWith("_os1") || i.EndsWith("_os4")) type = ScnLexer.ScnType.Orange;
                else if (i.EndsWith("_ms1")) type = ScnLexer.ScnType.Blue;
                else if (i.EndsWith("_ms2")) type = ScnLexer.ScnType.White;
                else if (RxTime.IsMatch(i)) type = ScnLexer.ScnType.Time;
                else if (RxPath.IsMatch(i)) type = ScnLexer.ScnType.Path;
            }
            base.Add(new Lexem { Type = type, Start = start, Length = length });
        }

    }

}