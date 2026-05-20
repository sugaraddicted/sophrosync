import { Component } from '@angular/core';

interface Session {
  initials: string;
  name: string;
  type: string;
  time: string;
  location: string;
}

interface WeekDay {
  label: string;
  date: number;
  isToday: boolean;
  sessions: { time: string; label: string }[];
}

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent {
  readonly today = new Date();

  readonly dateLabel = this.today.toLocaleDateString('en-US', {
    weekday: 'long',
    month: 'long',
    day: 'numeric',
  });

  readonly greeting = this.buildGreeting();

  readonly weekDays: WeekDay[] = this.buildWeek();

  readonly upcomingSessions: Session[] = [
    { initials: 'JV', name: 'Julian Vance',      type: 'Cognitive Behavioral Therapy', time: '10:00 AM', location: 'Room 4B / Remote' },
    { initials: 'ER', name: 'Elena Rodriguez',   type: 'Family Systems Intake',        time: '02:30 PM', location: 'Studio A'         },
    { initials: 'MD', name: 'Marcus Dupont',      type: 'Standard Follow-up',           time: '04:00 PM', location: 'Remote Link'      },
  ];

  private buildGreeting(): string {
    const hour = this.today.getHours();
    if (hour < 12) return 'Good morning.';
    if (hour < 17) return 'Good afternoon.';
    return 'Good evening.';
  }

  private buildWeek(): WeekDay[] {
    const days = ['MON', 'TUE', 'WED', 'THU', 'FRI', 'SAT', 'SUN'];
    const todayNum = this.today.getDay(); // 0 = Sun
    // ISO week: Mon = 0
    const isoDay = (todayNum + 6) % 7;
    const monday = new Date(this.today);
    monday.setDate(this.today.getDate() - isoDay);

    return days.map((label, i) => {
      const d = new Date(monday);
      d.setDate(monday.getDate() + i);
      const isToday = d.toDateString() === this.today.toDateString();
      const sessions = i === 0
        ? [{ time: '10:00', label: 'Julian V.' }, { time: '14:30', label: 'Elena R.' }]
        : i === 2
        ? [{ time: '09:00', label: 'Staff Mtg' }]
        : [];
      return { label, date: d.getDate(), isToday, sessions };
    });
  }
}
