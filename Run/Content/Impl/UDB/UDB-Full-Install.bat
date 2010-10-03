@ECHO off
REM ############################################################################
REM #
REM #      B A S I C   U S E R   C O N F I G U R A T I O N   A R E A
REM #
REM ############################################################################
REM #########################################
REM # server - Base Table host
REM # user - MySQL username
REM # pass - MySQL login password
REM # wdb  -  Database name
REM # udbdir  - Main UDB directory on your harddisk (note the trailing backslash!, for example "E:\Documents\My Documents\Code\UDB\")
REM # Remember not to include FULL_DB on the end of udbdir, only the main directory should be set here.
REM #########################################

set user="changeme"
set pass="changeme"
set wdb="wcellrealmserver"
set udbdir="changeme"

REM ############################################################################
REM #
REM #    A D V A N C E D   U S E R   C O N F I G U R A T I O N   A R E A
REM #
REM ############################################################################
set server="localhost"
set port="3306"
set udb-main="UDB_0.12.1.393_mangos_10545_SD2_1833"
@ECHO "User:"     %user%
@ECHO "Pass:"     %pass%
@ECHO "wdb:"      %wdb%
@ECHO "udbdir:"   %udbdir%
@ECHO "server:"   %server%
@ECHO "port:"     %port%
@ECHO "udb-main:" %udb-main%
@ECHO "Check the above variables to see if they are correct before continuing"
pause
REM ############################################################################
REM #
REM #     D O   N O T   M O D I F Y   B E Y O N D   T H I S   P O I N T
REM #
REM ############################################################################
if %user% == changeme GOTO error2
if %pass% == changeme GOTO error2
if %udbdir% == "changeme" GOTO error3
:menu
cls
ECHO.
ECHO.
ECHO		####################################
ECHO		#######                      #######
ECHO		######        Database        ######
ECHO		#######     Import Tool      #######
ECHO		####################################
ECHO.
ECHO		Please type the letter for the option:
ECHO.
ECHO		 e = Extract UDB
ECHO		 i = Install UDB
ECHO		 c = Install all Changesets (393)
ECHO.
ECHO		 393 = Install Changeset 393
ECHO.
ECHO.
ECHO		 x - Exit
ECHO.
set /p l=            Enter Letter:
if %l%==* goto error
if %l%==i goto import
if %l%==I goto import
if %l%==E goto extract
if %l%==e goto extract
if %l%==c goto changesets
if %l%==C goto changesets
if %l%==x goto quit
if %l%==X goto quit
if %l%==393 goto changeset393
goto error

:import
CLS
ECHO.
ECHO.
ECHO [Importing] Started...
ECHO [Importing] Main UDB database ...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Full_DB\%udb-main%.sql
ECHO [Importing] Finished
ECHO.
PAUSE    
GOTO menu

:changesets
CLS
ECHO.
ECHO.
ECHO [Importing] Started...
ECHO [Importing] UDB database changesets...
ECHO [Importing] UDB database changeset 393...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.12.1_additions\393_corepatch_mangos_10259_to_10545.sql
ECHO [Importing] UDB updatepack 393...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.12.1_additions\393_updatepack_mangos.sql
ECHO [Importing] Finished
ECHO.
PAUSE    
GOTO menu

:changeset393
CLS
ECHO.
ECHO.
ECHO Started...
ECHO [Importing] UDB database changeset 393...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.12.1_additions\393_corepatch_mangos_10259_to_10545.sql
ECHO [Importing] UDB updatepack 393...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.12.1_additions\393_updatepack_mangos.sql
ECHO [Importing] Finished
ECHO.
PAUSE    
GOTO menu

:extract
CLS
ECHO.
ECHO		####################################
ECHO		######        Extracting      ######
ECHO		####################################
ECHO.
ECHO.
ECHO        Is the file in ZIP or RAR format? Enter C to return to the menu.
ECHO.
ECHO.
ECHO.
set /p t=        Enter format:
if %t% == ZIP GOTO unzip
if %t% == RAR GOTO unrar
if %t% == zip GOTO unzip
if %t% == rar GOTO unrar
if %t% == C GOTO menu
if %t% == c GOTO menu
GOTO extract-error1

:unzip
ECHO.
7za e -y %udbdir%Full_DB\%udb-main%.zip -o%udbdir%Full_DB\
ECHO.
PAUSE.
GOTO menu

:unrar
ECHO.
unrar x -y %udbdir%Full_DB\%udb-main%.rar %udbdir%Full_DB\
ECHO.
PAUSE
GOTO menu

:extract-error1
ECHO.
ECHO [ERROR] Extracting from .%t% type of archives is not supported
ECHO [ERROR] Please enter ZIP or RAR
PAUSE
GOTO extract

:error
CLS
ECHO.
ECHO.
ECHO [ERROR] An error has occured, you will be directed back to the
ECHO [ERROR] main menu.
PAUSE    
GOTO menu

:error2
CLS
ECHO.
ECHO.
ECHO [FAILURE] You did not change the proper directives in this file.
ECHO [FAILURE] Please edit this script and fill in the proper MYSQL Information.
ECHO [FAILURE] When the information is correct: Please Try Again.
PAUSE    
GOTO quit 

:error3
ECHO [FAILURE] You did not change the UDB directory variable
ECHO [FAILURE] Please edit this script and enter the proper directory
ECHO [FAILURE] (e.g. set udbdir="E:\Code\UDB\")
PAUSE
GOTO quit

:quit