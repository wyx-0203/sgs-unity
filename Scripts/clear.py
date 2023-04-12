import os

def findAllFile(base):
    for root, ds, fs in os.walk(base):
        for f in fs:
            if not f[-2]==".":continue

            # print(f)
            # os.system("rm "+)
            # print("wget -P " + dir + " " + s)

findAllFile("./Assets/StreamingAssets")