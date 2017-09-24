#!/bin/sh
source ./CONFIGURATION.conf

############################################################################
#
#     D O   N O T   M O D I F Y   B E Y O N D   T H I S   P O I N T
#
############################################################################

menu()
{
cls
while true; do
clear
	cat << EOB

		####################################
		#######                      #######
		######        Database        ######
		#######     Import Tool      #######
		####################################

		Please type the letter for the option:

		 a = Dont ask these stupid questions, do it all! NOW!
		 e = Extract UDB
		 i = Install UDB
		 c = Install all Changesets (394, 395, 396, 397, 398, 399, 400, 401)
		 v = Install Vehicle data patch

		 394 = Install Changeset 394
		 395 = Install Changeset 395
		 396 = Install Changeset 396
		 397 = Install Changeset 397
		 398 = Install Changeset 398
		 399 = Install Changeset 399
		 400 = Install Changeset 400
		 401 = Install Changeset 401
		 402 = Install Changeset 402


		 x - Exit

EOB
	read -p "           Enter Letter: " letter
	#convert the string to lower case
	declare -l letter
	letter=$letter

	case "$letter" in
		x)
		     quit;break;;
		a)
		     extract;import;changesets;;
		i)
		     import;;

		e)
		     extract;;

		c)
		     changesets;;

		v)
		     vehicles;;


		394)
		     changeset 394;;
		395)
		     changeset 395;;
		396)
		     changeset 396;;
		397)
		     changeset 397;;
		398)
		     changeset 398;;
		399)
		     changeset 399;;
		400)
		     changeset 400;;
		401)
		     changeset 401;;
		402)
		     changeset 402;;
	esac;
done
}

import()
{
clear
echo "[Importing] Started..."
echo "[Importing] Main UDB database ..."
mysql -h $server --user=$user --password=$pass --port=$port $wdb < $udbdir/Full_DB/$udbmain.sql
if [ "$?" == "0" ]; then
  echo "[Importing] Finished"
else
  echo "Error importing database"
fi
echo ""
read -p "Press ANY key to continue"
}


vehicles()
{
clear
echo "[Importing] Started..."
echo "[Importing] Vehicle data patch ..."
mysql -h $server --user=$user --password=$pass --port=$port $wdb < ./Data/vehicle_patch.sql
if [ "$?" == "0" ]; then
  echo "[Importing] Finished"
else
  echo "Error importing Vehicle data patch"
fi
echo ""
read -p "Press ANY key to continue"
}

changesets()
{
clear
echo ""
echo ""
echo "[Importing] Started..."
echo "[Importing] UDB database changesets..."
for filename in `find $udbdir/Updates/0.12.2_additions -maxdepth 1 -type f \( -iname "*.sql" ! -iname "*characters*" \)`
do
  echo "[Importing]"`basename $filename | cut -d '.' -f1`
  mysql -h $server --user=$user --password=$pass --port=$port $wdb < $filename
  if [ "$?" == "0" ]; then
    echo "[Imported]"
  else
    echo "Error importing"
  fi
done
echo "[Importing] WCell Vehicle patch..."
mysql -h $server --user=$user --password=$pass --port=$port $wdb < ./Data/vehicle_patch.sql
if [ "$?" == "0" ]; then
  echo "[Importing] Finished"
else
  echo "Error importing Vehicle data patch"
fi
echo ""
read -p "Press ANY key to continue"
}

changeset()
{
SETID=$1
clear
echo ""
echo ""
echo "Started..."
echo "[Importing] UDB database changeset "$SETID"..."
for filename in `find $udbdir/Updates/0.12.2_additions -maxdepth 1 -type f \( -iname "$SETID*" -iname "*.sql" ! -iname "*characters*" \)`
do
  echo "[Importing]"`basename $filename | cut -d '.' -f1`
  mysql -h $server --user=$user --password=$pass --port=$port $wdb < $filename 
done
echo "[Importing] Finished"
echo ""
read -p "Press ANY key to continue"
}


extract()
{
if [ -e $udbdir/Full_DB/$udbmain.zip ]
  then
  echo unzipping
  unzip -u -q $udbdir/Full_DB/$udbmain.zip -d $udbdir/Full_DB/
elif [ -e %udbdir%/Full_DB/%udb-main%.rar ]
  then
  echo unraring
  unrar x -y $udbdir/Full_DB/$udbmain.rar $udbdir/Full_DB/
else
  echo "Couldn't find UDB archive!!!"
fi
if [ $? == "0" ]
  then
  echo Success
else
  echo Failed
fi
read -p "Press ANY key to continue"
}

error()
{
if [ $1 == "2" ]; then
cat << ETWO
	[FAILURE] You did not change the proper directives in this file.
	[FAILURE] Please edit this script and fill in the proper MYSQL Information.
	[FAILURE] When the information is correct: Please try again.
ETWO
elif [ $1 == "3" ]; then
cat << ETHREE
	[FAILURE] You did not change the UDB directory variable
	[FAILURE] Please edit CONFIGURATION.conf and enter the proper directory
	[FAILURE] (e.g. udbdir=/home/Code/UDB)
ETHREE
else
cat << EONE
	[ERROR] An error has occured, you will be directed back to the
	[ERROR] main menu.
EONE
fi
read -p "Press ANY key to continue"
quit
}

quit()
{
#Clear all set environment variables
user=""
pass=""
wdb=""
udbdir=""
server=""
port=""
udbmain=""
rusdbdir=""
exit
}

echo ""
echo "User: "$user
echo "Pass: "$pass
echo "wdb: "$wdb
echo "udbdir: "$udbdir
echo "server: "$server
echo "port: "$port
echo "udb-main: "$udbmain
echo ""
echo "Check the above variables to see if they are correct before continuing"
echo "These can be edited in CONFIGURATION.conf"
echo ""
read -p "Press ANY key to continue"

test "$user" == 'changeme' && (error 2)

test "$pass" == 'changeme' && (error 2)

test "$udbdir" == 'changeme' && (error 3)
menu