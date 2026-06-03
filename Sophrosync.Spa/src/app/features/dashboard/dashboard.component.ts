import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { NotesService } from '../notes/notes.service';
import { Note } from '../notes/models/note.model';
import { SettingsService } from '../settings/settings.service';
import { AppointmentsCalendarComponent } from './appointments-calendar/appointments-calendar.component';
import { NextSessionCardComponent } from './next-session-card/next-session-card.component';
import { AppointmentsService, AppointmentDto } from './appointments.service';
import { ClientsService, ClientDto } from '../clients/clients.service';
import { ReportsService } from '../reports/reports.service';
import { AppointmentSummaryDto } from '../reports/report.model';
import { firstValueFrom } from 'rxjs';

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
  private readonly router = inject(Router);
  private readonly appointmentsService = inject(AppointmentsService);
  private readonly clientsService = inject(ClientsService);
  private readonly reportsService = inject(ReportsService);
  private readonly notesService = inject(NotesService);
  private readonly settingsService = inject(SettingsService);
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

  // Raw data signals populated by ngOnInit
  private readonly appointments = signal<AppointmentDto[]>([]);
  private readonly clients = signal<ClientDto[]>([]);
  private readonly appointmentSummary = signal<AppointmentSummaryDto | null>(null);

  // Derived: sessions in the current ISO week (Monâ€“Sun)
  protected readonly appointmentsThisWeek = computed(() => {
    const monday = this.getMondayOfCurrentWeek();
    const sundayEnd = new Date(monday);
    sundayEnd.setDate(monday.getDate() + 6);
    sundayEnd.setHours(23, 59, 59, 999);
    return this.appointments().filter(a => {
      const d = new Date(a.scheduledAt);
      return d >= monday && d <= sundayEnd;
    }).length;
  });

  // Derived: sessions in the current calendar month
  protected readonly appointmentsThisMonth = computed(() => {
    const now = new Date();
    return this.appointments().filter(a => {
      const d = new Date(a.scheduledAt);
      return d.getFullYear() === now.getFullYear() && d.getMonth() === now.getMonth();
    }).length;
  });

  // Derived: sessions today
  protected readonly appointmentsToday = computed(() => {
    const todayStr = new Date().toDateString();
    return this.appointments().filter(a =>
      new Date(a.scheduledAt).toDateString() === todayStr
    ).length;
  });

  private readonly practiceTargets = this.settingsService.getPracticeTargets();

  // Progress toward configurable weekly target (default 5)
  protected readonly weeklyHoursPercent = computed(() =>
    Math.min(100, Math.round(this.appointmentsThisWeek() / this.practiceTargets.weeklySessionTarget * 100))
  );

  // Progress toward configurable monthly target (default 20)
  protected readonly monthlyTargetPercent = computed(() =>
    Math.min(100, Math.round(this.appointmentsThisMonth() / this.practiceTargets.monthlySessionTarget * 100))
  );

  // Retention proxy: completion rate derived from AppointmentSummaryDto
  protected readonly retentionPercent = computed(() => {
    const summary = this.appointmentSummary();
    if (!summary) return null;
    const scheduled = summary.totalScheduled;
    return Math.min(100, Math.round(
      (1 - summary.totalCancelled / Math.max(1, scheduled)) * 100
    ));
  });

  // Active clients count
  protected readonly activeClientsCount = computed(() => this.clients().length);

  // Avg engagement months â€” ClientDto has no createdAt, so always 'â€”'
  protected readonly avgEngagementMonths = 'â€”';

  // Notes not locked, created > 14 days ago
  protected readonly overdueNotes = signal<Note[]>([]);

  // Current month label for Practice Velocity subtitle
  protected readonly currentMonthLabel = computed(() => {
    return new Date().toLocaleDateString('en-US', { month: 'long', year: 'numeric' });
  });

  private static readonly AVATAR_COLORS = [
    '#546253', '#6b5b5b', '#5f5f5f', '#4a6b6b', '#5b4a6b', '#6b6b4a',
  ];

  ngOnInit(): void {
    const now = new Date();

    // Week bounds for the week-strip calendar dots
    const monday = this.getMondayOfCurrentWeek();
    const sunday = new Date(monday);
    sunday.setDate(monday.getDate() + 6);
    sunday.setHours(23, 59, 59, 999);

    // Month bounds for appointmentsThisMonth + appointment summary
    const startOfMonth = new Date(now.getFullYear(), now.getMonth(), 1);
    startOfMonth.setHours(0, 0, 0, 0);
    const endOfMonth = new Date(now.getFullYear(), now.getMonth() + 1, 0);
    endOfMonth.setHours(23, 59, 59, 999);

    Promise.all([
      this.appointmentsService.getByDateRange(startOfMonth, endOfMonth),
      this.clientsService.getAll(),
      firstValueFrom(this.reportsService.getAppointmentSummary(startOfMonth, endOfMonth))
        .catch(() => null),
      firstValueFrom(this.notesService.getNotes()).catch(() => [] as Note[]),
    ])
      .then(([appts, loadedClients, summary, notes]) => {
        // Populate raw signals so computed() signals derive live values
        this.appointments.set(appts);
        this.clients.set(loadedClients);
        if (summary) {
          this.appointmentSummary.set(summary);
        }

        const clientMap = new Map(loadedClients.map(c => [c.id, c]));

        // Week-strip dots: filter to just this week's scheduled/confirmed
        const activeDayStrings = new Set(
          appts
            .filter(a => {
              if (a.status !== 'Scheduled' && a.status !== 'Confirmed') return false;
              const d = new Date(a.scheduledAt);
              return d >= monday && d <= sunday;
            })
            .map(a => new Date(a.scheduledAt).toDateString())
        );
        this.weekDays.set(this.buildWeekDays(activeDayStrings));

        const upcoming = appts
          .filter(a =>
            (a.status === 'Scheduled' || a.status === 'Confirmed') &&
            new Date(a.scheduledAt) >= now
          )
          .sort((a, b) => a.scheduledAt.localeCompare(b.scheduledAt))
          .slice(0, 5)
          .map(a => this.toUpcomingSession(a, clientMap));
        this.upcomingSessions.set(upcoming);

        const fourteenDaysAgo = new Date(now.getTime() - 14 * 24 * 60 * 60 * 1000);
        const overdue = (notes as Note[]).filter(n =>
          (n.status === 'Draft' || n.status === 'Signed' || n.status === 'PendingCoSign') &&
          new Date(n.createdAt) < fourteenDaysAgo
        );
        this.overdueNotes.set(overdue);
      })
      .catch(() => { /* silent */ })
      .finally(() => { this.sessionsLoading.set(false); this.cdr.markForCheck(); });
  }

  protected onWeekDayClick(): void {
    this.router.navigate(['/calendar']);
  }

  protected isUrgentNote(note: Note): boolean {
    const seventyTwoHoursAgo = new Date(Date.now() - 72 * 60 * 60 * 1000);
    return new Date(note.updatedAt) < seventyTwoHoursAgo;
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
