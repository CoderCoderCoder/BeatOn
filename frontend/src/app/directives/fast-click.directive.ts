import { Directive, ElementRef } from '@angular/core';
import { createOfflineCompileUrlResolver } from '@angular/compiler';

@Directive({
    selector: '[fast-click]',
})
export class FastClickDirective {
    element: HTMLElement;
    wasClicked: boolean = false;
    constructor(private elRef: ElementRef) {
        this.element = elRef.nativeElement;
        this.element.addEventListener('click', this.onClick.bind(this));
        this.element.addEventListener('touchstart', this.onTouchStart.bind(this));
    }
    onClick() {
        this.wasClicked = true;
    }

    onTouchStart(e) {
        e.stopPropagation();
        this.wasClicked = false;
        setTimeout(() => {
            if (!this.wasClicked) {
                this.element.click();
                this.wasClicked = true;
            }
        }, 300);
    }
}
