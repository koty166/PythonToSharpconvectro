import re
data.class = open("FIO").read() + 0
i = 0
out = ""
for j in (data.split("\n")):
	if(j == ""):
		continue
	if(i == 4):
		i = 0
		print(out)
		out=""
	if(i ==2):
		out+= j + "\t"
	i+=1