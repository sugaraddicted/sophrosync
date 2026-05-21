import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { MonthGridComponent } from './month-grid/month-grid.component';
import { AppointmentsService, AppointmentDto } from '../appointments.service';

export interface Appointment {
  id: string;
  day: number;
  time: string;   // "HH:MM"
  client: string;
}

interface MonthDescriptor {
  month: number;
  year: number;
  label: string;
}

@Component({
  selector: 'app-appointments-calendar',
  standalone: true,
  imports: [MonthGridComponent],
  templateUrl: './appointments-calendar.component.html',
  styleUrl: './appointments-calendar.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppointmentsCalendarComponent implements OnInit {
  private readonly today = new Date();
  private readonly appointmentsService = inject(AppointmentsService);
  private readonly cdr = inject(ChangeDetectorRef);

  readonly windowOffset = signal(0);
  readonly appointments = signal<Appointment[]>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  readonly currentMonth = computed<MonthDescriptor>(() => {
    const d = new Date(this.today.getFullYear(), this.today.getMonth() + this.windowOffset(), 1);
    return {
      month: d.getMonth(),
      year: d.getFullYear(),
      label: d.toLocaleString('default', { month: 'long', year: 'numeric' }),
    };
  });

  ngOnInit(): void {
    this.loadCurrentMonth();
  }

  prev(): void {
    this.windowOffset.update(v => v - 1);
    this.loadCurrentMonth();
  }

  next(): void {
    this.windowOffset.update(v => v + 1);
    this.loadCurrentMonth();
  }

  private loadCurrentMonth(): void {
    const offset = this.windowOffset();
    const d = new Date(this.today.getFullYear(), this.today.getMonth() + offset, 1);
    const from = new Date(d.getFullYear(), d.getMonth(), 1);
    const to   = new Date(d.getFullYear(), d.getMonth() + 1, 0, 23, 59, 59);
    this.loading.set(true);
    this.error.set(null);

    this.appointmentsService.getByDateRange(from, to)
      .then(dtos => { this.appointments.set(dtos.map(dto => this.toAppointment(dto))); })
      .catch(() => { this.error.set('Failed to load appointments'); this.appointments.set([]); })
      .finally(() => { this.loading.set(false); this.cdr.markForCheck(); });
  }

  private readonly TYPE_LABELS: Record<string, string> = {
    InPerson: 'In Person',
    Video: 'Video Call',
    Phone: 'Phone Call',
  };

  private typeLabel(type: string): string {
    return this.TYPE_LABELS[type] ?? type;
  }

  private toAppointment(dto: AppointmentDto): Appointment {
    const d = new Date(dto.scheduledAt);
    const hours   = String(d.getHours()).padStart(2, '0');
    const minutes = String(d.getMinutes()).padStart(2, '0');
    return {
      id: dto.id,
      day: d.getDate(),
      time: `${hours}:${minutes}`,
      client: this.typeLabel(dto.type),
    };
  }
}
