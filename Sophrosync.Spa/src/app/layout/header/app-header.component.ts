import {
  ChangeDetectionStrategy,
  Component,
  OnDestroy,
  inject,
  output,
  signal,
} from '@angular/core';
import { AuthService } from '../../core/auth/auth.service';
import { NotificationsService } from '../../features/notifications/notifications.service';
import { NotificationPanelComponent } from './notification-panel/notification-panel.component';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [NotificationPanelComponent],
  templateUrl: './app-header.component.html',
  styleUrl: './app-header.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppHeaderComponent implements OnDestroy {
  readonly menuToggle = output<void>();

  private readonly auth = inject(AuthService);
  private readonly notificationsSvc = inject(NotificationsService);

  protected readonly profile = this.auth.userProfile;
  protected readonly unreadCount = signal(0);
  protected readonly isPanelOpen = signal(false);

  private pollTimer?: ReturnType<typeof setInterval>;

  constructor() {
    this.loadUnreadCount();
    this.pollTimer = setInterval(() => this.loadUnreadCount(), 30_000);
  }

  ngOnDestroy(): void {
    clearInterval(this.pollTimer);
  }

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

  protected loadUnreadCount(): void {
    this.notificationsSvc.getUnreadCount().subscribe({
      next: (count) => this.unreadCount.set(count),
      error: () => {/* silently ignore — badge simply stays at last known value */},
    });
  }

  protected togglePanel(): void {
    this.isPanelOpen.update((open) => !open);
  }

  protected closePanel(): void {
    this.isPanelOpen.set(false);
  }
}
