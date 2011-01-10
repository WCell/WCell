#!/bin/bash

style="Style.Light.xsl"
mode="Debug"

for arg in $*
do
    case "$arg" in
        --style=* )
            style=`echo "$arg" | sed 's/^--style=//'`
            if [ ! -e $style ]
            then
                echo "Style file not found."
                exit 1
            fi
            ;;
        --mode=* )
            mode=`echo "$arg" | sed 's/^--mode=//'`
            if [ $mode != "Debug" ] && [ $mode != "Release" ]
            then
                echo "Invalid mode - must be Debug/Release."
                exit 1
            fi
            ;;
        * )
            ;;
    esac
done

prepare()
{
    cp -f ../Run/$mode/*.dll .
    cp -f ../Run/$mode/*.exe .
    cp -f ../Run/$mode/*.xml .
}

cleanup()
{
    rm -f *.dll
    rm -f *.exe
    rm -f *.xml
}

process()
{
    for fn in `ls | grep WCell.*."$1"$ | grep -v .vshost.exe$`
    do
        noext=`basename "$fn" ."$1"`

        if [ -e "$noext.xml" ]
        then
            mdoc update -i "$noext.xml" -o "$fn.doc" "$fn"

            if [ -d "$fn.doc" ]
            then
                cd "$fn.doc"
                mdoc export-html -o "../$fn.web" --template="../$style" --force-update .
                cd ..
            fi
        fi
    done
}

prepare
process "dll"
process "exe"
cleanup
