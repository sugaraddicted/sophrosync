import { Component, computed, signal } from '@angular/core';
import { MonthGridComponent } from './month-grid/month-grid.component';

export interface Appointment {
  day: number;
  time: string;   // "HH:MM"
  client: string;
}

interface MonthDescriptor {
  month: number;
  year: number;
  label: string;
  appointments: Appointment[];
}

// Placeholder data keyed by "YYYY-M" (month 0-based)
const PLACEHOLDER_APPOINTMENTS: Record<string, Appointment[]> = {
  '2026-2': [
    { day: 5,  time: '09:00', client: 'Anna Berg' },
    { day: 12, time: '11:30', client: 'Tom Reeves' },
    { day: 17, time: '10:00', client: 'Jane Doe' },
    { day: 18, time: '14:00', client: 'Maria Stone' },
    { day: 24, time: '09:30', client: 'James Park' },
    { day: 25, time: '16:00', client: 'Lucy Chen' },
  ],
  '2026-3': [
    { day: 2,  time: '10:00', client: 'Anna Berg' },
    { day: 9,  time: '13:00', client: 'Tom Reeves' },
    { day: 16, time: '11:00', client: 'Jane Doe' },
    { day: 23, time: '15:30', client: 'Maria Stone' },
  ],
  '2026-4': [
    { day: 7,  time: '09:00', client: 'James Park' },
    { day: 14, time: '14:00', client: 'Anna Berg' },
    { day: 21, time: '10:30', client: 'Tom Reeves' },
  ],
};

@Component({
  selector: 'app-appointments-calendar',
  imports: [MonthGridComponent],
  templateUrl: './appointments-calendar.component.html',
  styleUrl: './appointments-calendar.component.scss',
})
export class AppointmentsCalendarComponent {
  private readonly today = new Date();

  readonly windowOffset = signal(0);

  readonly currentMonth = computed<MonthDescriptor>(() => {
    const d = new Date(this.today.getFullYear(), this.today.getMonth() + this.windowOffset(), 1);
    const key = `${d.getFullYear()}-${d.getMonth()}`;
    return {
      month: d.getMonth(),
      year: d.getFullYear(),
      label: d.toLocaleString('default', { month: 'long', year: 'numeric' }),
      appointments: PLACEHOLDER_APPOINTMENTS[key] ?? [],
    };
  });

  prev(): void { this.windowOffset.update(v => v - 1); }
  next(): void { this.windowOffset.update(v => v + 1); }
}
