import { TestBed } from '@angular/core/testing';
import { AppointmentDetailModalComponent, AppointmentAction } from './appointment-detail-modal.component';
import { AppointmentDto } from '../../dashboard/appointments.service';

const makeAppt = (status = 'Scheduled'): AppointmentDto => ({
  id: 'appt-1', tenantId: 't', clientId: 'c1', therapistId: 'th1',
  scheduledAt: '2026-06-01T09:00:00.000Z',
  durationMinutes: 50, type: 'InPerson', status,
  notes: undefined, cancellationReason: undefined, createdAt: '',
});

describe('AppointmentDetailModalComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppointmentDetailModalComponent],
    }).compileComponents();
  });

  it('creates the component', () => {
    const fixture = TestBed.createComponent(AppointmentDetailModalComponent);
    fixture.componentRef.setInput('appointment', makeAppt());
    fixture.detectChanges();
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('Scheduled status — isEditable and canConfirm are true', () => {
    const fixture = TestBed.createComponent(AppointmentDetailModalComponent);
    fixture.componentRef.setInput('appointment', makeAppt('Scheduled'));
    fixture.detectChanges();
    expect(fixture.componentInstance.isEditable()).toBe(true);
    expect(fixture.componentInstance.canConfirm()).toBe(true);
  });

  it('Confirmed status — isEditable true, canConfirm false', () => {
    const fixture = TestBed.createComponent(AppointmentDetailModalComponent);
    fixture.componentRef.setInput('appointment', makeAppt('Confirmed'));
    fixture.detectChanges();
    expect(fixture.componentInstance.isEditable()).toBe(true);
    expect(fixture.componentInstance.canConfirm()).toBe(false);
  });

  it('Completed status — isEditable false', () => {
    const fixture = TestBed.createComponent(AppointmentDetailModalComponent);
    fixture.componentRef.setInput('appointment', makeAppt('Completed'));
    fixture.detectChanges();
    expect(fixture.componentInstance.isEditable()).toBe(false);
  });

  it('click Cancel sets pendingAction to cancel', () => {
    const fixture = TestBed.createComponent(AppointmentDetailModalComponent);
    fixture.componentRef.setInput('appointment', makeAppt());
    fixture.detectChanges();
    fixture.componentInstance.onStartCancel();
    expect(fixture.componentInstance.pendingAction()).toBe('cancel');
  });

  it('onConfirm emits confirm action', () => {
    const fixture = TestBed.createComponent(AppointmentDetailModalComponent);
    fixture.componentRef.setInput('appointment', makeAppt());
    fixture.detectChanges();
    const actions: AppointmentAction[] = [];
    fixture.componentInstance.actionRequested.subscribe(a => actions.push(a));
    fixture.componentInstance.onConfirm();
    expect(actions).toHaveLength(1);
    expect(actions[0]).toEqual({ type: 'confirm' });
  });

  it('onConfirmCancel emits cancel action with reason', () => {
    const fixture = TestBed.createComponent(AppointmentDetailModalComponent);
    fixture.componentRef.setInput('appointment', makeAppt());
    fixture.detectChanges();
    const actions: AppointmentAction[] = [];
    fixture.componentInstance.actionRequested.subscribe(a => actions.push(a));
    fixture.componentInstance.pendingAction.set('cancel');
    fixture.componentInstance.cancelReason.set('test reason');
    fixture.componentInstance.onConfirmCancel();
    expect(actions).toHaveLength(1);
    expect(actions[0]).toEqual({ type: 'cancel', reason: 'test reason' });
  });

  it('click Reschedule sets pendingAction to reschedule', () => {
    const fixture = TestBed.createComponent(AppointmentDetailModalComponent);
    fixture.componentRef.setInput('appointment', makeAppt());
    fixture.detectChanges();
    fixture.componentInstance.onStartReschedule();
    expect(fixture.componentInstance.pendingAction()).toBe('reschedule');
  });

  it('onConfirmReschedule emits reschedule action with ISO date', () => {
    const fixture = TestBed.createComponent(AppointmentDetailModalComponent);
    fixture.componentRef.setInput('appointment', makeAppt());
    fixture.detectChanges();
    const actions: AppointmentAction[] = [];
    fixture.componentInstance.actionRequested.subscribe(a => actions.push(a));
    fixture.componentInstance.pendingAction.set('reschedule');
    fixture.componentInstance.rescheduleDate.set('2026-07-01T10:00');
    fixture.componentInstance.onConfirmReschedule();
    expect(actions).toHaveLength(1);
    expect(actions[0].type).toBe('reschedule');
    if (actions[0].type === 'reschedule') {
      expect(actions[0].newScheduledAt).toBe(new Date('2026-07-01T10:00').toISOString());
    }
  });
});
