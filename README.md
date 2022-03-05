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
