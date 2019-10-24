#@echo off

#robocopy %1 %2 /xf UnityEngine.dll UnityEngine.UI.dll nunit.framework.dll nunit.framework.xml UnityEngine.TestRunner
#if %ERRORLEVEL% LSS 2 echo OKCOPY & goto end

#:end
import sys
import os
import shutil

if len(sys.argv) < 3:
    print "Exiting without operation: not enough arguments"
    sys.exit(0)

exclusion_list = ["UnityEngine.dll", "UnityEngine.UI.dll", "nunit.framework.dll", "nunit.framework.xml", "UnityEngine.TestRunner"]

src = sys.argv[1]
dest = sys.argv[2]
print "PostBuild.py: Running"
print "source: " + src
print "destination: " + dest

for directory, subdirectories, files in os.walk(src):
    print "walking directories and subdirectories"
    for file in files:
        if file not in exclusion_list:
            srcFile = os.path.join(directory, file)
            destFile = os.path.join(dest, file)

            print srcFile
            print destFile
            shutil.copy(srcFile, destFile)
print "PostBuild.py: Finished"
