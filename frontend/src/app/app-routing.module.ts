import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { SetupComponent } from './setup/setup.component';
import { SetupStep1Component } from './setup-step1/setup-step1.component';
import { SetupStep2Component } from './setup-step2/setup-step2.component';
import { SetupStep3Component } from './setup-step3/setup-step3.component';
import { MainComponent } from './main/main.component';
import { MainPlaylistsComponent } from './main-playlists/main-playlists.component';
import { MainBrowserComponent } from './main-browser/main-browser.component';
import { ToolsComponent } from './tools/tools.component';

const routes: Routes = [
  { path: 'setup', component: SetupComponent },
  { path: 'setupstep1', component: SetupStep1Component },
  { path: 'setupstep2', component: SetupStep2Component },
  { path: 'setupstep3', component: SetupStep3Component },
  {
    path: 'main',
    component: MainComponent,
    children: [
      {path: 'playlists', component: MainPlaylistsComponent},
      {path: 'browser', component: MainBrowserComponent},
      {path: 'tools', component: ToolsComponent}
    ]
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
