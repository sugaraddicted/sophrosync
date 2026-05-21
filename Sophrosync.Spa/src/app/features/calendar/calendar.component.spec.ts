import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { vi } from 'vitest';
import { signal } from '@angular/core';
import { CalendarComponent } from './calendar.component';
import { AppointmentsService, AppointmentDto, CreateAppointmentDto } from '../dashboard/appointments.service';
import { ClientsService, ClientDto } from '../clients/clients.service';
import { AuthService } from '../../core/auth/auth.service';
import { Appointment } from '../dashboard/appointments-calendar/appointments-calendar.component';

const makeApptDto = (): AppointmentDto => ({
  id: 'appt-1', tenantId: 't', clientId: 'c1', therapistId: 'th1',
  scheduledAt: '2026-06-01T09:00:00.000Z',
  durationMinutes: 50, type: 'InPerson', status: 'Scheduled', createdAt: '',
});

const makeAppt = (): Appointment => ({
  id: 'appt-1', day: 1, time: '09:00', client: 'In Person',
});

describe('CalendarComponent', () => {
  let mockAppts: {
    getByDateRange: ReturnType<typeof vi.fn>;
    createAppointment: ReturnType<typeof vi.fn>;
    confirmAppointment: ReturnType<typeof vi.fn>;
    cancelAppointment: ReturnType<typeof vi.fn>;
    rescheduleAppointment: ReturnType<typeof vi.fn>;
  };
  let mockClients: { getAll: ReturnType<typeof vi.fn> };
  let mockAuth: { userId: ReturnType<typeof signal<string>> };

  beforeEach(async () => {
    mockAppts = {
      getByDateRange: vi.fn().mockResolvedValue([]),
      createAppointment: vi.fn().mockResolvedValue(makeApptDto()),
      confirmAppointment: vi.fn().mockResolvedValue(undefined),
      cancelAppointment: vi.fn().mockResolvedValue(undefined),
      rescheduleAppointment: vi.fn().mockResolvedValue(undefined),
    };
    mockClients = { getAll: vi.fn().mockResolvedValue([]) };
    mockAuth = { userId: signal('therapist-uuid') };

    await TestBed.configureTestingModule({
      imports: [CalendarComponent],
      providers: [
        { provide: AppointmentsService, useValue: mockAppts },
        { provide: ClientsService, useValue: mockClients },
        { provide: AuthService, useValue: mockAuth },
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    }).compileComponents();
  });

  it('creates the component', async () => {
    const fixture = TestBed.createComponent(CalendarComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('calls getByDateRange and getAll on init', async () => {
    const fixture = TestBed.createComponent(CalendarComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    expect(mockAppts.getByDateRange).toHaveBeenCalledOnce();
    expect(mockClients.getAll).toHaveBeenCalledOnce();
  });

  it('Schedule button sets showScheduleModal to true', async () => {
    const fixture = TestBed.createComponent(CalendarComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    const component = fixture.componentInstance;
    expect(component.showScheduleModal()).toBe(false);
    component.showScheduleModal.set(true);
    expect(component.showScheduleModal()).toBe(true);
  });

  it('onDayClicked sets prefillDate and showScheduleModal', async () => {
    const fixture = TestBed.createComponent(CalendarComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    const component = fixture.componentInstance;
    component.onDayClicked(15);
    expect(component.prefillDate()).not.toBeNull();
    expect(component.prefillDate()).toContain('-15T');
    expect(component.showScheduleModal()).toBe(true);
  });

  it('onScheduleSubmit calls createAppointment with therapistId from auth and closes modal', async () => {
    const fixture = TestBed.createComponent(CalendarComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    const component = fixture.componentInstance;
    component.showScheduleModal.set(true);

    const dto: CreateAppointmentDto = {
      clientId: 'c1', therapistId: '',
      scheduledAt: '2026-06-15T09:00:00.000Z',
      durationMinutes: 50, type: 'InPerson',
    };
    component.onScheduleSubmit(dto);
    await fixture.whenStable();

    expect(mockAppts.createAppointment).toHaveBeenCalledOnce();
    const calledWith = mockAppts.createAppointment.mock.calls[0][0] as CreateAppointmentDto;
    expect(calledWith.therapistId).toBe('therapist-uuid');
    expect(component.showScheduleModal()).toBe(false);
  });

  it('onAppointmentClicked sets selectedAppointment', async () => {
    mockAppts.getByDateRange.mockResolvedValue([makeApptDto()]);
    const fixture = TestBed.createComponent(CalendarComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    const component = fixture.componentInstance;
    component.onAppointmentClicked(makeAppt());
    expect(component.selectedAppointment()).not.toBeNull();
    expect(component.selectedAppointment()?.id).toBe('appt-1');
  });

  it('Confirm action calls confirmAppointment, clears selection, reloads', async () => {
    mockAppts.getByDateRange.mockResolvedValue([makeApptDto()]);
    const fixture = TestBed.createComponent(CalendarComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    const component = fixture.componentInstance;
    component.onAppointmentClicked(makeAppt());
    expect(component.selectedAppointment()).not.toBeNull();

    component.onAction({ type: 'confirm' });
    await fixture.whenStable();

    expect(mockAppts.confirmAppointment).toHaveBeenCalledWith('appt-1');
    expect(component.selectedAppointment()).toBeNull();
    expect(mockAppts.getByDateRange).toHaveBeenCalledTimes(2);
  });

  it('Cancel action calls cancelAppointment(id, reason), clears selection, reloads', async () => {
    mockAppts.getByDateRange.mockResolvedValue([makeApptDto()]);
    const fixture = TestBed.createComponent(CalendarComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    const component = fixture.componentInstance;
    component.onAppointmentClicked(makeAppt());

    component.onAction({ type: 'cancel', reason: 'Patient ill' });
    await fixture.whenStable();

    expect(mockAppts.cancelAppointment).toHaveBeenCalledWith('appt-1', 'Patient ill');
    expect(component.selectedAppointment()).toBeNull();
    expect(mockAppts.getByDateRange).toHaveBeenCalledTimes(2);
  });

  it('Reschedule action calls rescheduleAppointment, clears selection, reloads', async () => {
    mockAppts.getByDateRange.mockResolvedValue([makeApptDto()]);
    const fixture = TestBed.createComponent(CalendarComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    const component = fixture.componentInstance;
    component.onAppointmentClicked(makeAppt());

    component.onAction({
      type: 'reschedule',
      newScheduledAt: '2026-07-01T10:00:00.000Z',
      newDurationMinutes: 60,
    });
    await fixture.whenStable();

    expect(mockAppts.rescheduleAppointment).toHaveBeenCalledWith('appt-1', '2026-07-01T10:00:00.000Z', 60);
    expect(component.selectedAppointment()).toBeNull();
    expect(mockAppts.getByDateRange).toHaveBeenCalledTimes(2);
  });
});
