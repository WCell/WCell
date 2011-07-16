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
REM # udbdir  - Main UDB directory on your harddisk (note the trailing backslash!, for example C:\Users\Villem\Documents\Kood\UDB\)
REM # The folder should contain 2 folders: Updates and Full_DB
REM # Remember not to include FULL_DB on the end of udbdir, only the main directory should be set here.
REM #########################################

set user=changeme
set pass=changeme
set wdb=wcellrealmserver
set udbdir="changeme"
set rusdbdir=changeme

REM ############################################################################
REM #
REM #    A D V A N C E D   U S E R   C O N F I G U R A T I O N   A R E A
REM #
REM ############################################################################

set server=localhost
set port=3306
set udb-main=UDB_0.12.1.393_mangos_10545_SD2_1833