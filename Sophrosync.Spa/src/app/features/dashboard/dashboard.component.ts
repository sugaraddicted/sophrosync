import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { AuthService } from '../../core/auth/auth.service';
import { AppointmentsCalendarComponent } from './appointments-calendar/appointments-calendar.component';
import { NextSessionCardComponent } from './next-session-card/next-session-card.component';
import { AppointmentsService, AppointmentDto } from './appointments.service';
import { ClientsService, ClientDto } from '../clients/clients.service';

interface WeekDay {
  label: string;
  date: number;
  hasSessions: boolean;
  isToday: boolean;
}

interface UpcomingSession {
  clientId: string;
  initials: string;
  name: string;
  type: string;
  time: string;
  avatarColor: string;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [AppointmentsCalendarComponent, NextSessionCardComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardComponent implements OnInit {
  protected readonly auth = inject(AuthService);
  private readonly appointmentsService = inject(AppointmentsService);
  private readonly clientsService = inject(ClientsService);
  private readonly cdr = inject(ChangeDetectorRef);

  protected readonly profile = this.auth.userProfile;

  protected readonly greeting = computed(() => {
    const name = this.profile()?.firstName;
    return name ? `Good morning, ${name}.` : 'Good morning.';
  });

  protected readonly today = new Date();

  protected readonly formattedDate = this.today.toLocaleDateString('en-US', {
    weekday: 'long', year: 'numeric', month: 'long', day: 'numeric',
  });

  readonly weekDays = signal<WeekDay[]>(this.buildWeekDays());
  readonly upcomingSessions = signal<UpcomingSession[]>([]);
  readonly sessionsLoading = signal(true);
  protected readonly reflectionText = signal('');
  protected readonly weeklyHoursPercent = 68;
  protected readonly monthlyTargetPercent = 52;

  private static readonly AVATAR_COLORS = [
    '#546253', '#6b5b5b', '#5f5f5f', '#4a6b6b', '#5b4a6b', '#6b6b4a',
  ];

  ngOnInit(): void {
    const monday = this.getMondayOfCurrentWeek();
    const sunday = new Date(monday);
    sunday.setDate(monday.getDate() + 6);
    sunday.setHours(23, 59, 59, 999);

    Promise.all([
      this.appointmentsService.getByDateRange(monday, sunday),
      this.clientsService.getAll(),
    ])
      .then(([appts, clients]) => {
        const clientMap = new Map(clients.map(c => [c.id, c]));

        const activeDayStrings = new Set(
          appts
            .filter(a => a.status === 'Scheduled' || a.status === 'Confirmed')
            .map(a => new Date(a.scheduledAt).toDateString())
        );
        this.weekDays.set(this.buildWeekDays(activeDayStrings));

        const now = new Date();
        const upcoming = appts
          .filter(a =>
            (a.status === 'Scheduled' || a.status === 'Confirmed') &&
            new Date(a.scheduledAt) >= now
          )
          .sort((a, b) => a.scheduledAt.localeCompare(b.scheduledAt))
          .slice(0, 5)
          .map(a => this.toUpcomingSession(a, clientMap));
        this.upcomingSessions.set(upcoming);
      })
      .catch(() => { /* silent */ })
      .finally(() => { this.sessionsLoading.set(false); this.cdr.markForCheck(); });
  }

  private getMondayOfCurrentWeek(): Date {
    const today = new Date();
    const dayOfWeek = today.getDay();
    const monday = new Date(today);
    monday.setDate(today.getDate() - ((dayOfWeek + 6) % 7));
    monday.setHours(0, 0, 0, 0);
    return monday;
  }

  private buildWeekDays(activeDayStrings?: Set<string>): WeekDay[] {
    const today = new Date();
    const monday = this.getMondayOfCurrentWeek();
    return Array.from({ length: 7 }, (_, i) => {
      const d = new Date(monday);
      d.setDate(monday.getDate() + i);
      return {
        label: d.toLocaleDateString('en-US', { weekday: 'short' }),
        date: d.getDate(),
        hasSessions: activeDayStrings ? activeDayStrings.has(d.toDateString()) : false,
        isToday: d.toDateString() === today.toDateString(),
      };
    });
  }

  private toUpcomingSession(appt: AppointmentDto, clientMap: Map<string, ClientDto>): UpcomingSession {
    const client = clientMap.get(appt.clientId);
    const name = client?.name ?? appt.clientId.substring(0, 8);
    const words = name.split(' ');
    const initials = words.length >= 2
      ? (words[0][0] + words[words.length - 1][0]).toUpperCase()
      : name.substring(0, 2).toUpperCase();
    const colorIndex = appt.clientId.charCodeAt(0) % DashboardComponent.AVATAR_COLORS.length;
    const d = new Date(appt.scheduledAt);
    const time = d.toLocaleTimeString('en-US', { hour: 'numeric', minute: '2-digit' });
    const typeMap: Record<string, string> = {
      InPerson: 'In Person', Video: 'Video Call', Phone: 'Phone Call',
    };
    return {
      clientId: appt.clientId,
      initials,
      name,
      type: typeMap[appt.type] ?? appt.type,
      time,
      avatarColor: DashboardComponent.AVATAR_COLORS[colorIndex],
    };
  }
}
