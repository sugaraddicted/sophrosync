import { Component, signal } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';

interface NavItem {
  label: string;
  icon: string;
  route: string;
}

@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.scss',
})
export class ShellComponent {
  readonly navItems: NavItem[] = [
    { label: 'Dashboard', icon: 'dashboard',       route: '/dashboard' },
    { label: 'Clients',   icon: 'group',            route: '/clients'   },
    { label: 'Calendar',  icon: 'calendar_today',   route: '/calendar'  },
    { label: 'Notes',     icon: 'edit_note',        route: '/notes'     },
    { label: 'Settings',  icon: 'settings',         route: '/settings'  },
  ];
}
