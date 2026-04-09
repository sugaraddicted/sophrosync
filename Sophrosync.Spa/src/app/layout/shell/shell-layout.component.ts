import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AppHeaderComponent } from '../header/app-header.component';
import { AppSideMenuComponent } from '../side-menu/app-side-menu.component';

@Component({
  selector: 'app-shell-layout',
  imports: [RouterOutlet, AppHeaderComponent, AppSideMenuComponent],
  templateUrl: './shell-layout.component.html',
  styleUrl: './shell-layout.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShellLayoutComponent {
  readonly menuOpen = signal(true);

  toggleMenu(): void {
    this.menuOpen.update(v => !v);
  }
}
