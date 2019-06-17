import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import {MatButtonModule, MatCardModule, MatIconModule, MatToolbarModule, MatDialogModule, MatProgressSpinnerModule} from '@angular/material';
import { HttpClientModule } from '@angular/common/http';
import { SetupComponent } from './setup/setup.component';
import { SetupStep1Component } from './setup-step1/setup-step1.component';
import { ProgressSpinnerDialogComponent } from './progress-spinner-dialog/progress-spinner-dialog.component';
import { ScrollingModule } from '@angular/cdk/scrolling';
import { SetupStep2Component } from './setup-step2/setup-step2.component';
import { SetupStep3Component } from './setup-step3/setup-step3.component';
import { MainComponent } from './main/main.component';
import { MainPlaylistsComponent } from './main-playlists/main-playlists.component';
import { MainBrowserComponent } from './main-browser/main-browser.component';
import {MatTabsModule} from '@angular/material/tabs';



@NgModule({
  declarations: [
    AppComponent,
    SetupComponent,
    SetupStep1Component,
    ProgressSpinnerDialogComponent,
    SetupStep2Component,
    SetupStep3Component,
    MainComponent,
    MainPlaylistsComponent,
    MainBrowserComponent
  ],
  entryComponents: [ProgressSpinnerDialogComponent],
  imports: [
    BrowserModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatToolbarModule,
    MatDialogModule,
    MatProgressSpinnerModule,
    HttpClientModule,
    ScrollingModule,
    MatTabsModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
