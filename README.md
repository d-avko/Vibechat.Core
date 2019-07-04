# Vibechat.Web
ASP.NET Core Messenger: features Groups, dialogs, end-to-end encrypted chats. 
Uses mobile phone login system (firebase is used as SMS provider) and tokens based authorization.


![Showcase](https://i.imgur.com/qsJnwc5.png)


Messaging             |  User profiles
:-------------------------:|:-------------------------:
![1](https://i.imgur.com/cjcfl13.png)  |  ![2](https://i.imgur.com/WSAXiZX.png)
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
5. Publishing.

<code> <strong>Front-end: </strong> </code>
1. Tech stack.
2. Folders overview.

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
### Database 
**EF Core** with **PostgreSQL** provider was used.
### Publishing

1. Publish to folder.
2. Create hosting.json in root folder with contents similar to:
``` json
{
    "certificateSettings": {
      "filename": "certificate_combined.pfx",
      "password": "password"
    }
}
```
3. Send published folder to VM via SSH.
4. Run.

### Front-end

**Framework**: Angular 7

**UI Components**: Angular material.
