import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AppHeaderComponent } from '../header/app-header.component';
import { AppFooterComponent } from '../footer/app-footer.component';
import { AppSideMenuComponent } from '../side-menu/app-side-menu.component';

@Component({
  selector: 'app-shell-layout',
  standalone: true,
  imports: [RouterOutlet, AppHeaderComponent, AppFooterComponent, AppSideMenuComponent],
  templateUrl: './shell-layout.component.html',
  styleUrl: './shell-layout.component.scss',
})
export class ShellLayoutComponent {}
