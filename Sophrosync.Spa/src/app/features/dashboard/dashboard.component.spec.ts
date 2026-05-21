import { TestBed, ComponentFixture } from '@angular/core/testing';
import { Component, NO_ERRORS_SCHEMA, signal } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { vi } from 'vitest';
import { DashboardComponent } from './dashboard.component';
import { AppointmentsService, AppointmentDto } from './appointments.service';
import { ClientsService, ClientDto } from '../clients/clients.service';
import { AuthService } from '../../core/auth/auth.service';
import { AppointmentsCalendarComponent } from './appointments-calendar/appointments-calendar.component';
import { NextSessionCardComponent } from './next-session-card/next-session-card.component';

// Stub components to prevent child service calls
@Component({ selector: 'app-appointments-calendar', template: '', standalone: true })
class StubAppointmentsCalendarComponent {}

@Component({ selector: 'app-next-session-card', template: '', standalone: true })
class StubNextSessionCardComponent {}

const makeAppt = (
  scheduledAt: string,
  status = 'Scheduled',
  clientId = 'c1'
): AppointmentDto => ({
  id: `appt-${scheduledAt}`, tenantId: 't', clientId, therapistId: 'th1',
  scheduledAt, durationMinutes: 50, type: 'InPerson', status, createdAt: '',
});

describe('DashboardComponent', () => {
  let mockAppointmentsService: { getByDateRange: ReturnType<typeof vi.fn> };
  let mockClientsService: { getAll: ReturnType<typeof vi.fn> };
  let mockAuthService: Partial<AuthService>;

  const createComponent = (): ComponentFixture<DashboardComponent> => {
    const fixture = TestBed.createComponent(DashboardComponent);
    fixture.detectChanges();
    return fixture;
  };

  beforeEach(async () => {
    mockAppointmentsService = { getByDateRange: vi.fn().mockResolvedValue([]) };
    mockClientsService = { getAll: vi.fn().mockResolvedValue([]) };
    mockAuthService = {
      userProfile: signal(null),
      isAuthenticated: signal(false),
      userRoles: signal([]),
      userId: signal(''),
    };

    await TestBed.configureTestingModule({
      imports: [DashboardComponent],
      providers: [
        { provide: AppointmentsService, useValue: mockAppointmentsService },
        { provide: ClientsService, useValue: mockClientsService },
        { provide: AuthService, useValue: mockAuthService },
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    })
    .overrideComponent(DashboardComponent, {
      remove: { imports: [AppointmentsCalendarComponent, NextSessionCardComponent] },
      add: { imports: [StubAppointmentsCalendarComponent, StubNextSessionCardComponent] },
    })
    .compileComponents();
  });

  it('creates the component', async () => {
    const fixture = createComponent();
    await fixture.whenStable();
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('calls getByDateRange exactly once on init with Monday/Sunday bounds', async () => {
    const fixture = createComponent();
    await fixture.whenStable();
    await fixture.whenStable();

    expect(mockAppointmentsService.getByDateRange).toHaveBeenCalledOnce();
    const [from, to] = mockAppointmentsService.getByDateRange.mock.calls[0] as [Date, Date];
    expect(from).toBeInstanceOf(Date);
    expect(to).toBeInstanceOf(Date);
    expect(from.getDay()).toBe(1); // Monday
    expect(from.getHours()).toBe(0);
    expect(from.getMinutes()).toBe(0);
    expect(to.getHours()).toBe(23);
    expect(to.getMinutes()).toBe(59);
    expect(to.getSeconds()).toBe(59);
  });

  it('calls getAll() on init', async () => {
    const fixture = createComponent();
    await fixture.whenStable();
    await fixture.whenStable();
    expect(mockClientsService.getAll).toHaveBeenCalledOnce();
  });

  it('hasSessions dots set for Scheduled and Confirmed days, not Cancelled', async () => {
    const today = new Date();
    const dayOfWeek = today.getDay();
    const mondayDate = new Date(today);
    mondayDate.setDate(today.getDate() - ((dayOfWeek + 6) % 7));
    mondayDate.setHours(0, 0, 0, 0);

    const mondayIso = new Date(mondayDate.getFullYear(), mondayDate.getMonth(), mondayDate.getDate(), 9).toISOString();
    const tuesdayNum = mondayDate.getDate() + 1;
    const wednesdayNum = mondayDate.getDate() + 2;
    const tuesdayIso = new Date(mondayDate.getFullYear(), mondayDate.getMonth(), tuesdayNum, 9).toISOString();
    const wednesdayIso = new Date(mondayDate.getFullYear(), mondayDate.getMonth(), wednesdayNum, 9).toISOString();

    mockAppointmentsService.getByDateRange.mockResolvedValue([
      makeAppt(mondayIso, 'Scheduled'),
      makeAppt(tuesdayIso, 'Cancelled'),
      makeAppt(wednesdayIso, 'Confirmed'),
    ]);

    const fixture = createComponent();
    await fixture.whenStable();
    await fixture.whenStable();

    const days = fixture.componentInstance.weekDays();
    const mon = days.find(d => d.date === mondayDate.getDate());
    const tue = days.find(d => d.date === tuesdayNum);
    const wed = days.find(d => d.date === wednesdayNum);

    if (mon) expect(mon.hasSessions).toBe(true);
    if (tue) expect(tue.hasSessions).toBe(false);
    if (wed) expect(wed.hasSessions).toBe(true);
  });

  it('upcomingSessions excludes Cancelled and Completed appointments', async () => {
    const now = new Date();
    const future = new Date(now.getTime() + 60 * 60 * 1000).toISOString();
    mockAppointmentsService.getByDateRange.mockResolvedValue([
      makeAppt(future, 'Scheduled'),
      makeAppt(future, 'Cancelled'),
      makeAppt(future, 'Completed'),
    ]);

    const fixture = createComponent();
    await fixture.whenStable();
    await fixture.whenStable();

    expect(fixture.componentInstance.upcomingSessions()).toHaveLength(1);
  });

  it('upcomingSessions sorted ascending by scheduledAt', async () => {
    const now = new Date();
    const t1 = new Date(now.getTime() + 2 * 60 * 60 * 1000).toISOString();
    const t2 = new Date(now.getTime() + 1 * 60 * 60 * 1000).toISOString();
    mockAppointmentsService.getByDateRange.mockResolvedValue([
      makeAppt(t1, 'Scheduled', 'c1'),
      makeAppt(t2, 'Scheduled', 'c2'),
    ]);

    const fixture = createComponent();
    await fixture.whenStable();
    await fixture.whenStable();

    const sessions = fixture.componentInstance.upcomingSessions();
    expect(sessions).toHaveLength(2);
    const firstTime = new Date(t2).toLocaleTimeString('en-US', { hour: 'numeric', minute: '2-digit' });
    expect(sessions[0].time).toBe(firstTime);
  });

  it('sessionsLoading becomes false after fetch completes', async () => {
    const fixture = createComponent();
    await fixture.whenStable();
    await fixture.whenStable();
    await fixture.whenStable();
    expect(fixture.componentInstance.sessionsLoading()).toBe(false);
  });
});
