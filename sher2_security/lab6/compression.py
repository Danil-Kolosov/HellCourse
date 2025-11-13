

class CompressionCode:
    #def __init__(self, generator_poly=None):
    def HaffmanCompression(self, text):
        alf = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
        alfLow = alf
        alfLow.lower()
        alfCount = []
        alf = alfLow.lower()
        
        for simbol in alf:  
            #print(simbol)
            if(text.count(simbol) != 0):          
                alfCount.append([simbol, text.count(simbol)])
        #print(text.count("a"))
        alfCount.sort(key=lambda item: item[1], reverse=True)
        compressedCode = []
        i = 1
        while len(alfCount) != 1:
            tempSum = alfCount[-2][1] + alfCount[-1][1]
            compressedCode.append([alfCount[-2][0], alfCount[-1][0], tempSum, "V" + str(i)])
            alfCount.remove(alfCount[-1])
            alfCount.remove(alfCount[-1])
            alfCount.append(["V" + str(i), tempSum])
            alfCount.sort(key=lambda item: item[1], reverse=True)
            i = i + 1
        alfCount = []
        for string in alf:
            alfCount.append(0)     
        code = ""
        for ind in range(-len(compressedCode), 0):
            alfCount[alf.index(compressedCode[ind][0])] = int((code + "0"))
            code = code + "1"
        print(alfCount)

tryClass = CompressionCode()
tryClass.HaffmanCompression("ccccccccccaaaaaabb")