import { ChangeDetectionStrategy, Component, inject, output } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

interface NavItem {
  label: string;
  path: string;
  icon: string;
}

@Component({
  selector: 'app-side-menu',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './app-side-menu.component.html',
  styleUrl: './app-side-menu.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppSideMenuComponent {
  readonly menuToggle = output<void>();

  private readonly auth = inject(AuthService);

  readonly primaryNav: NavItem[] = [
    { label: 'Dashboard', path: '/dashboard',  icon: 'dashboard' },
    { label: 'Clients',   path: '/clients',    icon: 'group' },
    { label: 'Calendar',  path: '/calendar',   icon: 'calendar_today' },
    { label: 'Notes',     path: '/notes',      icon: 'edit_note' },
    { label: 'Settings',  path: '/settings',   icon: 'settings' },
  ];

  readonly bottomNav: NavItem[] = [];

  logout(): void {
    this.auth.logout();
  }
}
