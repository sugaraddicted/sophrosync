import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { MonthGridComponent } from '../dashboard/appointments-calendar/month-grid/month-grid.component';
import { Appointment } from '../dashboard/appointments-calendar/appointments-calendar.component';
import { AppointmentsService, AppointmentDto, CreateAppointmentDto } from '../dashboard/appointments.service';
import { ClientsService, ClientDto } from '../clients/clients.service';
import { AuthService } from '../../core/auth/auth.service';
import { ScheduleAppointmentModalComponent } from './schedule-appointment-modal/schedule-appointment-modal.component';
import { AppointmentDetailModalComponent, AppointmentAction } from './appointment-detail-modal/appointment-detail-modal.component';

@Component({
  selector: 'app-calendar',
  standalone: true,
  imports: [
    MonthGridComponent,
    ScheduleAppointmentModalComponent,
    AppointmentDetailModalComponent,
  ],
  templateUrl: './calendar.component.html',
  styleUrl: './calendar.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CalendarComponent implements OnInit {
  private readonly appointmentsService = inject(AppointmentsService);
  private readonly clientsService = inject(ClientsService);
  private readonly auth = inject(AuthService);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly today = new Date();

  readonly windowOffset = signal(0);
  readonly appointments = signal<AppointmentDto[]>([]);
  readonly clients = signal<ClientDto[]>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly showScheduleModal = signal(false);
  readonly prefillDate = signal<string | null>(null);
  readonly selectedAppointment = signal<AppointmentDto | null>(null);

  readonly currentMonth = computed(() => {
    const d = new Date(this.today.getFullYear(), this.today.getMonth() + this.windowOffset(), 1);
    return {
      month: d.getMonth(),
      year: d.getFullYear(),
      label: d.toLocaleString('default', { month: 'long', year: 'numeric' }),
    };
  });

  readonly clientNames = computed(() =>
    new Map(this.clients().map(c => [c.id, c.name]))
  );

  readonly mappedAppointments = computed(() =>
    this.appointments().map(dto => this.toAppointment(dto))
  );

  ngOnInit(): void {
    Promise.all([
      this.loadCurrentMonthPromise(),
      this.clientsService.getAll().then(c => this.clients.set(c)),
    ]).finally(() => this.cdr.markForCheck());
  }

  prev(): void {
    this.windowOffset.update(v => v - 1);
    this.loadCurrentMonth();
  }

  next(): void {
    this.windowOffset.update(v => v + 1);
    this.loadCurrentMonth();
  }

  loadCurrentMonth(): void {
    this.loadCurrentMonthPromise().finally(() => this.cdr.markForCheck());
  }

  private loadCurrentMonthPromise(): Promise<void> {
    const { year, month } = this.currentMonth();
    const from = new Date(year, month, 1);
    const to = new Date(year, month + 1, 0, 23, 59, 59, 999);
    this.loading.set(true);
    this.error.set(null);
    return this.appointmentsService.getByDateRange(from, to)
      .then(dtos => { this.appointments.set(dtos); })
      .catch(() => { this.error.set('Failed to load appointments'); })
      .finally(() => { this.loading.set(false); });
  }

  private toAppointment(dto: AppointmentDto): Appointment {
    const d = new Date(dto.scheduledAt);
    const hours = String(d.getHours()).padStart(2, '0');
    const minutes = String(d.getMinutes()).padStart(2, '0');
    const typeLabels: Record<string, string> = {
      InPerson: 'In Person', Video: 'Video Call', Phone: 'Phone Call',
    };
    return {
      id: dto.id,
      day: d.getDate(),
      time: `${hours}:${minutes}`,
      client: typeLabels[dto.type] ?? dto.type,
    };
  }

  onDayClicked(day: number): void {
    const { year, month } = this.currentMonth();
    const d = new Date(year, month, day, 9, 0);
    const pad = (n: number) => String(n).padStart(2, '0');
    const str = `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
    this.prefillDate.set(str);
    this.showScheduleModal.set(true);
  }

  onAppointmentClicked(appt: Appointment): void {
    this.selectedAppointment.set(
      this.appointments().find(a => a.id === appt.id) ?? null
    );
  }

  onScheduleSubmit(dto: CreateAppointmentDto): void {
    const fullDto: CreateAppointmentDto = { ...dto, therapistId: this.auth.userId() };
    this.appointmentsService.createAppointment(fullDto)
      .then(() => {
        this.showScheduleModal.set(false);
        this.prefillDate.set(null);
        this.loadCurrentMonth();
      })
      .finally(() => this.cdr.markForCheck());
  }

  onScheduleCancelled(): void {
    this.showScheduleModal.set(false);
    this.prefillDate.set(null);
  }

  onAction(action: AppointmentAction): void {
    const appt = this.selectedAppointment();
    if (!appt) return;
    const id = appt.id;
    let call: Promise<void>;
    switch (action.type) {
      case 'confirm':
        call = this.appointmentsService.confirmAppointment(id);
        break;
      case 'cancel':
        call = this.appointmentsService.cancelAppointment(id, action.reason);
        break;
      case 'reschedule':
        call = this.appointmentsService.rescheduleAppointment(id, action.newScheduledAt, action.newDurationMinutes);
        break;
    }
    call
      .then(() => {
        this.selectedAppointment.set(null);
        this.loadCurrentMonth();
      })
      .finally(() => this.cdr.markForCheck());
  }
}
