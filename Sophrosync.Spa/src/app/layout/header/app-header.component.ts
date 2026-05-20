import { ChangeDetectionStrategy, Component, inject, output } from '@angular/core';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-header',
  imports: [],
  templateUrl: './app-header.component.html',
  styleUrl: './app-header.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppHeaderComponent {
  readonly menuToggle = output<void>();

  private readonly auth = inject(AuthService);
  protected readonly profile = this.auth.userProfile;

  get initials(): string {
    const p = this.profile();
    if (!p) return 'U';
    const f = p.firstName?.[0] ?? '';
    const l = p.lastName?.[0] ?? '';
    return (f + l).toUpperCase() || p.username?.[0]?.toUpperCase() || 'U';
  }

  get displayName(): string {
    const p = this.profile();
    if (!p) return 'User';
    return p.firstName ? `${p.firstName} ${p.lastName}`.trim() : p.username;
  }
}
