"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var http_1 = require("http");
var express = require("express");
var socketIo = require("socket.io");
var WebsocketServer = /** @class */ (function () {
    function WebsocketServer() {
        this.createApp();
        this.config();
        this.createServer();
        this.sockets();
        this.listen();
    }
    WebsocketServer.prototype.createApp = function () {
        this.app = express();
    };
    WebsocketServer.prototype.createServer = function () {
        this.server = http_1.createServer(this.app);
    };
    WebsocketServer.prototype.config = function () {
        this.port = process.env.PORT || WebsocketServer.PORT;
    };
    WebsocketServer.prototype.sockets = function () {
        this.io = socketIo(this.server);
    };
    WebsocketServer.prototype.listen = function () {
        var _this = this;
        this.server.listen(this.port, function () {
            console.log('Running server on port %s', _this.port);
        });
        this.io.on('connect', function (socket) {
            console.log('Connected client on port %s.', _this.port);
            socket.on('disconnect', function () {
                console.log('Client disconnected');
            });
        });
    };
    WebsocketServer.prototype.getApp = function () {
        return this.app;
    };
    WebsocketServer.PORT = 8080;
    return WebsocketServer;
}());
exports.WebsocketServer = WebsocketServer;
