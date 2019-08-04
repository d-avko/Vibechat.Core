"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
Object.defineProperty(exports, "__esModule", { value: true });
var ChatMessage_1 = require("../Data/Message");
var MessageState_1 = require("../Shared/MessageState");
var ConversationsService = /** @class */ (function () {
    function ConversationsService(dateParser, authService, requestsBuilder, connectionManager) {
        this.dateParser = dateParser;
        this.authService = authService;
        this.requestsBuilder = requestsBuilder;
        this.connectionManager = connectionManager;
        this.connectionManager.setConversationsService(this);
    }
    ConversationsService.prototype.IsConversationSelected = function () {
        return this.CurrentConversation != null;
    };
    ConversationsService.prototype.GetConversationsIds = function () {
        return this.Conversations.map(function (x) { return x.id; });
    };
    ConversationsService.prototype.ChangeConversation = function (conversation) {
        return __awaiter(this, void 0, void 0, function () {
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        if (conversation == this.CurrentConversation) {
                            this.CurrentConversation = null;
                            return [2 /*return*/];
                        }
                        this.CurrentConversation = conversation;
                        return [4 /*yield*/, this.UpdateExisting(conversation)];
                    case 1:
                        _a.sent();
                        return [2 /*return*/];
                }
            });
        });
    };
    ConversationsService.prototype.FindGroupsByName = function (name) {
        return __awaiter(this, void 0, void 0, function () {
            var result;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0: return [4 /*yield*/, this.requestsBuilder.SearchForGroups(name)];
                    case 1:
                        result = _a.sent();
                        if (!result.isSuccessfull) {
                            return [2 /*return*/, null];
                        }
                        if (result.response == null) {
                            return [2 /*return*/, new Array()];
                        }
                        else {
                            return [2 /*return*/, result.response.slice()];
                        }
                        return [2 /*return*/];
                }
            });
        });
    };
    ConversationsService.prototype.GetMessagesForConversation = function (count) {
        return __awaiter(this, void 0, void 0, function () {
            var result;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0: return [4 /*yield*/, this.requestsBuilder.GetConversationMessages(this.CurrentConversation.messages.length, count, this.CurrentConversation.id)];
                    case 1:
                        result = _a.sent();
                        if (!result.isSuccessfull) {
                            return [2 /*return*/];
                        }
                        //server sent zero messages, we reached end of our history.
                        if (result.response == null || result.response.length == 0) {
                            return [2 /*return*/, result.response];
                        }
                        result.response = result.response.sort(this.MessagesSortFunc);
                        this.dateParser.ParseStringDatesInMessages(result.response);
                        //append old messages to new ones.
                        this.CurrentConversation.messages = result.response.concat(this.CurrentConversation.messages).slice();
                        return [2 /*return*/];
                }
            });
        });
    };
    ConversationsService.prototype.MessagesSortFunc = function (left, right) {
        if (left.timeReceived < right.timeReceived)
            return -1;
        if (left.timeReceived > right.timeReceived)
            return 1;
        return 0;
    };
    ConversationsService.prototype.DeleteMessages = function (messages) {
        return __awaiter(this, void 0, void 0, function () {
            var currentConversationId, notLocalMessages, response, conversation;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        currentConversationId = this.CurrentConversation.id;
                        notLocalMessages = messages.filter(function (x) { return x.state != MessageState_1.MessageState.Pending; });
                        //delete local unsent messages
                        this.CurrentConversation.messages = this.CurrentConversation.messages
                            .filter(function (msg) { return notLocalMessages.findIndex(function (selected) { return selected.id == msg.id; }) == -1; });
                        if (notLocalMessages.length == 0) {
                            return [2 /*return*/];
                        }
                        return [4 /*yield*/, this.requestsBuilder.DeleteMessages(notLocalMessages, this.CurrentConversation.id)];
                    case 1:
                        response = _a.sent();
                        if (!response.isSuccessfull) {
                            return [2 /*return*/];
                        }
                        conversation = this.Conversations.find(function (x) { return x.id == currentConversationId; });
                        conversation.messages = conversation
                            .messages
                            .filter(function (msg) { return messages.findIndex(function (selected) { return selected.id == msg.id; }) == -1; });
                        messages.splice(0, messages.length);
                        return [2 /*return*/];
                }
            });
        });
    };
    ConversationsService.prototype.UpdateConversations = function () {
        return __awaiter(this, void 0, void 0, function () {
            var response;
            var _this = this;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0: return [4 /*yield*/, this.requestsBuilder.UpdateConversationsRequest()];
                    case 1:
                        response = _a.sent();
                        if (!response.isSuccessfull) {
                            return [2 /*return*/];
                        }
                        if (response.response != null) {
                            //parse string date to js Date
                            response.response
                                .forEach(function (conversation) {
                                if (conversation.messages != null) {
                                    _this.dateParser.ParseStringDatesInMessages(conversation.messages);
                                }
                                else {
                                    conversation.messages = new Array();
                                }
                            });
                            this.Conversations = response.response;
                            this.Conversations.forEach(function (x) {
                                if (!x.isGroup) {
                                    x.isMessagingRestricted = x.dialogueUser.isMessagingRestricted;
                                }
                            });
                        }
                        else {
                            this.Conversations = new Array();
                        }
                        //Initiate signalR group connections
                        this.connectionManager.Start();
                        return [2 /*return*/];
                }
            });
        });
    };
    ConversationsService.prototype.OnMessageReceived = function (data) {
        this.dateParser.ParseStringDateInMessage(data.message);
        var conversation = this.Conversations
            .find(function (x) { return x.id == data.conversationId; });
        conversation.messages = conversation.messages.slice();
        if (data.senderId != this.authService.User.id) {
            ++conversation.messagesUnread;
        }
    };
    ConversationsService.prototype.BuildForwardedMessage = function (whereTo, forwarded) {
        var min = 1;
        var max = 100000;
        var clientMessageId = Math.floor(Math.random() * (+max - +min) + +min);
        return new ChatMessage_1.Message({
            id: clientMessageId,
            isAttachment: false,
            user: this.authService.User,
            conversationID: whereTo,
            state: MessageState_1.MessageState.Pending,
            timeReceived: new Date(),
            forwardedMessage: forwarded
        });
    };
    ConversationsService.prototype.BuildMessage = function (message, whereTo, isAttachment, AttachmentInfo) {
        if (isAttachment === void 0) { isAttachment = false; }
        if (AttachmentInfo === void 0) { AttachmentInfo = null; }
        var min = 1;
        var max = 100000;
        var clientMessageId = Math.floor(Math.random() * (+max - +min) + +min);
        return new ChatMessage_1.Message({
            id: clientMessageId,
            messageContent: message,
            isAttachment: isAttachment,
            attachmentInfo: AttachmentInfo,
            user: this.authService.User,
            conversationID: whereTo,
            state: MessageState_1.MessageState.Pending,
            timeReceived: new Date()
        });
    };
    ConversationsService.prototype.BanFromConversation = function (userToBan) {
        return __awaiter(this, void 0, void 0, function () {
            var result;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0: return [4 /*yield*/, this.requestsBuilder.BanFromConversation(userToBan.id, this.CurrentConversation.id)];
                    case 1:
                        result = _a.sent();
                        if (!result.isSuccessfull) {
                            return [2 /*return*/];
                        }
                        userToBan.isBlockedInConversation = true;
                        return [2 /*return*/];
                }
            });
        });
    };
    ConversationsService.prototype.UnbanFromConversation = function (userToUnban) {
        return __awaiter(this, void 0, void 0, function () {
            var result;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0: return [4 /*yield*/, this.requestsBuilder.UnBanFromConversation(userToUnban.id, this.CurrentConversation.id)];
                    case 1:
                        result = _a.sent();
                        if (!result.isSuccessfull) {
                            return [2 /*return*/];
                        }
                        userToUnban.isBlockedInConversation = false;
                        return [2 /*return*/];
                }
            });
        });
    };
    ConversationsService.prototype.RemoveGroup = function (group) {
        this.connectionManager.RemoveConversation(group);
    };
    ConversationsService.prototype.GetAttachmentsFor = function (groupId, attachmentKind, offset, count) {
        return __awaiter(this, void 0, void 0, function () {
            var result;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0: return [4 /*yield*/, this.requestsBuilder.GetAttachmentsForConversation(groupId, attachmentKind, offset, count)];
                    case 1:
                        result = _a.sent();
                        if (!result.isSuccessfull) {
                            return [2 /*return*/, null];
                        }
                        if (result.response == null || result.response.length == 0) {
                            return [2 /*return*/, result.response];
                        }
                        this.dateParser.ParseStringDatesInMessages(result.response);
                        return [2 /*return*/];
                }
            });
        });
    };
    ConversationsService.prototype.SendMessage = function (message) {
        var messageToSend = this.BuildMessage(message, this.CurrentConversation.id);
        this.CurrentConversation.messages.push(messageToSend);
        this.connectionManager.SendMessage(messageToSend, this.CurrentConversation);
    };
    ConversationsService.prototype.ReadMessage = function (message) {
        if (message.user.id == this.authService.User.id) {
            return;
        }
        if (this.PendingReadMessages.find(function (x) { return x == message.id; })) {
            return;
        }
        this.PendingReadMessages.push(message.id);
        this.connectionManager.ReadMessage(message.id, message.id);
    };
    ConversationsService.prototype.CreateGroup = function (name, isPublic) {
        return __awaiter(this, void 0, void 0, function () {
            var result;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0: return [4 /*yield*/, this.requestsBuilder.CreateConversation(name, this.authService.User.id, null, null, true, isPublic)];
                    case 1:
                        result = _a.sent();
                        if (!result.isSuccessfull) {
                            return [2 /*return*/];
                        }
                        this.connectionManager.InitiateConnections(new Array(1).fill(result.response.id));
                        result.response.messages = new Array();
                        this.Conversations = this.Conversations.concat([result.response]);
                        return [2 /*return*/];
                }
            });
        });
    };
    ConversationsService.prototype.CreateDialogWith = function (user) {
        this.connectionManager.CreateDialog(user);
    };
    ConversationsService.prototype.RemoveDialogWith = function (user) {
        this.connectionManager.RemoveConversation(this.FindDialogWith(user));
        this.CurrentConversation = null;
    };
    ConversationsService.prototype.JoinGroup = function (group) {
        this.connectionManager.AddUserToConversation(this.authService.User.id, group);
    };
    ConversationsService.prototype.KickUser = function (user) {
        this.connectionManager.RemoveUserFromConversation(user.id, this.CurrentConversation.id, false);
    };
    ConversationsService.prototype.ExistsIn = function (id) {
        return this.Conversations.find(function (x) { return x.id == id; }) != null;
    };
    ConversationsService.prototype.FindDialogWith = function (user) {
        return this.Conversations.find(function (x) { return !x.isGroup && x.dialogueUser.id == user.id; });
    };
    ConversationsService.prototype.UpdateExisting = function (target) {
        return __awaiter(this, void 0, void 0, function () {
            var result;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0: return [4 /*yield*/, this.requestsBuilder.GetConversationById(target.id)];
                    case 1:
                        result = _a.sent();
                        if (!result.isSuccessfull) {
                            return [2 /*return*/];
                        }
                        this.UpdateConversationFields(target, result.response);
                        return [2 /*return*/];
                }
            });
        });
    };
    ConversationsService.prototype.ForwardMessagesTo = function (destination, messages) {
        var _this = this;
        if (destination == null || destination.length == 0) {
            return;
        }
        destination.forEach(function (conversation) {
            messages.forEach(function (msg) {
                var messageToSend = _this.BuildForwardedMessage(conversation.id, msg.forwardedMessage ? msg.forwardedMessage : msg);
                _this.connectionManager.SendMessage(messageToSend, conversation);
                conversation.messages.push(messageToSend);
            });
        });
        messages.splice(0, messages.length);
    };
    ConversationsService.prototype.RemoveAllMessages = function (group) {
        return __awaiter(this, void 0, void 0, function () {
            var response;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0: return [4 /*yield*/, this.requestsBuilder.DeleteMessages(group.messages, group.id)];
                    case 1:
                        response = _a.sent();
                        if (!response.isSuccessfull) {
                            return [2 /*return*/];
                        }
                        //delete messages locally
                        if (group.messages.length != 0) {
                            group.messages.splice(0, group.messages.length);
                        }
                        this.CurrentConversation = null;
                        return [2 /*return*/];
                }
            });
        });
    };
    ConversationsService.prototype.Leave = function (from) {
        this.connectionManager.RemoveUserFromConversation(this.authService.User.id, from.id, true);
        this.CurrentConversation = null;
    };
    ConversationsService.prototype.ChangeThumbnail = function (file) {
        return __awaiter(this, void 0, void 0, function () {
            var currentConversationId, result, conversation;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        currentConversationId = this.CurrentConversation.id;
                        return [4 /*yield*/, this.requestsBuilder.UploadConversationThumbnail(file, this.CurrentConversation.id)];
                    case 1:
                        result = _a.sent();
                        if (!result.isSuccessfull) {
                            return [2 /*return*/];
                        }
                        conversation = this.Conversations.find(function (x) { return x.id == currentConversationId; });
                        conversation.thumbnailUrl = result.response.thumbnailUrl;
                        conversation.fullImageUrl = result.response.fullImageUrl;
                        return [2 /*return*/];
                }
            });
        });
    };
    ConversationsService.prototype.ChangeConversationName = function (name) {
        return __awaiter(this, void 0, void 0, function () {
            var currentConversationId, result;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        currentConversationId = this.CurrentConversation.id;
                        return [4 /*yield*/, this.requestsBuilder.ChangeConversationName(name, this.CurrentConversation.id)];
                    case 1:
                        result = _a.sent();
                        if (!result.isSuccessfull) {
                            return [2 /*return*/];
                        }
                        this.Conversations.find(function (x) { return x.id == currentConversationId; }).name = name;
                        return [2 /*return*/];
                }
            });
        });
    };
    ConversationsService.prototype.InviteUsersToGroup = function (users, group) {
        var _this = this;
        if (users == null || users.length == 0) {
            return;
        }
        users.forEach(function (value) {
            _this.connectionManager.AddUserToConversation(value.id, group);
        });
        //Now add users locally
        users.forEach(function (user) {
            //sort of sanitization of input
            if (user.id == _this.authService.User.id) {
                return;
            }
            group.participants.push(user);
            group.participants = group.participants.slice();
        });
    };
    ConversationsService.prototype.OnAddedToGroup = function (data) {
        return __awaiter(this, void 0, void 0, function () {
            var conversation;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        if (data.conversation.messages != null) {
                            this.dateParser.ParseStringDatesInMessages(data.conversation.messages);
                        }
                        if (data.conversation.isGroup) {
                            //we created new group.
                            if (data.user.id == this.authService.User.id) {
                                if (data.conversation.messages == null) {
                                    data.conversation.messages = new Array();
                                }
                                this.Conversations = this.Conversations.concat([data.conversation]);
                            }
                            else {
                                conversation = this.Conversations.find(function (x) { return x.id == data.conversation.id; });
                                conversation.participants.push(data.user);
                                conversation.participants = conversation.participants.slice();
                            }
                        }
                        else {
                            //we created dialog with someone;
                            if (data.user.id == this.authService.User.id) {
                                if (data.conversation.messages == null) {
                                    data.conversation.messages = new Array();
                                }
                                this.Conversations = this.Conversations.concat([data.conversation]);
                            }
                            else {
                                //someone created dialog with us.
                                data.conversation.dialogueUser = data.user;
                                this.Conversations = this.Conversations.concat([data.conversation]);
                            }
                        }
                        //update data about this conversation.
                        return [4 /*yield*/, this.UpdateExisting(data.conversation)];
                    case 1:
                        //update data about this conversation.
                        _a.sent();
                        return [2 /*return*/];
                }
            });
        });
    };
    ConversationsService.prototype.OnRemovedFromGroup = function (data) {
        //either this client left or creator removed him.
        if (data.userId == this.authService.User.id) {
            this.Conversations.splice(this.Conversations.findIndex(function (x) { return x.id == data.conversationId; }), 1);
        }
        else {
            var participants = this.Conversations.find(function (x) { return x.id == data.conversationId; }).participants;
            participants.splice(participants.findIndex(function (x) { return x.id == data.userId; }), 1);
        }
    };
    ConversationsService.prototype.OnMessageRead = function (msgId, conversationId) {
        var conversation = this.Conversations.find(function (x) { return x.id == conversationId; });
        if (conversation == null) {
            return;
        }
        var message = conversation.messages.find(function (x) { return x.id == msgId; });
        if (message == null) {
            return;
        }
        if (message.state == MessageState_1.MessageState.Read) {
            return;
        }
        message.state = MessageState_1.MessageState.Read;
        conversation.messages = conversation.messages.slice();
        if (message.user.id != this.authService.User.id) {
            --conversation.messagesUnread;
        }
        var pendingIndex = this.PendingReadMessages.findIndex(function (x) { return x == message.id; });
        if (pendingIndex != -1) {
            this.PendingReadMessages.splice(pendingIndex, 1);
        }
    };
    ConversationsService.prototype.OnMessageDelivered = function (msgId, clientMessageId, conversationId) {
        var conversation = this.Conversations.find(function (x) { return x.id == conversationId; });
        if (conversation == null) {
            return;
        }
        var message = conversation.messages.find(function (x) { return x.id == clientMessageId; });
        if (message == null) {
            return;
        }
        message.id = msgId;
        message.state = MessageState_1.MessageState.Delivered;
        conversation.messages = conversation.messages.slice();
    };
    ConversationsService.prototype.UploadImages = function (files) {
        return __awaiter(this, void 0, void 0, function () {
            var conversationToSend, result, response;
            var _this = this;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        if (files.length == 0) {
                            return [2 /*return*/];
                        }
                        conversationToSend = this.CurrentConversation.id;
                        return [4 /*yield*/, this.requestsBuilder.UploadImages(files)];
                    case 1:
                        result = _a.sent();
                        response = result.body;
                        if (!response.isSuccessfull) {
                            return [2 /*return*/];
                        }
                        response.response.uploadedFiles.forEach(function (file) {
                            var message = _this.BuildMessage(null, conversationToSend, true, file);
                            _this.connectionManager.SendMessage(message, _this.CurrentConversation);
                            _this.CurrentConversation.messages.push(message);
                        });
                        return [2 /*return*/];
                }
            });
        });
    };
    ConversationsService.prototype.UpdateConversationFields = function (old, New) {
        old.isMessagingRestricted = New.isMessagingRestricted;
        old.name = New.name;
        old.fullImageUrl = New.fullImageUrl;
        old.thumbnailUrl = New.thumbnailUrl;
    };
    return ConversationsService;
}());
exports.ConversationsService = ConversationsService;
//# sourceMappingURL=ConversationsService.js.map
