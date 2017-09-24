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
ECHO		 c = Install all Changesets (394, 395, 396, 397, 398, 399, 400, 401)
ECHO             v = Install Vehicle data patch
ECHO.
ECHO		 394 = Install Changeset 394
ECHO		 395 = Install Changeset 395
ECHO		 396 = Install Changeset 396
ECHO		 397 = Install Changeset 397
ECHO		 398 = Install Changeset 398
ECHO		 399 = Install Changeset 399
ECHO		 400 = Install Changeset 400
ECHO		 401 = Install Changeset 401
ECHO.
ECHO.
ECHO		 x - Exit
ECHO.
set /p  l=            Enter Letter:
if %l%==* GOTO error
if %l%==i GOTO import
if %l%==I GOTO import
if %l%==E GOTO extract
if %l%==e GOTO extract
if %l%==c GOTO changesets
if %l%==C GOTO changesets
if %l%==v GOTO vehicles
if %l%==V GOTO vehicles
if %l%==x GOTO quit
if %l%==X GOTO quit
if "%l%"=="394" GOTO changeset394
if "%l%"=="395" GOTO changeset395
if "%l%"=="396" GOTO changeset396
if "%l%"=="397" GOTO changeset397
if "%l%"=="398" GOTO changeset398
if "%l%"=="399" GOTO changeset399
if "%l%"=="400" GOTO changeset400
if "%l%"=="401" GOTO changeset401
goto error

:import
CLS
ECHO.
ECHO.
ECHO [Importing] Started...
ECHO [Importing] Main UDB database ...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Full_DB\%udb-main%.sql
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < .\Data\vehicle_patch.sql
ECHO [Importing] Finished
ECHO.
PAUSE    
GOTO menu

:vehicles
CLS
ECHO.
ECHO.
ECHO [Importing] Started...
ECHO [Importing] Vehicle data patch ...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < .\Data\vehicle_patch.sql
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
ECHO [Importing] UDB database changeset 394...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.12.1_additions\394_corepatch_mangos_10546_to_10720.sql
ECHO [Importing] UDB updatepack 394...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.12.1_additions\394_updatepack_mangos.sql
ECHO [Importing] UDB database changeset 395...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.12.1_additions\395_corepatch_mangos_10721_to_10892.sql
ECHO [Importing] UDB updatepack 395...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.12.1_additions\395_updatepack_mangos.sql
ECHO [Importing] UDB updatepack 396...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.12.1_additions\396_updatepack_mangos.sql
ECHO [Importing] UDB database changeset 397...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.12.1_additions\397_corepatch_mangos_10905_to_11064.sql
ECHO [Importing] UDB updatepack 397...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.12.1_additions\397_updatepack_mangos.sql
ECHO [Importing] UDB database changeset 398...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.12.1_additions\398_corepatch_mangos_11065_to_11156.sql
ECHO [Importing] UDB updatepack 398...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.12.1_additions\398_updatepack_mangos.sql
ECHO [Importing] UDB database changeset 399...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.12.1_additions\399_corepatch_mangos_11157_to_11242.sql
ECHO [Importing] UDB updatepack 399...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.12.1_additions\399_updatepack_mangos.sql
ECHO [Importing] UDB updatepack 400...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.12.1_additions\400_updatepack_mangos.sql
ECHO [Importing] UDB database changeset 401...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.12.1_additions\401_corepatch_mangos_11305_to_11376.sql
ECHO [Importing] UDB updatepack 401...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.12.1_additions\401_updatepack_mangos.sql
ECHO [Importing] Finished
ECHO.
PAUSE    
GOTO menu

:changeset394
CLS
ECHO.
ECHO.
ECHO Started...
ECHO [Importing] UDB database changeset 394...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.12.1_additions\394_corepatch_mangos_10546_to_10720.sql
ECHO [Importing] UDB updatepack 394...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.12.1_additions\394_updatepack_mangos.sql
ECHO [Importing] Finished
ECHO.
PAUSE
GOTO menu

:changeset395
CLS
ECHO.
ECHO.
ECHO Started...
ECHO [Importing] UDB database changeset 395...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.12.1_additions\395_corepatch_mangos_10721_to_10892.sql
ECHO [Importing] UDB updatepack 395...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.12.1_additions\395_updatepack_mangos.sql
ECHO [Importing] Finished
ECHO.
PAUSE
GOTO menu

:changeset396
CLS
ECHO.
ECHO.
ECHO Started...
ECHO [Importing] UDB updatepack 396...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.12.1_additions\396_updatepack_mangos.sql
ECHO [Importing] Finished
ECHO.
PAUSE
GOTO menu

:changeset397
CLS
ECHO.
ECHO.
ECHO Started...
ECHO [Importing] UDB database changeset 397...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.12.1_additions\397_corepatch_mangos_10905_to_11064.sql
ECHO [Importing] UDB updatepack 397...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.12.1_additions\397_updatepack_mangos.sql
ECHO [Importing] Finished
ECHO.
PAUSE
GOTO menu

:changeset398
CLS
ECHO.
ECHO.
ECHO Started...
ECHO [Importing] UDB database changeset 398...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.12.1_additions\398_corepatch_mangos_11065_to_11156.sql
ECHO [Importing] UDB updatepack 398...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.12.1_additions\398_updatepack_mangos.sql
ECHO [Importing] Finished
ECHO.
PAUSE
GOTO menu

:changeset399
CLS
ECHO.
ECHO.
ECHO Started...
ECHO [Importing] UDB database changeset 399...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.12.1_additions\399_corepatch_mangos_11157_to_11242.sql
ECHO [Importing] UDB updatepack 399...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.12.1_additions\399_updatepack_mangos.sql
ECHO [Importing] Finished
ECHO.
PAUSE
GOTO menu

:changeset400
CLS
ECHO.
ECHO.
ECHO Started...
ECHO [Importing] UDB updatepack 400...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.12.1_additions\400_updatepack_mangos.sql
ECHO [Importing] Finished
ECHO.
PAUSE
GOTO menu

:changeset401
CLS
ECHO.
ECHO.
ECHO Started...
ECHO [Importing] UDB database changeset 401...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.12.1_additions\401_corepatch_mangos_11305_to_11376.sql
ECHO [Importing] UDB updatepack 401...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.12.1_additions\401_updatepack_mangos.sql
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
ECHO [FAILURE] Please edit CONFIGURATION.bat and enter the proper directory
ECHO [FAILURE] (e.g. set udbdir=E:\Code\UDB\)
PAUSE
GOTO quit

:quit
REM Clear all set environment variables
set user=""
set pass=""
set wdb=""
set udbdir=""
set server=""
set port=""
set udb-main=""
set rusdbdir=""