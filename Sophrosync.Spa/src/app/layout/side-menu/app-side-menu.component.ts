import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-side-menu',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './app-side-menu.component.html',
  styleUrl: './app-side-menu.component.scss',
})
export class AppSideMenuComponent {
  readonly navItems = [
    { label: 'Dashboard', path: '/dashboard' },
    { label: 'Clients',   path: '/clients' },
    { label: 'Schedule',  path: '/schedule' },
    { label: 'Notes',     path: '/notes' },
  ];
}
