"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var websocket_server_1 = require("./websocket-server");
var app = new websocket_server_1.WebsocketServer().getApp();
exports.app = app;
