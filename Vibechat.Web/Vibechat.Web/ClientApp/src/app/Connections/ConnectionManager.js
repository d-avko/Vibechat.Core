"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var signalR = require("@aspnet/signalr");
var Cache_1 = require("../Auth/Cache");
var ConnectionManager = /** @class */ (function () {
    function ConnectionManager() {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl("/hubs/chat", { accessTokenFactory: function () { return Cache_1.AuthService.JwtToken; } })
            .build();
    }
    ConnectionManager.prototype.Start = function (OnConnectingNotify, OnMessageReceived, OnAddedToGroup, OnRemovedFromGroup, OnDisconnectedNotify) {
        var _this = this;
        this.connection.start();
        this.OnMessageReceived = OnMessageReceived;
        this.OnAddedToGroup = OnAddedToGroup;
        this.OnRemovedFromGroup = OnRemovedFromGroup;
        this.OnDisconnectedNotify = OnDisconnectedNotify;
        this.OnConnectingNotify = OnConnectingNotify;
        this.connection.onclose(function () { return _this.OnDisconnected(); });
        this.connection.on("ReceiveMessage", function (senderId, message, conversationId) {
            OnMessageReceived(senderId, message, conversationId);
        });
        this.connection.on("AddedToGroup", function (conversationId, userId) {
            OnAddedToGroup(conversationId, userId);
        });
    };
    ConnectionManager.prototype.OnConnected = function () {
        this.connection.send("OnConnected");
    };
    ConnectionManager.prototype.OnDisconnected = function () {
        this.OnDisconnectedNotify();
        this.connection.send("OnDisconnected");
        this.Start(this.OnConnectingNotify, this.OnMessageReceived, this.OnAddedToGroup, this.OnRemovedFromGroup, this.OnDisconnectedNotify);
    };
    ConnectionManager.prototype.SendMessage = function (message, conversation, whoSentId) {
        if (conversation.isGroup) {
            this.connection.send("SendMessageToGroup", message, whoSentId, conversation.conversationID);
        }
        else {
            this.connection.send("SendMessageToUser", message, whoSentId, conversation.dialogueUser.id, conversation.conversationID);
        }
    };
    return ConnectionManager;
}());
exports.ConnectionManager = ConnectionManager;
//# sourceMappingURL=ConnectionManager.js.map