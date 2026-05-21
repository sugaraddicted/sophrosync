import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
  output,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AppointmentDto } from '../../dashboard/appointments.service';

export type AppointmentAction =
  | { type: 'confirm' }
  | { type: 'cancel'; reason: string }
  | { type: 'reschedule'; newScheduledAt: string; newDurationMinutes?: number };

@Component({
  selector: 'app-appointment-detail-modal',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './appointment-detail-modal.component.html',
  styleUrl: './appointment-detail-modal.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppointmentDetailModalComponent {
  readonly appointment = input.required<AppointmentDto>();
  readonly clientNames = input<Map<string, string>>(new Map());
  readonly actionRequested = output<AppointmentAction>();
  readonly closed = output<void>();

  readonly pendingAction = signal<'cancel' | 'reschedule' | null>(null);
  readonly cancelReason = signal('');
  readonly rescheduleDate = signal('');
  readonly rescheduleDuration = signal<number | null>(null);

  readonly clientName = computed(() => {
    const id = this.appointment().clientId;
    return this.clientNames().get(id) ?? id.substring(0, 8) + '…';
  });

  readonly formattedDateTime = computed(() => {
    const d = new Date(this.appointment().scheduledAt);
    return d.toLocaleString('en-US', {
      weekday: 'short', year: 'numeric', month: 'short',
      day: 'numeric', hour: 'numeric', minute: '2-digit',
    });
  });

  readonly typeLabel = computed(() => {
    const labels: Record<string, string> = {
      InPerson: 'In Person', Video: 'Video Call', Phone: 'Phone Call',
    };
    return labels[this.appointment().type] ?? this.appointment().type;
  });

  readonly isEditable = computed(() => {
    const s = this.appointment().status;
    return s === 'Scheduled' || s === 'Confirmed';
  });

  readonly canConfirm = computed(() => this.appointment().status === 'Scheduled');

  onConfirm(): void {
    this.actionRequested.emit({ type: 'confirm' });
  }

  onStartCancel(): void {
    this.cancelReason.set('');
    this.pendingAction.set('cancel');
  }

  onConfirmCancel(): void {
    const reason = this.cancelReason().trim();
    if (!reason) return;
    this.actionRequested.emit({ type: 'cancel', reason });
  }

  onStartReschedule(): void {
    this.rescheduleDate.set('');
    this.rescheduleDuration.set(null);
    this.pendingAction.set('reschedule');
  }

  onConfirmReschedule(): void {
    const raw = this.rescheduleDate();
    if (!raw) return;
    const parsed = new Date(raw);
    if (isNaN(parsed.getTime())) return;
    const dur = this.rescheduleDuration();
    this.actionRequested.emit({
      type: 'reschedule',
      newScheduledAt: parsed.toISOString(),
      ...(dur !== null ? { newDurationMinutes: dur } : {}),
    });
  }

}
