import types
import selectors
import socket
import random
from node import Node
from a_star import a_star
from navmesh import Navmesh
import time

class PathfindingServer:
	def __init__(self, port):
		self.socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
		self.socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
		self.socket.bind(("localhost", port))
		self.socket.listen()
		self.socket.setblocking(False)

		print(f"Listening on port {port}")

		self.selector = selectors.DefaultSelector()
		self.selector.register(self.socket, selectors.EVENT_READ, data=None)

		self.outgoing = ""
		self.incoming = ""

		self.nodes = {}
		self.navmesh = {}
		self.tick = 0
		self.last_read = -1

		while True:
			events = self.selector.select(timeout=1)
			for key, mask in events:
				if key.data is None:
					self.accept_connection(key.fileobj)
				else:
					self.handle_client(key.fileobj, key.data, mask)
			
			self.tick = self.tick + 1
		
		self.socket.close()
	
	def accept_connection(self, socket):
		connection, address = socket.accept()
		connection.setblocking(False)

		self.selector.register(connection,
			selectors.EVENT_READ,
			data=types.SimpleNamespace(addr=address, inb=b'', outb=b'')
		)

		print("Client connected")
	
	def handle_client(self, socket, data, mask):
		if mask & selectors.EVENT_READ:
			received = socket.recv(1024)
			if received:
				self.incoming += received.decode("ascii")
				self.last_read = self.tick

				if len(self.incoming) > 0 and self.incoming[-1] == "\n":
					split = self.incoming.split("\n")
					if len(split) > 1:
						for line in split:
							words = line.split(" ")
							if words[0] == "add":
								id = int(words[1])
								position = (float(words[2]), float(words[3]), float(words[4]))
								self.nodes[id] = Node(id, position)
								print(f"Created node {id} at position {position}")
							elif words[0] == "neighbor":
								id1 = int(words[1])
								id2 = int(words[2])
								self.nodes[id1].neighbors.append(self.nodes[id2])
								print(f"Linked node {id1} and {id2}")
							elif words[0] == "simplify":
								self.navmesh = Navmesh(0, self.nodes)
								print(f"Simplified navmesh")
							elif words[0] == "visualize":
								self.navmesh.visualize()
								print(f"Visualizing navmesh")
							elif words[0] == "path":
								path_id = int(words[1])
								start = int(words[2])
								end = int(words[3])

								if start in self.nodes and end in self.nodes:
									starttime = time.time()
									path = a_star(self.nodes[start], self.nodes[end])
									self.outgoing = f"path {path_id}"

									if path != False:
										for node in path:
											self.outgoing = f"{self.outgoing} {node.id}"
										self.outgoing = self.outgoing + "\r\n"
										
										print(f"Completed path in {time.time() - starttime}")
									else:
										self.outgoing = f"error {path_id}"
										print(f"Error completing path (a_star = false)")
								else:
									self.outgoing = f"error {path_id}"
									print(f"Error completing path")

					self.incoming = ""
			else: # kill the server on disconnect
				self.selector.unregister(self.socket)
				self.socket.close()
		
		if self.outgoing != "":
			socket.send(self.outgoing.encode("ascii"))
			self.outgoing = ""
	
	def send(self, send):
		self.outgoing += send