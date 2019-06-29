import { Injectable, EventEmitter, Output } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ToolbarEventsService {
  constructor() { }

  @Output() backClicked = new EventEmitter<boolean>();
  @Output() navigate = new EventEmitter<string>();
  @Output() refreshClicked = new EventEmitter<boolean>();

  triggerBackClicked() {
    this.backClicked.emit(true);
  }

  triggerRefreshClicked() {
    this.refreshClicked.emit(true);
  }

  triggerNavigate(url) {
    this.navigate.emit(url);
  }
}
