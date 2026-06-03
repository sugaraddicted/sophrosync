import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Observable, forkJoin, of, throwError } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';
import { AuthService } from '../../../core/auth/auth.service';
import { ReportsService } from '../reports.service';
import {
  AppointmentSummaryDto,
  NoteCompletionRateDto,
  PracticeAnalyticsSummaryDto,
} from '../report.model';
import { StatCardComponent } from '../stat-card/stat-card.component';
import { AppointmentBreakdownTableComponent } from '../appointment-breakdown-table/appointment-breakdown-table.component';

@Component({
  selector: 'app-reports-page',
  standalone: true,
  imports: [FormsModule, StatCardComponent, AppointmentBreakdownTableComponent],
  templateUrl: './reports-page.component.html',
  styleUrl: './reports-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ReportsPageComponent implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly reportsService = inject(ReportsService);

  readonly from = signal<Date>(this.startOfMonth(new Date()));
  readonly to = signal<Date>(this.endOfMonth(new Date()));

  readonly appointmentSummary = signal<AppointmentSummaryDto | null>(null);
  readonly noteCompletion = signal<NoteCompletionRateDto | null>(null);
  readonly practiceAnalytics = signal<PracticeAnalyticsSummaryDto | null>(null);
  readonly isLoading = signal(false);
  readonly loadError = signal<string | null>(null);

  readonly isAdminOrSupervisor = computed(() => {
    const roles = this.auth.userRoles();
    return roles.includes('admin') || roles.includes('supervisor');
  });

  /** YYYY-MM-DD string for the from date input */
  readonly fromDateInput = computed(() => this.toDateInputValue(this.from()));

  /** YYYY-MM-DD string for the to date input */
  readonly toDateInput = computed(() => this.toDateInputValue(this.to()));

  readonly formattedPeriod = computed(() => {
    const opts: Intl.DateTimeFormatOptions = { year: 'numeric', month: 'long', day: 'numeric' };
    return `${this.from().toLocaleDateString('en-US', opts)} – ${this.to().toLocaleDateString('en-US', opts)}`;
  });

  readonly noteCompletionPercent = computed(() => {
    const nc = this.noteCompletion();
    if (!nc || nc.totalAppointments === 0) return 'N/A';
    return `${(nc.completionRate * 100).toFixed(1)}%`;
  });

  ngOnInit(): void {
    this.loadData();
  }

  onFromChange(value: string): void {
    const d = new Date(value);
    if (!isNaN(d.getTime())) {
      this.from.set(d);
    }
  }

  onToChange(value: string): void {
    const d = new Date(value);
    if (!isNaN(d.getTime())) {
      this.to.set(d);
    }
  }

  loadData(): void {
    this.isLoading.set(true);
    this.loadError.set(null);

    const appt$ = this.reportsService.getAppointmentSummary(this.from(), this.to());
    const notes$ = this.reportsService.getNoteCompletionRate(this.from(), this.to());
    const analytics$: Observable<PracticeAnalyticsSummaryDto | null> = this.isAdminOrSupervisor()
      ? this.reportsService.getPracticeAnalytics(this.from(), this.to()).pipe(
          catchError((e: { status: number }) =>
            e.status === 403 ? of(null) : throwError(() => e)
          )
        )
      : of(null);

    forkJoin([appt$, notes$, analytics$] as const)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: ([appt, notes, analytics]) => {
          this.appointmentSummary.set(appt);
          this.noteCompletion.set(notes);
          this.practiceAnalytics.set(analytics);
        },
        error: () => this.loadError.set('Failed to load reports. Please try again.'),
      });
  }

  private startOfMonth(d: Date): Date {
    return new Date(d.getFullYear(), d.getMonth(), 1, 0, 0, 0, 0);
  }

  private endOfMonth(d: Date): Date {
    return new Date(d.getFullYear(), d.getMonth() + 1, 0, 23, 59, 59, 999);
  }

  private toDateInputValue(d: Date): string {
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }
}
