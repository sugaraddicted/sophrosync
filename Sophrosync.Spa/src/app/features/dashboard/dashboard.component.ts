import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { AuthService } from '../../core/auth/auth.service';
import { AppointmentsCalendarComponent } from './appointments-calendar/appointments-calendar.component';

interface UpcomingSession {
  initials: string;
  name: string;
  type: string;
  time: string;
  avatarColor: string;
}

@Component({
  selector: 'app-dashboard',
  imports: [AppointmentsCalendarComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardComponent {
  protected readonly auth = inject(AuthService);
  protected readonly profile = this.auth.userProfile;

  protected readonly greeting = computed(() => {
    const name = this.profile()?.firstName;
    return name ? `Good morning, ${name}.` : 'Good morning.';
  });

  protected readonly today = new Date();

  protected readonly formattedDate = this.today.toLocaleDateString('en-US', {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  });

  protected readonly weekDays = this.buildWeekDays();

  protected readonly upcomingSessions: UpcomingSession[] = [
    { initials: 'AB', name: 'Anna Berg',    type: 'Individual therapy',    time: '9:00 AM',  avatarColor: '#546253' },
    { initials: 'TR', name: 'Tom Reeves',   type: 'Cognitive behavioural', time: '11:30 AM', avatarColor: '#6b5b5b' },
    { initials: 'JD', name: 'Jane Doe',     type: 'Assessment session',    time: '2:00 PM',  avatarColor: '#5f5f5f' },
  ];

  protected readonly reflectionText = signal('');

  protected readonly weeklyHoursPercent = 68;
  protected readonly monthlyTargetPercent = 52;

  private buildWeekDays(): { label: string; date: number; hasSessions: boolean; isToday: boolean }[] {
    const today = new Date();
    const dayOfWeek = today.getDay(); // 0 = Sunday
    // Find Monday of current week
    const monday = new Date(today);
    monday.setDate(today.getDate() - ((dayOfWeek + 6) % 7));

    const sessionDays = new Set([1, 3]); // placeholder: sessions on Mon + Wed indices

    return Array.from({ length: 7 }, (_, i) => {
      const d = new Date(monday);
      d.setDate(monday.getDate() + i);
      return {
        label: d.toLocaleDateString('en-US', { weekday: 'short' }),
        date: d.getDate(),
        hasSessions: sessionDays.has(i),
        isToday: d.toDateString() === today.toDateString(),
      };
    });
  }
}
