import { Component, computed, input } from '@angular/core';
import { Appointment } from '../appointments-calendar.component';

@Component({
  selector: 'app-month-grid',
  imports: [],
  templateUrl: './month-grid.component.html',
  styleUrl: './month-grid.component.scss',
})
export class MonthGridComponent {
  readonly month = input.required<number>();   // 0-based
  readonly year = input.required<number>();
  readonly appointments = input<Appointment[]>([]);

  readonly dayLabels = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];

  readonly leadingBlanks = computed(() => {
    const firstDay = new Date(this.year(), this.month(), 1).getDay();
    return Array((firstDay + 6) % 7).fill(null);
  });

  readonly days = computed(() => {
    const count = new Date(this.year(), this.month() + 1, 0).getDate();
    return Array.from({ length: count }, (_, i) => i + 1);
  });

  readonly dayMap = computed<Map<number, Appointment[]>>(() => {
    const map = new Map<number, Appointment[]>();
    for (const appt of this.appointments()) {
      const list = map.get(appt.day) ?? [];
      list.push(appt);
      map.set(appt.day, list);
    }
    return map;
  });
}
