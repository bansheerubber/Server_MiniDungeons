import numpy as np
from scipy.spatial import Delaunay
import matplotlib.pyplot as plot
import random

class Navmesh:
	def __init__(self, id, nodes):
		self.nodes = nodes

		last_node = None
		same_z = True
		for _, node in nodes.items():
			if last_node != None and last_node.position[2] != node.position[2]:
				same_z = False
			last_node = node

		if same_z == False:
			self.points = np.array([node.position for _, node in nodes.items()])
			self.is_3d = True
		else:
			self.points = np.array([(node.position[0], node.position[1]) for _, node in nodes.items()])
			self.is_3d = False
		
		self.id = id

		self.simplify()
	
	def visualize(self):
		if self.is_3d:
			figure = plot.figure()
			axis = figure.add_subplot(projection='3d')

			axis.plot_trisurf(self.points[:,0], self.points[:,1], self.points[:,2], triangles=self.triangles.simplices, cmap=plot.cm.Spectral)
			plot.show()
		else:
			plot.triplot(self.points[:,0], self.points[:,1], self.triangles.simplices)
			plot.show()

		# plot actual neighbors
		figure = plot.figure()
		axis = figure.add_subplot()

		for line in self.lines():
			axis.plot(
				[line[0][0], line[1][0]],
				[line[0][1], line[1][1]]
			)
		
		plot.show()
	
	def lines(self):
		found = {}
		for _, node in self.nodes.items():
			for neighbor in node.neighbors:
				if (node, neighbor) not in found and (neighbor, node) not in found:
					yield (node.position, neighbor.position)
					found[(node, neighbor)] = True
		
	def simplify(self):
		self.triangles = Delaunay(self.points)

		# figure out what neighbors we can simplify
		# algorithm: basically create a new neighbors list. if a node was previously in the neighbor list, then add it back into it
		for simplex in self.triangles.simplices:
			for index in range(len(simplex)):
				node = self.nodes[simplex[index]]

				for index2 in range(len(simplex)):
					neighbor = self.nodes[simplex[index2]]

					if node != neighbor:
						node.add_simple_neighbor(neighbor)
						neighbor.add_simple_neighbor(node)
		
		old_neighbor_count = 0
		new_neighbor_count = 0
		for index in self.nodes:
			old_neighbor_count = old_neighbor_count + len(self.nodes[index].neighbors)
			self.nodes[index].set_simple_neighbors()
			new_neighbor_count = new_neighbor_count + len(self.nodes[index].neighbors)
		
		print(f"{old_neighbor_count} links simplified to {new_neighbor_count} links")