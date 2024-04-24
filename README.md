Packs a relocatable HP Inverse Assembler into a HFSLIF file structure, suitable
for transferring to a HP Logic Analyzer via FTP. This program provides an
alternative to the HP provided IALDOWN.EXE file, which only supports uploading
via a serial or GPIB connection.

usage:
HFSLIFWriter.exe inputFilePath outputFilePath fileDescription invasmFieldOpt

inputFilePath    Path to the relocatable inverse assembler file on disk.
                 Usually a ".A" file as output by ASM.EXE.
outputFilePath   Path to write the generated HFSLIF file to
fileDescription  A file description up to 32 characters to display on the logic
                 analyzer when listing this file on disk.
invasmFieldOpt   The control setting for the invasm field. Usage is the same as
                 in IALDOWN.EXE, a single character of A,B,C or D must be
                 specified as follows:
                    A = No "Invasm" Field
                    B = "Invasm" Field with no pop-up
                    C = "Invasm" Field with pop-up. 2 choices in pop-up.
                    D = "Invasm" Field with pop-up. 8 choices in pop-up.
