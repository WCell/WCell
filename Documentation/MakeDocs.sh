#!/bin/bash

process()
{
    mkdir $1
    mv *.doc $1
    mv *.web $1
}

cleanup()
{
    rm -r -f $1
}

./MonoDoc.sh --mode="Debug" --style="Style.Light.xsl"
process "Light"

./MonoDoc.sh --mode="Debug" --style="Style.Dark.xsl"
process "Dark"

zip -r -9 "WCellDocs.zip" "Light" "Dark"

cleanup "Light"
cleanup "Dark"
