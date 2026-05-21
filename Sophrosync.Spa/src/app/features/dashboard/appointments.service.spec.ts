import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { AppointmentsService, AppointmentDto, CreateAppointmentDto } from './appointments.service';
import { environment } from '../../../environments/environment';

describe('AppointmentsService', () => {
  let service: AppointmentsService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [AppointmentsService, provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(AppointmentsService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('sends GET to /api/appointments/range with from and to params', () => {
    const from = new Date('2026-05-01T00:00:00.000Z');
    const to   = new Date('2026-05-31T23:59:59.000Z');
    service.getByDateRange(from, to);
    const req = httpMock.expectOne(r =>
      r.url.includes('/appointments/range') &&
      r.params.get('from') === from.toISOString() &&
      r.params.get('to')   === to.toISOString()
    );
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('returns mapped DTO array from HTTP response', async () => {
    const dto: AppointmentDto = {
      id: '1', tenantId: 't1', clientId: 'c1', therapistId: 'th1',
      scheduledAt: '2026-05-15T09:00:00Z', durationMinutes: 60,
      type: 'InPerson', status: 'Scheduled', createdAt: '',
    };
    const from = new Date('2026-05-01T00:00:00.000Z');
    const to   = new Date('2026-05-31T23:59:59.000Z');
    const promise = service.getByDateRange(from, to);
    httpMock.expectOne(() => true).flush([dto]);
    const result = await promise;
    expect(result).toHaveLength(1);
    expect(result[0].id).toBe('1');
    expect(result[0].type).toBe('InPerson');
  });

  it('rejects when HTTP returns error', async () => {
    const from = new Date('2026-05-01T00:00:00.000Z');
    const to   = new Date('2026-05-31T23:59:59.000Z');
    const promise = service.getByDateRange(from, to);
    httpMock.expectOne(() => true).flush('Server error', { status: 500, statusText: 'Internal Server Error' });
    await expect(promise).rejects.toBeDefined();
  });

  describe('createAppointment', () => {
    it('sends POST to /api/appointments with dto body and returns created dto', async () => {
      const dto: CreateAppointmentDto = {
        clientId: 'c1', therapistId: 't1',
        scheduledAt: '2026-06-01T09:00:00.000Z',
        durationMinutes: 50, type: 'InPerson',
      };
      const promise = service.createAppointment(dto);
      const req = httpMock.expectOne(`${environment.apiUrl}/appointments`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(dto);
      const result: AppointmentDto = {
        ...dto, id: 'new-id', tenantId: 't', status: 'Scheduled', createdAt: '',
      };
      req.flush(result, { status: 201, statusText: 'Created' });
      expect(await promise).toEqual(result);
    });

    it('rejects when server returns 422', async () => {
      const promise = service.createAppointment({
        clientId: '', therapistId: '', scheduledAt: '', durationMinutes: 0, type: 'InPerson',
      });
      const req = httpMock.expectOne(`${environment.apiUrl}/appointments`);
      req.flush({ errors: [] }, { status: 422, statusText: 'Unprocessable' });
      await expect(promise).rejects.toBeDefined();
    });
  });

  describe('confirmAppointment', () => {
    it('sends POST to /{id}/confirm', async () => {
      const promise = service.confirmAppointment('appt-1');
      const req = httpMock.expectOne(`${environment.apiUrl}/appointments/appt-1/confirm`);
      expect(req.request.method).toBe('POST');
      req.flush(null, { status: 204, statusText: 'No Content' });
      await expect(promise).resolves.toBeNull();
    });
  });

  describe('cancelAppointment', () => {
    it('sends POST to /{id}/cancel with reason', async () => {
      const promise = service.cancelAppointment('appt-1', 'Client request');
      const req = httpMock.expectOne(`${environment.apiUrl}/appointments/appt-1/cancel`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ reason: 'Client request' });
      req.flush(null, { status: 204, statusText: 'No Content' });
      await promise;
    });
  });

  describe('rescheduleAppointment', () => {
    it('sends POST with newScheduledAt only when duration omitted', async () => {
      const promise = service.rescheduleAppointment('appt-1', '2026-07-01T10:00:00.000Z');
      const req = httpMock.expectOne(`${environment.apiUrl}/appointments/appt-1/reschedule`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ newScheduledAt: '2026-07-01T10:00:00.000Z' });
      expect(req.request.body['newDurationMinutes']).toBeUndefined();
      req.flush(null, { status: 204, statusText: 'No Content' });
      await promise;
    });

    it('includes newDurationMinutes when provided', async () => {
      const promise = service.rescheduleAppointment('appt-1', '2026-07-01T10:00:00.000Z', 90);
      const req = httpMock.expectOne(`${environment.apiUrl}/appointments/appt-1/reschedule`);
      expect(req.request.body['newDurationMinutes']).toBe(90);
      req.flush(null, { status: 204, statusText: 'No Content' });
      await promise;
    });
  });
});
