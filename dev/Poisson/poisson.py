import random

N = int(input("N="));
list = [];

while len(list) < N:
	x = random.uniform(-1.0, 1.0)
	y = random.uniform(-1.0, 1.0)
	if (x*x + y*y <= 1.0):
		list.append((x,y))

for coord in list:
	print("\tvec2"+str(coord)+",")