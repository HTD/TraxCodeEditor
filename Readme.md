# Trax Code Editor Project

As **[Scintilla](http://www.scintilla.org/SciTE.html)** is probably the best text editor core for Windows
and ScintillaNET is its official and the most complete .NET binding -
Trax Code Editor aims to be the most complete code editor control out there.

It's built on top of **[ScintillaNET](https://github.com/jacobslusser/ScintillaNET)**
and provides advanced features known
from the best proffessional code editors. It's designed to be full
featured code editor in WinForms control.

Features as automatic indentation, syntax highlighting and folding,
code completion, search and replace functions, file and encoding
handling should be standard, built in, written once and for good.

Trax Code Editor aims to be the one code editor to rule them all.

## Why the name?

**[Trax](https://github.com/HTD/Trax)** is the name of post-processing
tool for [train simulator](http://eu07.pl)
scenery files. I decided to replace its text editor with another
one, based on Scintilla core.

The train simulator itself uses its own hermetic and esoteric file
format and syntax. There was no human-friendly editor for those
files, so I decided to make one.

As Trax became a versatile tool, with map view, project handling,
debugging features, more like IDE than editor - Trax Code Editor
became separate project with differet goals.

Once mature enough, it will be used in Trax.

## Why TCE, not the others?

Because it's **fast**. I mean really fast. Designed to process really
huge text files without slightest lags. It's also light.

The Scintilla core is written in C++, so it speaks for itself.
Scintilla core and ScintillaNET code is very mature, super high
quality and constant development.

The most important features of Trax Code Editor are:

 - bulit in, easy editable beautiful color schemes
 - easy color scheme extensibility
 - automatic indentation
 - powerful custom lexer and parser interface
 - TextBox backward compatibility
 - **zero configuration!**

You can just insert the control and use. No configuration
is needed for most of the advanced features.

## Is there all there is?

No! I've just begun. It will end up as a little Visual Studio
written in Visual Studio. Yo dog! ;)

There will be more. I miss good code visualisation and really
intelligent code completion. You don't have code visualisation
(as object structure tree) even in Visual Studio!

If you ever used Komodo IDE or Komodo Edit and stumbled upon
**[NST](https://github.com/HTD/NST)** extension, yes, that's mine.
And it's so obsolete!

You don't have good, working code completion in most editors.
Only in expensive IDE-s, but even there they are so incomplete.

There will be those, and load of other features.
Don't worry. TCE will remain super compact and super fast.

## Road map

 - automatic lexer detection and color scheme resetting
 - super overwrite mode (with no insert and delete)
 - search, replace, go to line dialogs
 - open, save, save as dialogs
 - basic, universal, automatic code completion
 - better highlighting for HTML, CSS and... C#
 - real code intelligence...

## Current build

The current build use the latest ScintillaNET from GitHub,
with bugs fixed since latest release.
Do not use 3.5.0 from NuGet package,
because some features (like document loader) will break.**