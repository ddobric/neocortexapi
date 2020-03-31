import { Injectable } from '@angular/core';
import { Observable, Subject, Observer } from "rxjs";
import { WebSocketSubject, webSocket } from "rxjs/webSocket";
import { filter, map, switchMap, retryWhen, delay } from 'rxjs/operators';
import { environment as env } from '../../environments/environment.prod';



@Injectable({
  providedIn: 'root'
})
export class WebsocketService {
  private subject: Subject<MessageEvent>;
  connection$: WebSocketSubject<any>;
  RETRY_SECONDS = 10;
  store: any;

  constructor() {

  }



  public connect(url): Subject<MessageEvent> {
    if (!this.subject) {
      this.subject = this.create(url);
      console.log("Successfully connected: " + url);

    }
    return this.subject;
  }

  private create(url): Subject<MessageEvent> {
    let ws = new WebSocket(url);

    let observable = Observable.create((obs: Observer<MessageEvent>) => {
      ws.onmessage = obs.next.bind(obs);
      ws.onerror = obs.error.bind(obs);
      ws.onclose = obs.complete.bind(obs);
      return ws.close.bind(ws);
    });


    let observer = {
      next: (data: Object) => {
        if (ws.readyState === WebSocket.OPEN) {
          ws.send(JSON.stringify(data));
        }
      }
    };
    return Subject.create(observer, observable);
  }


}
