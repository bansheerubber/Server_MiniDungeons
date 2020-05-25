class Node:
	# the position argument is a threeple
	def __init__(self, id, position):
		self.id = id
		self.position = position
		self.neighbors = []