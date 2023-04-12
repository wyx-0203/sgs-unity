import os

def findAllFile(base):
    dir="Assets/StreamingAssets/Image/General/Big"
    for root, ds, fs in os.walk(base):
        for f in fs:
            if not f[-4:]==".png":
                continue

            # print(f)
            s="https://web.sanguosha.com/10/pc/res/assets/runtime/general/big/static/"+f
            os.system("wget -P " + dir + " " + s)
            # print("wget -P " + dir + " " + s)

findAllFile("./Assets/StreamingAssets/Image/General/Seat")