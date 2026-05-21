import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { vi } from 'vitest';
import { ScheduleAppointmentModalComponent } from './schedule-appointment-modal.component';
import { ClientsService, ClientDto } from '../../clients/clients.service';
import { CreateAppointmentDto } from '../../dashboard/appointments.service';

const makeClient = (n: number): ClientDto => ({
  id: `client-${n}`, name: `Client ${n}`,
  email: `c${n}@x.com`, phone: `000${n}`, status: 'Active',
});

describe('ScheduleAppointmentModalComponent', () => {
  let mockClientsService: { getAll: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    mockClientsService = { getAll: vi.fn().mockResolvedValue([]) };

    await TestBed.configureTestingModule({
      imports: [ScheduleAppointmentModalComponent],
      providers: [
        { provide: ClientsService, useValue: mockClientsService },
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    }).compileComponents();
  });

  it('creates the component', async () => {
    const fixture = TestBed.createComponent(ScheduleAppointmentModalComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('calls ClientsService.getAll() on init', async () => {
    const fixture = TestBed.createComponent(ScheduleAppointmentModalComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    expect(mockClientsService.getAll).toHaveBeenCalledOnce();
  });

  it('populates clients signal after successful load', async () => {
    mockClientsService.getAll.mockResolvedValue([makeClient(1), makeClient(2)]);
    const fixture = TestBed.createComponent(ScheduleAppointmentModalComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    expect(fixture.componentInstance.clients()).toHaveLength(2);
    expect(fixture.componentInstance.clients()[0].name).toBe('Client 1');
  });

  it('sets clientsError signal on service failure', async () => {
    mockClientsService.getAll.mockRejectedValue(new Error('network error'));
    const fixture = TestBed.createComponent(ScheduleAppointmentModalComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    await fixture.whenStable();
    expect(fixture.componentInstance.clientsError()).not.toBeNull();
    expect(fixture.componentInstance.clients()).toHaveLength(0);
  });

  it('clears error and reloads on loadClients() retry', async () => {
    mockClientsService.getAll
      .mockRejectedValueOnce(new Error('network error'))
      .mockResolvedValue([makeClient(1)]);
    const fixture = TestBed.createComponent(ScheduleAppointmentModalComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    await fixture.whenStable();
    expect(fixture.componentInstance.clientsError()).not.toBeNull();

    fixture.componentInstance.loadClients();
    await fixture.whenStable();
    await fixture.whenStable();
    expect(fixture.componentInstance.clientsError()).toBeNull();
    expect(fixture.componentInstance.clients()).toHaveLength(1);
  });

  it('form is invalid when clientId is empty', () => {
    const fixture = TestBed.createComponent(ScheduleAppointmentModalComponent);
    fixture.detectChanges();
    const component = fixture.componentInstance;
    component.form.markAllAsTouched();
    expect(component.form.controls.clientId.invalid).toBe(true);
    expect(component.form.invalid).toBe(true);
  });

  it('form is invalid when scheduledAt is empty', () => {
    const fixture = TestBed.createComponent(ScheduleAppointmentModalComponent);
    fixture.detectChanges();
    const component = fixture.componentInstance;
    component.form.controls.clientId.setValue('client-1');
    component.form.controls.scheduledAt.setValue('');
    component.form.markAllAsTouched();
    expect(component.form.controls.scheduledAt.invalid).toBe(true);
  });

  it('emits submitted with correct dto on valid form (therapistId is empty string)', async () => {
    mockClientsService.getAll.mockResolvedValue([makeClient(1)]);
    const fixture = TestBed.createComponent(ScheduleAppointmentModalComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;
    const submittedValues: CreateAppointmentDto[] = [];
    component.submitted.subscribe((v) => submittedValues.push(v));

    component.form.setValue({
      clientId: 'client-1',
      scheduledAt: '2026-06-15T09:00',
      durationMinutes: 50,
      type: 'InPerson',
      notes: '',
    });

    component.onSubmit();

    expect(submittedValues).toHaveLength(1);
    expect(submittedValues[0].clientId).toBe('client-1');
    expect(submittedValues[0].therapistId).toBe('');
    expect(submittedValues[0].durationMinutes).toBe(50);
    expect(submittedValues[0].type).toBe('InPerson');
  });

  it('emits cancelled when onCancel() is called', () => {
    const fixture = TestBed.createComponent(ScheduleAppointmentModalComponent);
    fixture.detectChanges();
    const component = fixture.componentInstance;
    let cancelled = false;
    component.cancelled.subscribe(() => { cancelled = true; });
    component.onCancel();
    expect(cancelled).toBe(true);
  });

  it('patches scheduledAt from prefillDate input', async () => {
    const fixture = TestBed.createComponent(ScheduleAppointmentModalComponent);
    fixture.componentRef.setInput('prefillDate', '2026-07-01T10:00');
    fixture.detectChanges();
    await fixture.whenStable();
    expect(fixture.componentInstance.form.controls.scheduledAt.value).toBe('2026-07-01T10:00');
  });
});
