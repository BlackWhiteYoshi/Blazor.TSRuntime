###########################################
#
# goes throgh the folder and subfolders
#   if file name is "*.received.txt"
#     rename file to "*.verified.txt" (and overwrite existing file if any)
#
###########################################


import os
import subprocess

for (dirpath, dirnames, filenames) in os.walk(os.path.dirname(__file__)):
    for filename in filenames:
        if filename.endswith(".received.txt"):
            basename = filename[:-13]
            os.system(f"move /y {dirpath}\\{basename}.received.txt {dirpath}\\{basename}.verified.txt")
            print(f"accepted {basename}\n")
