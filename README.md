# Vibechat - Simple ASP.NET Core messenger
ASP.NET Core Messenger: PWA Asp.Net core & Angular messenger that features Groups, dialogs, end-to-end encrypted chats. 
Uses mobile phone login system (firebase is used as SMS provider) and tokens based authorization.
Running at https://denisavko.me

Messaging             |  User profiles
:-------------------------:|:-------------------------:
![1](https://i.imgur.com/6EFSCQg.png)  |  ![2](https://i.imgur.com/orJXVnU.png)
Side drawer             |  Group profiles
![3](https://i.imgur.com/MldmHIO.png)  |  ![4](https://i.imgur.com/MHthF61.png)
Search             |  Start view
![5](https://i.imgur.com/5Zkt7XU.png)  |  ![6](https://i.imgur.com/3Z8HM0D.png)

## Plan:
<code> <strong>Back-end overview:</strong> </code>
1. Authorization & login system.
2. Messaging.
3. End-to-end encrypted chats.
4. Database.
5. Storage.
6. Publishing.

<code> <strong>Front-end: </strong> </code>
1. Tech stack.

### Authorization & login system
* Login system & registration: to register or log in, user must have a valid phone number. 
If user logs in for the first time, he'll be asked to change his auto-generated username.
* Sms provider: For free SMS ( 10k/mo ) **firebase** was used. See [link](https://firebase.google.com/docs/auth/web/phone-auth) to get started.
* Tokens: After user logs in, he'll be granted a **JWT refresh token** - token with long expiration date, which, in 
turn, needed to get short-term tokens. 
### Messaging
For messaging and push-events **SignalR** was used.
### End-to-end encrypted chats
Standard [Diffie-Hellman](https://en.wikipedia.org/wiki/Diffie%E2%80%93Hellman_key_exchange) scheme was used. 
Public keys are 2048 bits length and pre-generated on server. On secure chat creation, client which creates secure chat sends a request 
to server whereupon server fires push event on this client with created secure chat. **Now the client needs to initiate key exchange.** 
Key exchange could be initiated in 2 ways: immediately(if user in dialog is online), or via subsription system: when second client comes online, 
key exchange will be initiated.
### Storage
For storage, Kestrel with ```PhysicalFileProvider``` was used.

### Database 
**EF Core** with **PostgreSQL** provider was used.
### Publishing
##### Deployment of fileserver
1. ```docker build -t vibechat.fileserver .```
2. ```docker save -o <Output path> vibechat.fileserver```
3. On linux VM: ``` docker load --input <filename>; sudo docker run -d -p 443:443 --mount source=fileserver_volume,target=/app vibechat.fileserver ```.

##### Deployment of database, front-end and back-end:
1. ``` docker-compose build ```
2. Save database image and back-end image, and load them onto your VM, as shown above.
3. ``` docker-compose up ```

### Front-end

**Framework**: Angular 7

**UI Components**: Angular material.
