import {
  Component,
  ChangeDetectionStrategy,
  NgZone,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { AppointmentsService, AppointmentDto } from '../appointments.service';

@Component({
  selector: 'app-next-session-card',
  standalone: true,
  imports: [],
  templateUrl: './next-session-card.component.html',
  styleUrl: './next-session-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NextSessionCardComponent implements OnInit {
  private readonly appointmentsService = inject(AppointmentsService);
  private readonly ngZone = inject(NgZone);

  readonly nextSession = signal<AppointmentDto | null>(null);
  readonly loading = signal(true);
  readonly error = signal(false);

  readonly formattedDate = computed<string>(() => {
    const session = this.nextSession();
    if (!session) return '';

    const scheduledAt = new Date(session.scheduledAt);
    const now = new Date();

    const sessionDay = new Date(
      Date.UTC(
        scheduledAt.getUTCFullYear(),
        scheduledAt.getUTCMonth(),
        scheduledAt.getUTCDate()
      )
    );
    const todayDay = new Date(
      Date.UTC(now.getUTCFullYear(), now.getUTCMonth(), now.getUTCDate())
    );
    const tomorrowDay = new Date(todayDay);
    tomorrowDay.setUTCDate(tomorrowDay.getUTCDate() + 1);

    const hours = String(scheduledAt.getUTCHours()).padStart(2, '0');
    const minutes = String(scheduledAt.getUTCMinutes()).padStart(2, '0');
    const timeStr = `${hours}:${minutes}`;

    if (sessionDay.getTime() === todayDay.getTime()) {
      return `Today, ${timeStr}`;
    }
    if (sessionDay.getTime() === tomorrowDay.getTime()) {
      return `Tomorrow, ${timeStr}`;
    }

    const dayName = scheduledAt.toLocaleDateString('en-GB', {
      weekday: 'long',
      timeZone: 'UTC',
    });
    const day = String(scheduledAt.getUTCDate()).padStart(2, '0');
    const month = scheduledAt.toLocaleDateString('en-GB', {
      month: 'short',
      timeZone: 'UTC',
    });
    const year = scheduledAt.getUTCFullYear();

    return `${dayName}, ${day} ${month} ${year} — ${timeStr}`;
  });

  readonly typeLabel = computed<string>(() => {
    const session = this.nextSession();
    if (!session) return '';
    switch (session.type) {
      case 'InPerson': return 'In Person';
      case 'Video':    return 'Video Call';
      case 'Phone':    return 'Phone Call';
      default:         return session.type;
    }
  });

  ngOnInit(): void {
    const now = new Date();
    const thirtyDaysOut = new Date(now);
    thirtyDaysOut.setDate(thirtyDaysOut.getDate() + 30);

    this.ngZone.run(async () => {
      try {
        const dtos = await this.appointmentsService.getByDateRange(now, thirtyDaysOut);
        const upcoming = dtos
          .filter(d => d.status === 'Scheduled' || d.status === 'Confirmed')
          .sort((a, b) => new Date(a.scheduledAt).getTime() - new Date(b.scheduledAt).getTime());
        this.nextSession.set(upcoming[0] ?? null);
      } catch {
        this.error.set(true);
      } finally {
        this.loading.set(false);
      }
    });
  }
}
