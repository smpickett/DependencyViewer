# DependencyViewer
Helps find those run-time assembly issues

Summary
------------
This console application will
 - Visually show assembly relationships
 - List assembly versions
 - List missing assemblies
 - Indicate likely assembly binding redirect
 
Usage
----------------
    DepView <root path>
  where [root Path] is the root directory of the .NET assemblies to look through.
  
  All files with a DLL and EXE extension will be considered, and sub paths will be searched.
    
Example Output
---------------------

	+------------------------------------------------+------------------+------------------+------------------+-------+--------+----------+--------------------------------------+
	| Assembly Name                                  | Ver Asm          | Ver File         | Ver Prod         |       | Signed | Resolved | Possible Issue                       |
	+------------------------------------------------+------------------+------------------+------------------+-------+--------+----------+--------------------------------------+
	| PaintDotNet.SystemLayer.Native.x64.dll         |                  | 3.511.0.0        | 3.511.0.0        | Amd64 |        | (N/A)    |                                      |
	| ShellExtension_x64.dll                         |                  | 3.511.0.0        | 3.511.0.0        |       |        | (N/A)    |                                      |
	| ShellExtension_x86.dll                         |                  | 3.511.0.0        | 3.511.0.0        |       |        | (N/A)    |                                      |
	| wiaaut.dll                                     |                  | 6.2.9200.16384   | 6.2.9200.16384   |       |        | (N/A)    |                                      |
	| PaintDotNet.Native.x64.dll                     |                  | 3.511.0.0        | 3.511.0.0        |       |        | (N/A)    |                                      |
	| PaintDotNet.Native.x86.dll                     |                  | 3.511.0.0        | 3.511.0.0        |       |        | (N/A)    |                                      |
	| PaintDotNet                                    | 3.511.4977.23448 | 3.511.4977.23448 | 3.511.4977.23448 | MSIL  |        | Yes      |                                      |
	|  \-PaintDotNet.Base                            | 3.511.4977.23436 | 3.511.4977.23436 | 3.511.4977.23436 | MSIL  |        | Yes      |                                      |
	|  \-PaintDotNet.Core                            | 3.511.4977.23444 | 3.511.4977.23444 | 3.511.4977.23444 | MSIL  |        | Yes      |                                      |
	|  |  \-PaintDotNet.Base                         | 3.511.4977.23436 | 3.511.4977.23436 | 3.511.4977.23436 | MSIL  |        | Yes      |                                      |
	|  |  \-PaintDotNet.SystemLayer                  | 3.511.4977.23442 | 3.511.4977.23442 | 3.511.4977.23442 | MSIL  |        | No       | 3.511.4973.33365 -> 3.511.4977.23436 |
	|  |  |  \-PaintDotNet.Base                      | 3.511.4977.23436 | 3.511.4977.23436 | 3.511.4977.23436 | MSIL  |        | Yes      |                                      |
	|  |  |  \-Interop.WIA                           | 1.0.0.0          | 1.0.0.0          | 1.0.0.0          | MSIL  |        | Yes      |                                      |
	|  |  |  \-PaintDotNet.SystemLayer.Native.x86    | 3.511.4973.33370 | 3.511.0.0        | 3.511.0.0        | X86   |        | No       | Unable to load                       |
	|  |  |  \-PaintDotNet.Base                      | 3.511.4977.23436 | 3.511.4977.23436 | 3.511.4977.23436 | MSIL  |        | Yes      |                                      |
	|  |  |  \-PaintDotNet.SystemLayer.Native.x64    | 3.511.4977.23441 |                  |                  |       |        |          | Not Found                            |
	|  |  \-PaintDotNet.Resources                    | 3.511.4977.23443 | 3.511.4977.23443 | 3.511.4977.23443 | MSIL  |        | Yes      |                                      |
	|  |  |  \-PaintDotNet.Base                      | 3.511.4977.23436 | 3.511.4977.23436 | 3.511.4977.23436 | MSIL  |        | Yes      |                                      |
	|  |  |  \-PaintDotNet.SystemLayer               | 3.511.4977.23442 | 3.511.4977.23442 | 3.511.4977.23442 | MSIL  |        | No       | 3.511.4973.33365 -> 3.511.4977.23436 |
	+------------------------------------------------+------------------+------------------+------------------+-------+--------+----------+--------------------------------------+
