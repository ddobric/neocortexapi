import { WebsocketServer } from './websocket-server';

let app = new WebsocketServer().getApp();
export { app };