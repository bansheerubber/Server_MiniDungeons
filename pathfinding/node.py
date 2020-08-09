class Node:
	# the position argument is a threeple
	def __init__(self, id, position):
		self.id = id
		self.position = position
		self.neighbors = []
		self._simple_neighbors = []
	
	def add_simple_neighbor(self, neighbor):
		if neighbor in self.neighbors:
			self._simple_neighbors.append(neighbor)

	def set_simple_neighbors(self):
		self.neighbors = self._simple_neighbors
		self._simple_neighbors = []