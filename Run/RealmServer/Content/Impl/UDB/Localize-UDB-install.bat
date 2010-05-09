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
REM # rusdbdir  - directory on your harddisk with localized DB (RUSDB - Russia (https://sourceforge.net/projects/rusdb/), GMDB - Germany (https://sourceforge.net/projects/gm-db/), etc) (for example d:\RUSDB\trunk\)
REM #########################################

set user=changeme
set pass=changeme
set wdb=changeme
set rusdbdir=changeme

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
if %rusdbdir% == "changeme" GOTO error3
:menu
cls
ECHO.
ECHO.
ECHO		####################################
ECHO		#######                      #######
ECHO		######        Database        ######
ECHO		#######     Localize Tool    #######
ECHO		####################################
ECHO.
ECHO		Please type the letter for the option:
ECHO.
ECHO		 l = Localize DB
ECHO.
ECHO		 x - Exit
ECHO.
set /p l=            Enter Letter:
if %l%==* goto error
if %l%==l goto loc
if %l%==L goto loc
if %l%==x goto quit
if %l%==X goto quit
goto error

:loc
CLS
ECHO.
ECHO.
ECHO [Importing] Started...
ECHO [Importing] Localizing mangos_string table...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %rusdbdir%\mangos_string.sql
ECHO [Importing] Finished
ECHO [Importing] Localizing locales_quest table...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %rusdbdir%\locales_quest.sql
ECHO [Importing] Finished
ECHO [Importing] Localizing locales_page_text table...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %rusdbdir%\locales_page_text.sql
ECHO [Importing] Finished
ECHO [Importing] Localizing locales_npc_text table...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %rusdbdir%\locales_npc_text.sql
ECHO [Importing] Finished
ECHO [Importing] Localizing locales_item table...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %rusdbdir%\locales_item.sql
ECHO [Importing] Finished
ECHO [Importing] Localizing locales_gameobject table...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %rusdbdir%\locales_gameobject.sql
ECHO [Importing] Finished
ECHO [Importing] Localizing locales_creature table...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %rusdbdir%\locales_creature.sql
ECHO [Importing] Finished
ECHO [Importing] Localizing locales_achievement_reward table...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %rusdbdir%\locales_achievement_reward.sql
ECHO [Importing] Finished
ECHO [Importing] Localizing db_script_string table...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %rusdbdir%\db_script_string.sql
ECHO [Importing] Finished
ECHO [Importing] Localizing creature_ai_texts table...
mysql -h %server% --user=%user% --password=%pass% --port=%port% %wdb% < %rusdbdir%\creature_ai_texts.sql
ECHO [Importing] Finished
ECHO.
PAUSE    
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
ECHO [FAILURE] You did not change the RUDB(another localized DB) directory variable
ECHO [FAILURE] Please edit this script and enter the proper directory
ECHO [FAILURE] (e.g. set rusdbdir="E:\Code\RUDB\trunk")
PAUSE
GOTO quit

:quit