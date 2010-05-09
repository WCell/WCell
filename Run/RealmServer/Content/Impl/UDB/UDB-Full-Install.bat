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
REM # udbdir  - Main UDB directory on your harddisk (for example "E:\Documents\My Documents\Code\UDB")
REM #########################################

set user=root
set pass=r00t
set wdb=wcellrealmserver
set udbdir="e:\coding\C#\WCellOther\UDB\"

REM ############################################################################
REM #
REM #    A D V A N C E D   U S E R   C O N F I G U R A T I O N   A R E A
REM #
REM ############################################################################
set server=localhost
set port=3306
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
ECHO		 c = Install all Changesets (385-387)
ECHO.
ECHO		 385 = Install Changeset 385
ECHO		 386 = Install Changeset 386
ECHO		 387 = Install Changeset 387
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
if %l%==385 goto changeset385
if %l%==386 goto changeset386
if %l%==387 goto changeset387
goto error

:import
CLS
ECHO.
ECHO.
ECHO [Importing] Started...
ECHO [Importing] UDB database rev 384...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Full_DB\UDB_0.11.6_Core_8734_SD2_1480.sql
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
ECHO [Importing] UDB database changeset 385...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.11.6_additions\385_corepatch_mangos_8735_to_8798.sql
ECHO [Importing] UDB updatepack 385...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.11.6_additions\385_updatepack_mangos.sql
ECHO [Importing] Finished
ECHO [Importing] UDB database changeset 386...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.11.6_additions\386_corepatch_mangos_8799_to_8994.sql
ECHO [Importing] UDB updatepack 386...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.11.6_additions\386_updatepack_mangos.sql
ECHO [Importing] Finished
ECHO [Importing] UDB database changeset 387...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.11.6_additions\387_corepatch_mangos_8994_to_9310.sql
ECHO [Importing] UDB updatepack 387...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.11.6_additions\387_updatepack_mangos.sql
ECHO [Importing] Finished
ECHO.
PAUSE    
GOTO menu

:changeset385
CLS
ECHO.
ECHO.
ECHO Started...
ECHO [Importing] UDB database changeset 385...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.11.6_additions\385_corepatch_mangos_8735_to_8798.sql
ECHO [Importing] UDB updatepack 385...
ECHO [Importing] Finished
ECHO.
PAUSE    
GOTO menu

:changeset386
CLS
ECHO.
ECHO.
ECHO Started...
ECHO [Importing] UDB database changeset 386...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.11.6_additions\386_corepatch_mangos_8799_to_8994.sql
ECHO [Importing] UDB updatepack 386...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.11.6_additions\386_updatepack_mangos.sql
ECHO [Importing] Finished
ECHO.
PAUSE    
GOTO menu

:changeset387
CLS
ECHO.
ECHO.
ECHO Started...
ECHO [Importing] UDB database changeset 387...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.11.6_additions\387_corepatch_mangos_8994_to_9310.sql
ECHO [Importing] UDB updatepack 387...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %udbdir%\Updates\0.11.6_additions\387_updatepack_mangos.sql
ECHO [Importing] Finished
ECHO.
PAUSE    
GOTO menu

:extract
CLS
ECHO.
unrar.exe
unrar x -y %udbdir%Full_DB\UDB_0.11.6_Core_8734_SD2_1480.rar %udbdir%Full_DB\
ECHO.
PAUSE.
GOTO menu

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
ECHO [FAILURE] (e.g. set udbdir="E:\Code\UDB")
PAUSE
GOTO quit

:quit