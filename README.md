# FileHub
A fault tolerant, load balanced file sharing service.

# Brainstorming
## Technical
* Blazor webassembly
* C# backend (dockerized)
* Loadbalancing: nginx
* MSSQL
* Entity Framework
* If one service crashes, all requests are executed again on the second service

## Use cases
* File upload
    * A user can upload one or multiple files which are stored in one folder
    * He can choose if other user are allowed to drop files to his folders
    * He gets an authentication code that he can share with other users
* File download
    * If a user enters a valid authentication code he gets access to the file folder
    * He always can download the stored files
    * Depending on the folder settings he has the possibility to drop files to the folder

# Architecture  
## System  
![system-arch.png](./sys-arch.png)  

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
