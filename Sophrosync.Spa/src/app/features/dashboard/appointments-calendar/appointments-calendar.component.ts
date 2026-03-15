import { Component } from '@angular/core';
import { NgClass } from '@angular/common';

@Component({
  selector: 'app-appointments-calendar',
  standalone: true,
  imports: [NgClass],
  templateUrl: './appointments-calendar.component.html',
  styleUrl: './appointments-calendar.component.scss',
})
export class AppointmentsCalendarComponent {
  readonly monthLabel = 'March 2026';
  readonly dayLabels = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];
  // March 2026 starts on Sunday — in a Mon-Sun grid that is column index 6 (0-based), so 6 leading blank cells
  readonly leadingBlanks = Array(6).fill(null);
  readonly days = Array.from({ length: 31 }, (_, i) => i + 1);
  // Hardcoded placeholder appointment indicators
  readonly appointmentDays = new Set([5, 12, 18, 25]);
}
