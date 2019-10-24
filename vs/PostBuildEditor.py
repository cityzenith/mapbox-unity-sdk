#@echo off

#robocopy %1 %2 CZ.Unity.Editor.* Mapbox.Unity.Editor.*
#if %ERRORLEVEL% LSS 2 echo OKCOPY & goto end

#:end
import sys
import os
import shutil

if len(sys.argv) < 3:
    print "Exiting without operation: not enough arguments"
    sys.exit(0)

inclusion_list = [
    "CZ.Unity.Editor", "Mapbox.Unity.Editor", "GLTFUnityLib.Editor", "XMLLayout.Editor.dll", "UnityPDF.Editor.dll", "UnityEmbedHTML.Editor.dll", "ChartAndGraph.Editor.dll"]

src = sys.argv[1]
dest = sys.argv[2]

for directory, subdirectories, files in os.walk(src):
    for file in files:
        for sub in inclusion_list:
            if sub in file:
                print "including " + file
                srcFile = os.path.join(src, file)
                destFile = os.path.join(dest, file)
                shutil.copy(srcFile, destFile)
