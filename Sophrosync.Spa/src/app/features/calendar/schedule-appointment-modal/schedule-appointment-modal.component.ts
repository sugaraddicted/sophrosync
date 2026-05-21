import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  NgZone,
  OnInit,
  computed,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ClientsService, ClientDto } from '../../clients/clients.service';
import { CreateAppointmentDto } from '../../dashboard/appointments.service';
import { SearchableSelectComponent } from '../../../shared/components/searchable-select/searchable-select.component';

@Component({
  selector: 'app-schedule-appointment-modal',
  standalone: true,
  imports: [ReactiveFormsModule, SearchableSelectComponent],
  templateUrl: './schedule-appointment-modal.component.html',
  styleUrl: './schedule-appointment-modal.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ScheduleAppointmentModalComponent implements OnInit {
  private readonly clientsService = inject(ClientsService);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly fb = inject(FormBuilder);
  private readonly ngZone = inject(NgZone);

  readonly prefillDate = input<string | null>(null);
  readonly submitted = output<CreateAppointmentDto>();
  readonly cancelled = output<void>();

  readonly clients = signal<ClientDto[]>([]);
  readonly clientsLoading = signal(false);
  readonly clientsError = signal<string | null>(null);
  readonly submitting = signal(false);

  readonly form = this.fb.nonNullable.group({
    clientId:        ['', Validators.required],
    scheduledAt:     ['', Validators.required],
    durationMinutes: [50, [Validators.required, Validators.min(15), Validators.max(480)]],
    type:            ['InPerson', Validators.required],
    notes:           [''],
  });

  readonly clientOptions = computed(() =>
    this.clients().map(c => ({ value: c.id, label: c.name }))
  );

  ngOnInit(): void {
    this.loadClients();
    const pre = this.prefillDate();
    if (pre) this.form.controls.scheduledAt.setValue(pre);
  }

  loadClients(): void {
    this.clientsLoading.set(true);
    this.clientsError.set(null);
    this.ngZone.run(() => {
      this.clientsService.getAll()
        .then(data => { this.clients.set(data); })
        .catch(() => { this.clientsError.set('Failed to load clients. Please try again.'); })
        .finally(() => { this.clientsLoading.set(false); this.cdr.markForCheck(); });
    });
  }

  onSubmit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    const v = this.form.getRawValue();
    this.submitted.emit({
      clientId: v.clientId,
      therapistId: '',  // parent fills this in
      scheduledAt: new Date(v.scheduledAt).toISOString(),
      durationMinutes: Number(v.durationMinutes),
      type: v.type as 'InPerson' | 'Video' | 'Phone',
      notes: v.notes || undefined,
    });
  }

  onCancel(): void {
    this.cancelled.emit();
  }
}
