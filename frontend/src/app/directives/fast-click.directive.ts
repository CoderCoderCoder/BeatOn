import { Directive, ElementRef } from '@angular/core';
import { createOfflineCompileUrlResolver } from '@angular/compiler';

@Directive({
  selector: '[fast-click]'
})
export class FastClickDirective {

  element : HTMLElement;
  wasClicked: boolean = false;
  constructor(private elRef: ElementRef) { 
    console.log("fast-click loaded");
    this.element = elRef.nativeElement;
    this.element.addEventListener('click', this.onClick.bind(this));
    this.element.addEventListener('touchstart', this.onTouchStart.bind(this));
  }
  onClick() {
    console.log("fastclick: onclick");
     this.wasClicked = true;
  }

  onTouchStart(e) {
    console.log("fastclick: onTouchStart")
    this.wasClicked = false;
    setTimeout(() => {
      if (!this.wasClicked) {
        console.log("fastclick: triggering click");
        console.log(this.element);
        this.element.click();
        this.wasClicked = true;
      } else
      {
        console.log("fastclick: it was already clicked");
      }
    }, 300);
  }

}
