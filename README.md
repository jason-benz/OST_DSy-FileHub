# FileHub
A fault tolerant, load balanced file sharing service.

# Contribution information
* Contributors: Adrian Locher & Jason Benz
* School: [OST - Eastern Switzerland University of Applied Sciences](https://www.ost.ch/)
* Module: Distributed Systems (DSy)
* Semester: Spring 2022
* Advisor: Dr. Thomas Bocek

# Overview
The goal of the project is to create a first own distributed system. Since this is a school project, it is only a software draft with limited functionality. For a productive operation, further development and extensive testing would be necessary.\
Required components ([detailed requirements](https://dsl.i.ost.ch/lect/fs22/#challenge-task-fs-2022)):
* Simple Frontend
* Self implemented websocket
* Scalable and fault tolerance service
* Load balancer
* Dockerization

# Use cases
* File upload
    * A user can upload files which are stored in one folder.
    * A user gets a transfer code for his file folder that he can share with other users.
* File download
    * If a user enters a valid transfer code he gets access to the file folder.
    * A user can download the files from the file folder.

# Technical overview
* Software platform: .NET
* Language: C#
* Frontend: Blazor webassembly
* Web socket: System.Net.WebSockets
* Service: Console Application
* Loadbalancing: Caddy
* Persistent storage: File system
* Container solution: Docker

# Architecture  
## System  
![system-arch.png](./sys-arch.png)  
### Technology-Decisions:  
As a frontend technology, we wanted to use a webclient for portability reasons. We chose to use Blazor Webassembly, because we were already familiar with this technology.  
In between the frontend and the backend, we used Caddy as a loadbalancer and as a webserver to deliver our frontend. Caddy seemed like the right way to go, since it was easy to setup and provided automated switching to only running services, if one service would crash.  
The backend is implemented in .NET for simplicitys sake, since our Blazor frontend is also implemented in .NET. Different libraries we're used to create the HTTP and WebSocket endpoints.  
We use a standard filesystem as our storage-backend and mount it into all our service-instances via docker. Since we want users to be able to transfer and store big files this made much more sense than serializing the files into a database.  

## Backend  
![backend-arch.png](./backend-arch.png) 
Our backend is basically a HTTP-Webserver with multiple endpoints.  
- Health-Status: Responds always with HTTP-200, to notify the loadbalancer that the Service is running.  
- File-Info: Responds information about all files of a group (a group of files is stored inside a single directory named after the group)  
- Websocket Handler: Upgrades the HTTP-Connection to a Websocket connection, to send and receive files in binary format.  

The File-Handler component reads and writes files from a common storage backend.  
The Directory-Info module gives information about file-names and -sizes of all files in a directory.  


## Client
![client-arch.png](./client-arch.png)  
![client-runtime.png](./client-runtime.png)  
DotNet Blazor Webassembly is based on a DotNet Runtime implemented in Webassembly.
The GUI Parts of the application are translated into HTML5, while Business logic implemented in C# runs on the runtime
and therefore indirectly on webassembly.

## Load Balancing  
