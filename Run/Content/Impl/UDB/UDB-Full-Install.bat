@ECHO off
CALL CONFIGURATION.bat

REM ############################################################################
REM #
REM #     D O   N O T   M O D I F Y   B E Y O N D   T H I S   P O I N T
REM #
REM ############################################################################

ECHO.
ECHO User: %user%
ECHO Pass: %pass%
ECHO wdb: %wdb%
ECHO udbdir: %udbdir%
ECHO server: %server%
ECHO port: %port%
ECHO udb-main: %udb-main%
ECHO.
ECHO Check the above variables to see if they are correct before continuing
ECHO These can be edited in CONFIGURATION.bat
ECHO.
pause

if %user%==changeme GOTO error2
if %pass%==changeme GOTO error2
if "%udbdir%"=="changeme" GOTO error3

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
ECHO		 c = Install all Changesets (402)
ECHO.
ECHO		 402 = Install Changeset 402
ECHO.
ECHO.
ECHO		 x - Exit
ECHO.
set l=""
set /p  l=            Enter Letter:
if %l%==* GOTO error-menu
if %l%==i GOTO import
if %l%==I GOTO import
if %l%==E GOTO extract
if %l%==e GOTO extract
if %l%==c GOTO changesets
if %l%==C GOTO changesets
if %l%==x GOTO cleanup
if %l%==X GOTO cleanup
if "%l%"=="402" GOTO changeset402
GOTO error-menu

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
ECHO   [Importing] UDB database changeset 402...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.12.2_additions\402_corepatch_mangos_11377_to_11792.sql
ECHO   [Importing] UDB updatepack 402...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.12.2_additions\402_updatepack_mangos.sql
ECHO [Importing] Finished
ECHO.
PAUSE
GOTO menu

:changeset402
CLS
ECHO.
ECHO.
ECHO Started...
ECHO [Importing] UDB database changeset 402...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.12.2_additions\402_corepatch_mangos_11377_to_11792.sql
ECHO [Importing] UDB updatepack 402...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.12.2_additions\402_updatepack_mangos.sql
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
PAUSE
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

:error-menu
CLS
ECHO.
ECHO.
ECHO [ERROR] Invalid menu option please try again.
ECHO [ERROR] You will be returned to the main menu.
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
GOTO cleanup 

:error3
ECHO [FAILURE] You did not change the UDB directory variable
ECHO [FAILURE] Please edit CONFIGURATION.bat and enter the proper directory
ECHO [FAILURE] (e.g. set udbdir=E:\Code\UDB\)
PAUSE
GOTO cleanup

:cleanup
REM Clear all set environment variables
set user=""
set pass=""
set wdb=""
set udbdir=""
set server=""
set port=""
set udb-main=""
set rusdbdir=""
GOTO:EOF