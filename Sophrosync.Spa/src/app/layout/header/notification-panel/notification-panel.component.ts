import {
  ChangeDetectionStrategy,
  Component,
  OnDestroy,
  inject,
  output,
  signal,
} from '@angular/core';
import { NotificationsService } from '../../../features/notifications/notifications.service';
import { NotificationDto } from '../../../features/notifications/notification.model';

@Component({
  selector: 'app-notification-panel',
  standalone: true,
  imports: [],
  templateUrl: './notification-panel.component.html',
  styleUrl: './notification-panel.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NotificationPanelComponent implements OnDestroy {
  private readonly svc = inject(NotificationsService);

  protected readonly notifications = signal<NotificationDto[]>([]);
  protected readonly isLoading = signal(false);
  readonly closed = output<void>();

  private refreshTimer?: ReturnType<typeof setInterval>;

  constructor() {
    this.load();
    this.refreshTimer = setInterval(() => this.load(), 30_000);
  }

  ngOnDestroy(): void {
    clearInterval(this.refreshTimer);
  }

  protected load(): void {
    this.isLoading.set(true);
    this.svc.getInbox().subscribe({
      next: (items) => {
        this.notifications.set(items);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false),
    });
  }

  protected dismiss(id: string): void {
    this.svc.dismiss(id).subscribe({
      next: () => this.notifications.update((list) => list.filter((n) => n.id !== id)),
    });
  }

  protected relativeTime(iso: string): string {
    const diff = Date.now() - new Date(iso).getTime();
    const min = Math.floor(diff / 60_000);
    if (min < 1) return 'just now';
    if (min < 60) return `${min}m ago`;
    const hr = Math.floor(min / 60);
    if (hr < 24) return `${hr}h ago`;
    return `${Math.floor(hr / 24)}d ago`;
  }
}
