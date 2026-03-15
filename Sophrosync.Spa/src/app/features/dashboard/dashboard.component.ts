import { Component, inject } from '@angular/core';
import { AuthService } from '../../core/auth/auth.service';
import { AppointmentsCalendarComponent } from './appointments-calendar/appointments-calendar.component';
import { NextSessionCardComponent } from './next-session-card/next-session-card.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [AppointmentsCalendarComponent, NextSessionCardComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent {
  protected readonly auth = inject(AuthService);
  protected readonly profile = this.auth.userProfile;
}
