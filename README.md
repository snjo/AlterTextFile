# AlterTextFile

	ALTERTEXTFILE.EXE -source FILENAME -target FILENAME (arguments)

Processes a text file to remove unwanted lines or strings, or change codepage
By Andreas Aakvik Gogstad

|  Argument      | Parameters          | Function                                                           |
|----------------|---------------------|--------------------------------------------------------------------|
| -source        | FILEPATH            | The file to read from                                              |
| -target        | FILEPATH            | The file to write to                                               |
| -inencoding    | CODEPAGE            | Read source file using UTF8/Latin1/865...                          |
| -outencoding   | CODEPAGE            | Write to target file using UTF8/Latin1/865...                      |
| -rlblank       |                     | removes blank lines                                                |
| -rlfirst       | number              | remove n number of lines from start of file                        |
| -rllast        | number              | remove n number of lines from end of file                          |
| -rlcontains    | "text"              | remove lines containing string (case insensitive)                  |
| -rlexact       | "text"              | remove lines containing an exact match (case insensitive)          |
| -casesensitive |                     | Use strict case matching                                           |
| -replace       | "oldText" "newText" | replace all instances of a string with (always case sensitive)     | 
| -trim          |                     | Trim white space at the start and end of lines                     |
| -verbose       |                     | Displays the effect of each argument during processing             |
| -help          |                     | This page                                                          |

## Output
You can use the same filepath as both source and target, but the program will not default to that, you must manually specify the target parameter.

## Encoding
If you see incorrect characters in the output, specify the -inencoding parameter. If you're unsure of the file's encoding, try opening it in Notepad++ and checking the Encoding menu.
You only need to change the -outencoding if the receiving system isn't expecting a UTF-8 file.

## Valid code pages
	default
	utf8 / utf-8
	utf8bom / utf-8-bom
	latin1
	ascii
	numbered codepage (ex. 865 for Nordic DOS)
	
Other named codepages will be evaluated by .net Encoding.GetEncoding