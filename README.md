# AlterTextFile

ALTERTEXTFILE.EXE -source FILENAME -target FILENAME (arguments)
Processes a text file to remove unwanted lines or strings, or change codepage
By Andreas Aakvik Gogstad

-source FILEPATH        The file to read from
-target FILEPATH        The file to write to
-inencoding CODEPAGE    Read source file using UTF8/Latin1/865...
-outencoding CODEPAGE   Write to target file using UTF8/Latin1/865...
-rlblank                removes blank lines
-rlfirst n              remove n number of lines from start of file
-rllast n               remove n number of lines from end of file
-rlcontains "aaa"       remove lines containing string (case insensitive)
-rlexact "aaa"          remove lines containing an exact match (case insensitive)
-casesensitive          Use strict case matching
-replace "aaa" "bbb"    replace all instances of a string with (always case sensitive)
-trim                   Trim white space at the start and end of lines
-verbose                Displays the effect of each argument during processing
-help                   This page