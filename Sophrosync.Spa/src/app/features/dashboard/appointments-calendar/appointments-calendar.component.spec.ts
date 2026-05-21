import { TestBed } from '@angular/core/testing';
import { AppointmentsCalendarComponent } from './appointments-calendar.component';
import { AppointmentsService, AppointmentDto } from '../appointments.service';

const makeDto = (scheduledAt = '2026-05-15T09:00:00', type = 'InPerson'): AppointmentDto => ({
  id: 'dto-id-1', tenantId: 't1', clientId: 'c1', therapistId: 'th1',
  scheduledAt, durationMinutes: 60, type, status: 'Scheduled', createdAt: '',
});

describe('AppointmentsCalendarComponent', () => {
  let mockService: { getByDateRange: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    mockService = { getByDateRange: vi.fn().mockResolvedValue([]) };
    await TestBed.configureTestingModule({
      imports: [AppointmentsCalendarComponent],
      providers: [{ provide: AppointmentsService, useValue: mockService }],
    }).compileComponents();
  });

  it('creates the component', () => {
    const fixture = TestBed.createComponent(AppointmentsCalendarComponent);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('calls service on init with Date args', async () => {
    const fixture = TestBed.createComponent(AppointmentsCalendarComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    expect(mockService.getByDateRange).toHaveBeenCalledOnce();
    expect(mockService.getByDateRange.mock.calls[0][0]).toBeInstanceOf(Date);
    expect(mockService.getByDateRange.mock.calls[0][1]).toBeInstanceOf(Date);
  });

  it('populates appointments signal after successful load', async () => {
    mockService.getByDateRange.mockResolvedValue([makeDto('2026-05-15T09:00:00')]);
    const fixture = TestBed.createComponent(AppointmentsCalendarComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    const appts = fixture.componentInstance.appointments();
    expect(appts).toHaveLength(1);
    expect(appts[0].day).toBe(15);
    expect(appts[0].time).toBe('09:00');
  });

  it('maps id from dto.id', async () => {
    mockService.getByDateRange.mockResolvedValue([makeDto('2026-05-15T09:00:00')]);
    const fixture = TestBed.createComponent(AppointmentsCalendarComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    expect(fixture.componentInstance.appointments()[0].id).toBe('dto-id-1');
  });

  it('maps client to human-readable type label', async () => {
    mockService.getByDateRange.mockResolvedValue([makeDto('2026-05-15T09:00:00', 'InPerson')]);
    const fixture = TestBed.createComponent(AppointmentsCalendarComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    expect(fixture.componentInstance.appointments()[0].client).toBe('In Person');
  });

  it('sets loading to false after fetch completes', async () => {
    const fixture = TestBed.createComponent(AppointmentsCalendarComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    await fixture.whenStable();
    expect(fixture.componentInstance.loading()).toBe(false);
  });

  it('sets error signal and clears appointments on service failure', async () => {
    mockService.getByDateRange.mockRejectedValue(new Error('network error'));
    const fixture = TestBed.createComponent(AppointmentsCalendarComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    await fixture.whenStable();
    expect(fixture.componentInstance.error()).not.toBeNull();
    expect(fixture.componentInstance.appointments()).toHaveLength(0);
  });

  it('reloads appointments when next() is called', async () => {
    const fixture = TestBed.createComponent(AppointmentsCalendarComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.componentInstance.next();
    await fixture.whenStable();
    expect(mockService.getByDateRange).toHaveBeenCalledTimes(2);
  });
});
